using Microsoft.EntityFrameworkCore;
using YsEdu.Core.Config;
using YsEdu.Core.Data;
using YsEdu.Core.Logging;
using YsEdu.Core.Messaging;
using YsEdu.Core.Services;
using YsEdu.Core.Simulation;

// ============================================================================
// 2차 과제 p.12 "완료 확인 시나리오" 자동 검증
//   실제 Kafka(localhost:9092)와 DB(YsEduLotDB)를 사용한다.
//   미들웨어는 이 프로그램 안에서 MiddlewareWorker로 함께 실행한다.
// ============================================================================
Console.OutputEncoding = System.Text.Encoding.UTF8;
var settings = AppSettings.Load();
var factory = new DbFactory(settings.ConnectionString);
var log = new FileLog(settings.LogDirectory, "ScenarioTest");
int pass = 0, fail = 0;

void Check(bool ok, string text)
{
    if (ok) pass++; else fail++;
    Console.WriteLine($"{(ok ? "  ✔" : "  ✘")} {text}");
}

Console.WriteLine("이력 초기화 (기준정보 유지)");
await HistoryReset.RunAsync(factory);

await using var worker = new MiddlewareWorker(settings, log);
worker.Start();
await using var client = new EquipmentClient(settings);
await client.StartAsync();
var sim = new InlineSimulator(client);

async Task Run(string title, Func<Task<List<ScenarioStep>>> scenario)
{
    Console.WriteLine($"\n■ {title}");
    foreach (var s in await scenario())
        Check(s.Pass, $"{s.Title,-34} 기대 {s.ExpectedResult} {s.ExpectedReason} / 실제 {s.Actual}");
}

await Run("1. 정상 인라인 (LOT_001)", () => sim.NormalInlineAsync("LOT_001"));
await Run("2. 중간 설비 — B 자체 LOT_START 없이 PROCESS (LOT_002)", () => sim.MiddleEquipmentAsync("LOT_002"));
await Run("3. RMS 불일치 — SPEED=500 → 450 재검증 (LOT_003)", () => sim.RmsMismatchAsync("LOT_003"));
await Run("4. 구성 오류 — 누락 / 추가 / 중복 (LOT_004)", () => sim.StructureErrorsAsync("LOT_004"));
await Run("5. 순서 · 중복 (LOT_005)", () => sim.OrderAndDuplicateAsync("LOT_005"));
await Run("6. 처리 오류 (LOT_006)", () => sim.ProcessingErrorsAsync("LOT_006"));

