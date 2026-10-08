using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using YsEdu.Core.Data;
using YsEdu.Core.Data.Entities;
using YsEdu.Core.Logging;
using YsEdu.Core.Messaging;

namespace YsEdu.Core.Services;

/// 메시지 1건 처리 결과
public sealed record ProcessOutcome(ProductionEventResponse Response, string ResponseXml, bool Applied, bool Duplicate, string Detail);

/// <summary>
/// 미들웨어 업무 처리 — 메시지 1건 (V2 p.13 "메시지 수신부터 조회까지")
///   ① 수신·해석 : XML 파싱                      → 구조 오류 ERROR
///   ② 값 검증   : 필수값·시간대·이벤트 종류       → ERROR
///   ③ 중복 확인 : 이미 반영한 MessageName         → 이전 결과로 응답, 다시 반영하지 않음
///   ④ 기준 조회 : 제품·STEP·설비 등록 여부        → 미등록 ERROR
///   ⑤ 업무 처리 : LOT/PROCESS 이력 또는 RMS 판정 — 하나의 트랜잭션 (ERROR면 롤백)
///   ⑥ 저장·응답 : 송수신 원문 저장 + 응답 생성 (Kafka 전송은 MiddlewareWorker)
/// "DB 실패를 OK로 응답하지 않는다." 실패해도 다음 메시지 처리는 계속한다.
/// </summary>
public sealed class MessageProcessor
{
    private readonly DbFactory _db;
    private readonly FileLog _log;
    private readonly LotEventHandler _lots = new();
    private readonly RmsValidator _rms = new();

    public MessageProcessor(DbFactory db, FileLog log)
    {
        _db = db;
        _log = log;
    }

    public ProcessOutcome Process(string rawXml, string topic, int partition, long offset)
    {
        // ① 수신·해석
        var e = XmlMessage.TryParse(rawXml, out var parseError);
        if (e == null)
            return Finish(rawXml, topic, partition, offset, null, Results.ERROR, Reasons.XmlFormatError, false, false, parseError);

        // ② 값 검증
        var errors = XmlMessage.Validate(e);
        if (errors.Count > 0)
            return Finish(rawXml, topic, partition, offset, e, Results.ERROR, string.Join(",", errors), false, false, "값 검증 실패");

        HandleResult handled;
        try
        {
            using var db = _db.Create();

            // ③ 중복 확인: 이미 반영한 MessageName이면 다시 반영하지 않고 처음 결과를 돌려준다
            var processed = db.TbMsgProcesseds.AsNoTracking().FirstOrDefault(m => m.MessageId == e.MessageName);
            if (processed != null)
            {
                string reason = string.IsNullOrEmpty(processed.Reason)
                    ? Reasons.DuplicateMessage
                    : $"{Reasons.DuplicateMessage},{processed.Reason}";
                return Finish(rawXml, topic, partition, offset, e, processed.Result, reason, false, true,
                              $"이미 반영된 MessageName (최초 처리 {processed.ProcessedAt:HH:mm:ss.fff}) — 반영하지 않음");
            }

            // ④ 기준 조회: 등록된 제품 · STEP · 설비인가
            string? unregistered =
                !db.TbProducts.Any(p => p.ProductId == e.ProductId && p.UseYn == "Y") ? Reasons.UnregisteredProduct
                : !db.TbSteps.Any(s => s.StepId == e.StepId && s.UseYn == "Y") ? Reasons.UnregisteredStep
                : !db.TbEquipments.Any(q => q.EqpId == e.EqpId && q.UseYn == "Y") ? Reasons.UnregisteredEquipment
                : null;
            if (unregistered != null)
                return Finish(rawXml, topic, partition, offset, e, Results.ERROR, unregistered, false, false, "기준정보 미등록");

            // ⑤ 업무 처리 — 상태와 관련 이력은 함께 성공해야 한다
            using var tx = db.Database.BeginTransaction();
            handled = e.EventType == EventTypes.RmsCheck ? _rms.Validate(db, e) : _lots.Handle(db, e);

            if (handled.Result == Results.ERROR)
            {
                tx.Rollback();                  // 성공 처리 방지: 아무것도 반영하지 않음
            }
            else
            {
                db.TbLotEvents.Add(new TbLotEvent
                {
                    MessageId = e.MessageName, EventType = e.EventType, ProductId = e.ProductId, LotId = e.LotId,
                    StepId = e.StepId, EqpId = e.EqpId, RecipeId = e.RecipeId, EventTime = e.EventTime,
                    Result = handled.Result, Reason = handled.Reason, RunId = handled.RunId, CheckId = handled.CheckId
                });
                db.TbMsgProcesseds.Add(new TbMsgProcessed
                {
                    MessageId = e.MessageName, EventType = e.EventType, EqpId = e.EqpId, Result = handled.Result, Reason = handled.Reason
                });
                db.SaveChanges();
                tx.Commit();
            }
        }
        catch (Exception ex) when (ex is DbUpdateException or SqlException or InvalidOperationException)
        {
            // DB 실패를 OK로 응답하지 않는다 → ERROR. 원인은 파일에 남긴다
            _log.Error($"[DB 오류] {e.MessageName} {e.EventType} {e.EqpId}: {ex.GetBaseException().Message}");
            return Finish(rawXml, topic, partition, offset, e, Results.ERROR, Reasons.DbError, false, false, ex.GetBaseException().Message);
        }

        bool applied = handled.Result != Results.ERROR;
        return Finish(rawXml, topic, partition, offset, e, handled.Result, handled.Reason, applied, false,
                      applied ? "반영 완료" : "업무 검증 실패 — 반영하지 않음");
    }

