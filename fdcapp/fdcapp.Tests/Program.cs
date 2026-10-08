using fdcapp.Data;
using fdcapp.Dtos;
using fdcapp.Models;
using fdcapp.Services;
using Microsoft.EntityFrameworkCore;

// ============================================================================
// fdcapp 서비스 단위 기능 검증 (구축 연습 p.6 "UI 연결 전에 Console로 메서드 단독 호출 정상 동작 확인")
//   실제 DB(TrainingDB)를 사용한다. 시험 데이터는 TST_ 로 시작하고, 시작·종료 시 정리한다.
// ============================================================================
Console.OutputEncoding = System.Text.Encoding.UTF8;
int pass = 0, fail = 0;

void Check(bool ok, string text)
{
    if (ok) pass++; else fail++;
    Console.WriteLine($"{(ok ? "  ✔" : "  ✘")} {text}");
}

void Expect(Action action, string text)
{
    try
    {
        action();
        Check(false, $"{text} — 예외가 나야 하는데 통과됨");
    }
    catch (InvalidOperationException ex)
    {
        Check(true, $"{text} → \"{ex.Message}\"");
    }
}

static void Cleanup()
{
    using var db = new AppDbContext();
    db.Database.ExecuteSqlRaw("DELETE FROM dbo.TB_EQUIP_DATA  WHERE EQUIP_ID LIKE N'TST[_]%'");
    db.Database.ExecuteSqlRaw("DELETE FROM dbo.TB_EQUIP_LOG   WHERE EQUIP_ID LIKE N'TST[_]%'");
    db.Database.ExecuteSqlRaw("DELETE FROM dbo.TB_PARAM_LIMIT WHERE EQUIP_ID LIKE N'TST[_]%'");
    db.Database.ExecuteSqlRaw("DELETE FROM dbo.TB_EQUIPMENT   WHERE EQUIP_ID LIKE N'TST[_]%'");
}

// 매번 새 DbContext로 DB에 실제 저장된 값을 읽는다 (서비스의 추적 상태와 섞이지 않게)
static T Read<T>(Func<AppDbContext, T> query)
{
    using var db = new AppDbContext();
    return query(db);
}

Cleanup();
var service = new EquipmentService(new AppDbContext());

// ---------------------------------------------------------------- 구축 연습
Console.WriteLine("■ 구축 연습 — 설비 등록 · 조회");
service.AddEquipment(new Equipment { EquipId = "TST_EQP_1", EquipName = "시험 설비 1", LineName = "LINE_A", Status = "IDLE" });
service.AddEquipment(new Equipment { EquipId = "TST_EQP_2", EquipName = "시험 설비 2", LineName = "LINE_B", Status = "IDLE" });
Check(service.GetEquipmentList().Count(e => e.EquipId.StartsWith("TST_")) == 2, "AddEquipment 2건 → GetEquipmentList에 2건");
Check(Read(db => db.Equipments.Single(e => e.EquipId == "TST_EQP_1").CreateDt) != null, "등록 시 CREATE_DT 자동 기록");
Expect(() => service.AddEquipment(new Equipment { EquipId = "TST_EQP_1", EquipName = "중복", LineName = "LINE_A" }),
       "중복 설비 ID 등록");

Console.WriteLine("\n■ 구축 연습 — 상태 변경 트랜잭션");
service.UpdateEquipmentStatus("TST_EQP_1", "RUN");
Check(Read(db => db.Equipments.Single(e => e.EquipId == "TST_EQP_1").Status) == "RUN", "TB_EQUIPMENT.STATUS IDLE → RUN (UPDATE)");
var statusLog = Read(db => db.EquipLogs.Where(l => l.EquipId == "TST_EQP_1" && l.LogType == "STATUS_CHG").ToList());
Check(statusLog.Count == 1 && statusLog[0].LogMsg == "상태 변경 [IDLE ➔ RUN]",
      $"TB_EQUIP_LOG에 STATUS_CHG 자동 추가 (INSERT): \"{statusLog.FirstOrDefault()?.LogMsg}\"");
Expect(() => service.UpdateEquipmentStatus("TST_NONE", "RUN"), "없는 설비 상태 변경");

Thread.Sleep(20);
service.UpdateEquipmentStatus("TST_EQP_1", "IDLE");
var logs = service.GetLogsByEquipId("TST_EQP_1");
Check(logs.Count == 2 && logs[0].LogMsg == "상태 변경 [RUN ➔ IDLE]", "GetLogsByEquipId — 최신 로그가 먼저 (역순 정렬)");

// ---------------------------------------------------------------- FDC 과제 1
Console.WriteLine("\n■ FDC 과제 1 — 임계치 판정 (CheckParameterAlarm)");
using (var db = new AppDbContext())
{
    db.ParamLimits.AddRange(
        new ParamLimit { EquipId = "TST_EQP_1", ParamName = "TEMP", LowerLimit = 20.0, UpperLimit = 80.0 },
        new ParamLimit { EquipId = "TST_EQP_1", ParamName = "PRESS", LowerLimit = 1.0, UpperLimit = 5.0 });
    db.SaveChanges();
}

FdcResult normal = service.CheckParameterAlarm("TST_EQP_1", "TEMP", 50.0);
Check(!normal.IsAlarm && Read(db => db.Equipments.Single(e => e.EquipId == "TST_EQP_1").Status) == "IDLE",
      $"정상(50℃) → 상태 유지: \"{normal.Message}\"");
Check(service.CheckParameterAlarm("TST_EQP_1", "TEMP", 80.0).IsAlarm == false, "경계값(80℃ = 상한) → 정상");