Console.WriteLine("\n■ DB 저장 결과 확인");
await using (var db = factory.Create())
{
    // 1. 정상 인라인
    var lot1 = await db.TbLots.FindAsync("LOT_001");
    Check(lot1?.LotStatus == "DONE" && lot1.StartTime != null && lot1.EndTime != null, $"TB_LOT LOT_001 작업 상태 DONE, 시작·종료 시간 저장 ({lot1?.LotStatus})");
    var runs1 = await db.TbEqpRuns.Where(r => r.LotId == "LOT_001").OrderBy(r => r.RunId).ToListAsync();
    Check(runs1.Count == 3 && runs1.All(r => r.RunStatus == "DONE" && r.EndTime != null && r.EndMsgId != null),
          $"TB_EQP_RUN LOT_001 설비별 구동 3건, START·END가 한 행으로 연결 ({string.Join(",", runs1.Select(r => r.EqpId + ":" + r.RunStatus))})");
    Check(runs1.All(r => r.CheckId != null), "각 구동이 직전 OK RMS 검증(CHECK_ID)과 연결");
    var ev1 = await db.TbLotEvents.Where(e => e.LotId == "LOT_001").ToListAsync();
    Check(ev1.Count == 11, $"TB_LOT_EVENT LOT_001 = LOT_START 1 + RMS_CHECK 3 + PROCESS 6 + LOT_END 1 = 11건 (실제 {ev1.Count})");
    Check(ev1.Count(e => e.EventType == EventTypes.LotStart) == 1 && ev1.Single(e => e.EventType == EventTypes.LotStart).EqpId == "EQP_A"
          && ev1.Single(e => e.EventType == EventTypes.LotEnd).EqpId == "EQP_C", "LOT_START는 A, LOT_END는 C에서만");

    // 2. 중간 설비
    var lot2 = await db.TbLots.FindAsync("LOT_002");
    var run2 = await db.TbEqpRuns.SingleOrDefaultAsync(r => r.LotId == "LOT_002");
    Check(lot2 != null && lot2.StartTime == null && run2?.EqpId == "EQP_B" && run2.RunStatus == "DONE",
          "LOT_002: LOT_START 없이 EQP_B 구동 이력 정상 저장 (LOT 시작 시간은 비어 있음)");

    // 3. RMS 불일치 → 재검증
    var checks3 = await db.TbRmsChecks.Include(c => c.TbRmsCheckDtls).Where(c => c.LotId == "LOT_003").OrderBy(c => c.CheckId).ToListAsync();
    Check(checks3.Count == 2 && checks3[0].Result == "NG" && checks3[1].Result == "OK" && checks3[0].MessageId != checks3[1].MessageId,
          $"RMS 검증 2건 NG → OK, 서로 다른 MessageName ({string.Join(" → ", checks3.Select(c => c.Result))})");
    var speed = checks3.FirstOrDefault()?.TbRmsCheckDtls.FirstOrDefault(d => d.ItemId == "SPEED");
    Check(speed is { BaseValue: "450", RecvValue: "500", Judge: "NG", Reason: "SPEED_MISMATCH" },
          $"상세: SPEED 기준값 450 / 수신값 500 / NG / SPEED_MISMATCH ({speed?.BaseValue}/{speed?.RecvValue}/{speed?.Judge}/{speed?.Reason})");
    var run3 = await db.TbEqpRuns.SingleOrDefaultAsync(r => r.LotId == "LOT_003");
    Check(run3 != null && run3.CheckId == checks3.LastOrDefault()?.CheckId, "구동은 재검증 OK 이후 1건, OK 판정 CHECK_ID와 연결");

    // 4. 구성 오류
    var checks4 = await db.TbRmsChecks.Where(c => c.LotId == "LOT_004").ToListAsync();
    Check(checks4.Count == 4 && checks4.All(c => c.Result == "NG"), $"구성 오류 4건 모두 NG 저장 ({string.Join(" / ", checks4.Select(c => c.Reason))})");
    Check(!await db.TbEqpRuns.AnyAsync(r => r.LotId == "LOT_004"), "NG 이후 구동 이력 없음 (구동 보류)");

    // 5. 순서 · 중복
    Check(!await db.TbEqpRuns.AnyAsync(r => r.LotId == "LOT_005"), "START 없는 END → 구동 이력 없음");
    var dupName = (await db.TbRmsChecks.SingleAsync(c => c.LotId == "LOT_005")).MessageId;
    int inCount = await db.TbMsgLogs.CountAsync(m => m.MessageId == dupName && m.Direction == "IN");
    Check(inCount == 2 && await db.TbRmsChecks.CountAsync(c => c.MessageId == dupName) == 1
          && await db.TbLotEvents.CountAsync(e => e.MessageId == dupName) == 1,
          $"같은 MessageName 2회 수신(원문 {inCount}건) → 검증·이벤트는 1건만 반영");

    // 6. 처리 오류
    var errors6 = await db.TbMsgLogs.Where(m => m.Direction == "IN" && m.Result == "ERROR").Select(m => m.Reason).ToListAsync();
    Check(errors6.Any(r => r!.StartsWith(Reasons.XmlFormatError)) && errors6.Any(r => r!.StartsWith(Reasons.UnregisteredEquipment))
          && errors6.Any(r => r!.StartsWith(Reasons.DbError)) && errors6.Any(r => r!.StartsWith(Reasons.NoProcessStart)),
          "ERROR 원문과 사유가 TB_MSG_LOG에 저장 (XML 형식 / 미등록 설비 / DB 실패 / 순서 오류)");
    Check(!await db.TbLots.AnyAsync(l => l.LotId.StartsWith("LLLL")), "DB 실패 메시지는 반영되지 않음");
    Check(!await db.TbLotEvents.AnyAsync(e => e.Result == "ERROR"), "ERROR 판정은 이력에 반영하지 않음");

    var eqps = await db.TbEquipments.ToListAsync();
    Check(eqps.All(q => q.EqpStatus == "IDLE" && q.CurLotId == null), "모든 설비 구동 종료 후 IDLE");
}

await worker.StopAsync();
Check(!worker.IsRunning, "중지 시 수신 작업 종료 확인");

Console.WriteLine($"\nRESULT: {(fail == 0 ? "PASS" : "FAIL")}  (통과 {pass} / 실패 {fail})");
return fail == 0 ? 0 : 1;