    /// ⑥ 응답 생성 + 송수신 원문 저장 (DB에 못 남기면 파일에 남긴다)
    private ProcessOutcome Finish(string rawXml, string topic, int partition, long offset, ProductionEvent? e,
                                  string result, string reason, bool applied, bool duplicate, string detail)
    {
        var response = new ProductionEventResponse(e?.MessageName ?? "", e?.EventType ?? "", e?.LotId ?? "", e?.EqpId ?? "",
                                                   result, reason, DateTimeOffset.Now);
        string responseXml = XmlMessage.Build(response);

        try
        {
            using var db = _db.Create();
            db.TbMsgLogs.Add(new TbMsgLog
            {
                Direction = "IN", Topic = topic, PartitionNo = partition, OffsetNo = offset,
                MessageId = Cut(e?.MessageName), EventType = Cut(e?.EventType, 20), EqpId = Cut(e?.EqpId), LotId = Cut(e?.LotId),
                RawXml = rawXml, Result = result, Reason = Cut($"{reason} {detail}".Trim(), 400)
            });
            db.TbMsgLogs.Add(new TbMsgLog
            {
                Direction = "OUT", Topic = "RESPONSE",
                MessageId = Cut(e?.MessageName), EventType = Cut(e?.EventType, 20), EqpId = Cut(e?.EqpId), LotId = Cut(e?.LotId),
                RawXml = responseXml, Result = result, Reason = Cut(reason, 400)
            });
            db.SaveChanges();
        }
        catch (Exception ex) when (ex is DbUpdateException or SqlException or InvalidOperationException)
        {
            _log.Error($"[원문 저장 실패] {e?.MessageName ?? "?"} {result} {reason}: {ex.GetBaseException().Message}{Environment.NewLine}{rawXml}");
        }

        return new ProcessOutcome(response, responseXml, applied, duplicate, detail);
    }

    /// 원문 로그 컬럼 길이를 넘는 값(DB 실패를 일으킨 값 등)도 로그에는 남을 수 있게 자른다
    private static string? Cut(string? value, int max = 50)
        => value == null ? null : value.Length <= max ? value : value[..max];
}
