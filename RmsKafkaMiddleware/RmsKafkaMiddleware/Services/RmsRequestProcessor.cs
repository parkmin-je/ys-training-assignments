using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RmsKafkaMiddleware.Common;
using RmsKafkaMiddleware.Data.Entities;
using RmsKafkaMiddleware.Messaging;

namespace RmsKafkaMiddleware.Services;

/// 메시지 1건 처리 결과
public sealed record ProcessOutcome(string ResultCode, string ResultMessage, string? EqpId, string? RecipeId, long? ResultId, string ResponseXml);

/// <summary>
/// RMS_TEST 요청 1건 처리 (Kafka p.4 흐름: 요청 수신 → XML 노드 파싱 → 레시피 조회 → 응답 XML 전송 → 결과 DB 저장)
///   02 파싱          : XML 형식 오류 / 필수 노드 누락 확인
///      Rule 확인     : RULE_NAME이 RMS_TEST인지 (Kafka p.5)
///      중복 확인     : 같은 요청(MESSAGE_KEY)이 이미 SUCCESS로 처리됐으면 다시 저장하지 않음
///   03 레시피 조회   : EQP_ID · FACTOR_ID · FACTOR_VALUE → RECIPE_ID → 파라미터 목록 (Kafka p.7)
///   05 결과 저장     : TB_RMS_RESULT 1건 + TB_RMS_RESULT_PARAMETER N건 + 처리 이력 — 한 트랜잭션
///   04 응답 XML      : 저장까지 성공했을 때만 SUCCESS. 저장에 실패하면 DB_ERROR로 응답한다
///                     (응답 전송은 KafkaWorker가 이 결과의 ResponseXml로 한다)
/// 정상·오류 모두 TB_RMS_REQUEST_LOG에 요청·응답 원문을 남긴다. DB에도 못 남기면 파일 로그.
/// </summary>
public sealed class RmsRequestProcessor(AppSettings settings, FileLog log)
{
    public ProcessOutcome Process(string requestXml, string topic, int partition, long offset)
    {
        // 02 XML 노드 파싱
        var parsed = RmsXml.Parse(requestXml);
        if (parsed.Request == null)
            return Fail(parsed.ErrorCode, parsed.ErrorMessage, parsed.RuleName, parsed.EventTime, parsed.EqpId, null, requestXml, topic, partition, offset);

        var req = parsed.Request;

        // Rule 확인
        if (!string.Equals(req.RuleName, settings.SupportedRule, StringComparison.Ordinal))
            return Fail(ResultCodes.RuleNotSupported, $"지원하지 않는 Rule입니다: {req.RuleName} (지원: {settings.SupportedRule})",
                        req.RuleName, req.EventTimeText, req.EqpId, req.MessageKey, requestXml, topic, partition, offset);

        try
        {
            using var db = settings.CreateDbContext();

            // 중복 확인 — 같은 요청을 이미 정상 처리했으면 저장하지 않고 처음 결과를 다시 알려 준다
            var previous = db.TbRmsRequestLogs.AsNoTracking()
                .Where(l => l.MessageKey == req.MessageKey && l.ResultCode == ResultCodes.Success)
                .Select(l => l.ResultId)
                .FirstOrDefault();
            if (previous != null)
            {
                var old = db.TbRmsResults.AsNoTracking().Include(r => r.TbRmsResultParameters).First(r => r.ResultId == previous);
                var dupResponse = new RmsResponse(req.RuleName, ResultCodes.Duplicate, $"이미 처리된 요청입니다 (RESULT_ID={old.ResultId}). 다시 저장하지 않았습니다.",
                    req.EventTimeText, req.EqpId, old.RecipeId,
                    old.TbRmsResultParameters.OrderBy(p => p.ParameterId).Select(p => new RmsParameter(p.ParameterId, p.ParameterValue)).ToList());
                return Log(db, dupResponse, old.ResultId, req.MessageKey, requestXml, topic, partition, offset);
            }

            // 03 레시피 조회: EQP_ID · FACTOR_ID · FACTOR_VALUE → RECIPE_ID (사용 중인 설비·레시피만)
            var recipe = db.TbRmsRecipes.AsNoTracking()
                .Where(r => r.EquipId == req.EqpId && r.FactorId == req.FactorId && r.FactorValue == req.FactorValue
                            && r.UseYn == "Y" && r.Equip.UseYn == "Y")
                .Select(r => new { r.RecipeId })
                .FirstOrDefault();
            if (recipe == null)
                return Fail(ResultCodes.RecipeNotFound,
                            $"조회 결과 없음: EQP_ID={req.EqpId}, FACTOR_ID={req.FactorId}, FACTOR_VALUE={req.FactorValue}에 해당하는 레시피가 없습니다.",
                            req.RuleName, req.EventTimeText, req.EqpId, req.MessageKey, requestXml, topic, partition, offset);

            // 파라미터 목록 조회
            var parameters = db.TbRmsRecipeParameters.AsNoTracking()
                .Where(p => p.RecipeId == recipe.RecipeId)
                .OrderBy(p => p.ParameterId)
                .Select(p => new RmsParameter(p.ParameterId, p.ParameterValue))
                .ToList();
            if (parameters.Count == 0)
                return Fail(ResultCodes.ParameterNotFound, $"레시피 {recipe.RecipeId}에 등록된 파라미터가 없습니다.",
                            req.RuleName, req.EventTimeText, req.EqpId, req.MessageKey, requestXml, topic, partition, offset);

            // 05 결과 저장: 요청 1건당 Master 1건 + 응답에 포함한 파라미터 — 처리 이력과 함께 한 트랜잭션
            var response = new RmsResponse(req.RuleName, ResultCodes.Success, "Processed successfully", req.EventTimeText,
                                           req.EqpId, recipe.RecipeId, parameters);
            string responseXml = RmsXml.Build(response);

            using var tx = db.Database.BeginTransaction();
            var result = new TbRmsResult { EventTime = req.EventTime, EquipId = req.EqpId, RecipeId = recipe.RecipeId };
            foreach (var p in parameters)
                result.TbRmsResultParameters.Add(new TbRmsResultParameter { ParameterId = p.ParameterId, ParameterValue = p.ParameterValue });
            db.TbRmsResults.Add(result);
            db.SaveChanges();                                   // RESULT_ID(Master PK) 생성

            db.TbRmsRequestLogs.Add(NewLog(req.MessageKey, topic, partition, offset, req.EqpId, ResultCodes.Success,
                                           response.ResultMessage, result.ResultId, requestXml, responseXml));
            db.SaveChanges();
            tx.Commit();

            log.Info($"[SUCCESS] {req.EqpId} {req.FactorId}={req.FactorValue} → {recipe.RecipeId}, 파라미터 {parameters.Count}건, RESULT_ID={result.ResultId}");
            return new ProcessOutcome(ResultCodes.Success, response.ResultMessage, req.EqpId, recipe.RecipeId, result.ResultId, responseXml);
        }
        catch (Exception ex) when (ex is DbUpdateException or SqlException or InvalidOperationException)
        {
            // DB 오류: 성공으로 응답하지 않는다. 원인은 파일에 남긴다
            string message = ex.GetBaseException().Message;
            log.Error($"[DB_ERROR] {req.EqpId} {req.FactorId}={req.FactorValue}: {message}");
            return Fail(ResultCodes.DbError, "DB 처리 중 오류가 발생했습니다: " + message, req.RuleName, req.EventTimeText, req.EqpId,
                        req.MessageKey, requestXml, topic, partition, offset);
        }
    }