FdcResult upper = service.CheckParameterAlarm("TST_EQP_1", "TEMP", 85.4);
Check(upper.IsAlarm && upper.Message == "온도 상한(80℃) 초과 발생: 85.4℃", $"상한 초과(85.4℃) → \"{upper.Message}\"");
Check(Read(db => db.Equipments.Single(e => e.EquipId == "TST_EQP_1").Status) == "DOWN", "이탈 시 TB_EQUIPMENT.STATUS → DOWN");
Check(Read(db => db.EquipLogs.Any(l => l.EquipId == "TST_EQP_1" && l.LogType == "ALARM" && l.LogMsg == upper.Message)),
      "이탈 시 TB_EQUIP_LOG에 ALARM 로그 추가");

FdcResult lower = service.CheckParameterAlarm("TST_EQP_1", "PRESS", 0.5);
Check(lower.IsAlarm && lower.Message == "압력 하한(1bar) 미달 발생: 0.5bar", $"하한 미달(PRESS 0.5) → \"{lower.Message}\"");

Expect(() => service.CheckParameterAlarm("TST_EQP_2", "TEMP", 50.0), "규칙 없는 설비 판정 (정상으로 오판하지 않음)");
Check(Read(db => db.EquipLogs.Any(l => l.EquipId == "TST_EQP_2" && l.LogType == "NO_RULE")), "규칙 미설정 시 NO_RULE 로그 기록");

// ---------------------------------------------------------------- FDC 과제 2
Console.WriteLine("\n■ FDC 과제 2 — 센서 수집 (CollectSensorData)");
service.UpdateEquipmentStatus("TST_EQP_1", "RUN");
FdcResult collectNormal = service.CollectSensorData("TST_EQP_1", 40.0, 3.0);
var lastRow = Read(db => db.EquipDatas.Where(d => d.EquipId == "TST_EQP_1").OrderByDescending(d => d.DataId).First());
Check(!collectNormal.IsAlarm && lastRow.TempVal == 40.0 && lastRow.PressVal == 3.0 && lastRow.IsFault == "NORMAL",
      "정상값 → TB_EQUIP_DATA INSERT, IS_FAULT = NORMAL");

FdcResult collectTemp = service.CollectSensorData("TST_EQP_1", 90.0, 3.0);
lastRow = Read(db => db.EquipDatas.Where(d => d.EquipId == "TST_EQP_1").OrderByDescending(d => d.DataId).First());
Check(collectTemp.IsAlarm && lastRow.IsFault == "ALARM" && Read(db => db.Equipments.Single(e => e.EquipId == "TST_EQP_1").Status) == "DOWN",
      $"온도 80℃ 초과 → IS_FAULT = ALARM, 설비 DOWN: \"{collectTemp.Message}\"");

FdcResult collectPress = service.CollectSensorData("TST_EQP_1", 40.0, 5.8);
Check(collectPress.IsAlarm && collectPress.Message.StartsWith("압력 상한"), $"압력 상한 초과도 ALARM: \"{collectPress.Message}\"");

int dataBefore = Read(db => db.EquipDatas.Count(d => d.EquipId == "TST_EQP_2"));
Expect(() => service.CollectSensorData("TST_EQP_2", 40.0, 3.0), "규칙 없는 설비 수집");
Check(Read(db => db.EquipDatas.Count(d => d.EquipId == "TST_EQP_2")) == dataBefore, "규칙이 없으면 수집 행도 저장하지 않음");

// ---------------------------------------------------------------- FDC 과제 2·3 — 멀티스레드
Console.WriteLine("\n■ FDC 과제 2 — 백그라운드 시뮬레이터 (Task.Run · CancellationToken · 1.5초 주기)");
service.UpdateEquipmentStatus("TST_EQP_1", "RUN");
int rowsBefore = Read(db => db.EquipDatas.Count(d => d.EquipId == "TST_EQP_1"));
var cycles = new List<SensorCycleResult>();
var simulator = new SensorSimulator(service);
using (var cts = new CancellationTokenSource())
{
    var loop = Task.Run(() => simulator.RunCollector("TST_EQP_1", cts.Token, c => { lock (cycles) cycles.Add(c); }), cts.Token);

    // 수집이 도는 동안 UI 스레드 역할로 같은 서비스를 동시에 호출 (DbContext 동시 사용 방지 확인)
    var uiError = (Exception?)null;
    var watch = System.Diagnostics.Stopwatch.StartNew();
    while (watch.Elapsed < TimeSpan.FromSeconds(4.2))
    {
        try
        {
            service.GetEquipmentList();
            service.GetLogsByEquipId("TST_EQP_1");
        }
        catch (Exception ex)
        {
            uiError = ex;
            break;
        }
        Thread.Sleep(10);
    }

    cts.Cancel();
    bool stopped = loop.Wait(TimeSpan.FromSeconds(3));
    Check(uiError == null, $"수집 중 화면 쪽 조회를 동시에 호출해도 DbContext 오류 없음{(uiError == null ? "" : " — " + uiError.Message)}");
    Check(stopped, "Cancel() 호출 → 수집 루프 안전 종료");
}

int rowsAdded = Read(db => db.EquipDatas.Count(d => d.EquipId == "TST_EQP_1")) - rowsBefore;
Check(cycles.Count >= 3 && cycles.Count <= 4 && rowsAdded == cycles.Count,
      $"4.2초 동안 1.5초 주기로 {cycles.Count}회 수집, TB_EQUIP_DATA {rowsAdded}행 INSERT");
Check(cycles.All(c => c.Error == null && c.TempVal is >= 20 and <= 95 && c.PressVal is >= 1 and <= 6),
      "가상 온도 20~95℃, 압력 1~6bar 범위로 생성");

Cleanup();
Console.WriteLine($"\nRESULT: {(fail == 0 ? "PASS" : "FAIL")}  (통과 {pass} / 실패 {fail})");
return fail == 0 ? 0 : 1;