    /// 오류 응답 생성 + 처리 이력 기록
    private ProcessOutcome Fail(string code, string message, string rule, string eventTime, string? eqpId, string? key,
                                string requestXml, string topic, int partition, long offset)
    {
        var response = new RmsResponse(rule.Length > 0 ? rule : settings.SupportedRule, code, message,
                                       eventTime.Length > 0 ? eventTime : DateTime.Now.ToString(RmsXml.TimeFormat), eqpId, null, []);
        try
        {
            using var db = settings.CreateDbContext();
            return Log(db, response, null, key, requestXml, topic, partition, offset);
        }
        catch (Exception ex) when (ex is DbUpdateException or SqlException or InvalidOperationException)
        {
            log.Error($"[이력 저장 실패] {code} {message} / {ex.GetBaseException().Message}{Environment.NewLine}{requestXml}");
            return new ProcessOutcome(code, message, eqpId, null, null, RmsXml.Build(response));
        }
    }

    private ProcessOutcome Log(Data.RmsDbContext db, RmsResponse response, long? resultId, string? key,
                               string requestXml, string topic, int partition, long offset)
    {
        string responseXml = RmsXml.Build(response);
        db.TbRmsRequestLogs.Add(NewLog(key, topic, partition, offset, response.EqpId, response.ResultCode,
                                       response.ResultMessage, resultId, requestXml, responseXml));
        db.SaveChanges();

        if (response.ResultCode == ResultCodes.Duplicate) log.Warn($"[DUPLICATE] {response.EqpId} — {response.ResultMessage}");
        else log.Error($"[{response.ResultCode}] {response.ResultMessage}");
        return new ProcessOutcome(response.ResultCode, response.ResultMessage, response.EqpId, response.RecipeId, resultId, responseXml);
    }

    private static TbRmsRequestLog NewLog(string? key, string topic, int partition, long offset, string? eqpId, string code,
                                          string message, long? resultId, string requestXml, string responseXml)
        => new()
        {
            MessageKey = key, KafkaTopic = topic, KafkaPartition = partition, KafkaOffset = offset,
            EquipId = eqpId?.Length > 50 ? eqpId[..50] : eqpId, ResultCode = code,
            ResultMessage = message.Length > 500 ? message[..500] : message, ResultId = resultId,
            RequestXml = requestXml, ResponseXml = responseXml
        };
}
