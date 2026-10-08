using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace RmsKafkaMiddleware.Messaging;

/// 결과 코드 (응답 HEADER의 RESULT_CODE)
public static class ResultCodes
{
    public const string Success = "SUCCESS";                     // 정상 처리
    public const string XmlFormatError = "XML_FORMAT_ERROR";     // 01 XML 형식 오류
    public const string MissingNode = "MISSING_NODE";            // 02 필수 노드 누락
    public const string RuleNotSupported = "RULE_NOT_SUPPORTED"; // 03 Rule 불일치
    public const string RecipeNotFound = "RECIPE_NOT_FOUND";     // 04 레시피 없음
    public const string ParameterNotFound = "PARAMETER_NOT_FOUND"; // 05 파라미터 없음
    public const string DbError = "DB_ERROR";                    // 06 DB 오류
    public const string InvalidValue = "INVALID_VALUE";          // 노드는 있으나 값 형식 오류 (EVENT_TIME)
    public const string Duplicate = "DUPLICATE";                 // 동일 메시지 재수신 — 다시 저장하지 않음
}

/// 요청 XML BODY·HEADER 값 (Kafka p.6)
public sealed record RmsRequest(string RuleName, string EventTimeText, DateTime EventTime, string EqpId, string FactorId, string FactorValue)
{
    /// 동일 메시지 식별 키: 요청의 모든 값을 이어 붙인 SHA-256
    public string MessageKey
    {
        get
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes($"{RuleName}|{EventTimeText}|{EqpId}|{FactorId}|{FactorValue}"));
            return Convert.ToHexString(bytes);
        }
    }
}

/// 응답 파라미터 1건
public sealed record RmsParameter(string ParameterId, string ParameterValue);

/// 응답 내용 (Kafka p.8)
public sealed record RmsResponse(string RuleName, string ResultCode, string ResultMessage, string EventTime,
                                 string? EqpId, string? RecipeId, IReadOnlyList<RmsParameter> Parameters);

/// 파싱 결과: 성공이면 Request, 실패면 오류 코드와 메시지 (응답에 쓸 수 있도록 읽어 낸 값도 함께)
public sealed record ParseResult(RmsRequest? Request, string ErrorCode, string ErrorMessage, string RuleName, string EventTime, string? EqpId);

/// <summary>
/// RMS_TEST 요청·응답 XML (Kafka p.6·8)
///   &lt;MESSAGE&gt;&lt;HEADER&gt;RULE_NAME, EVENT_TIME&lt;/HEADER&gt;&lt;BODY&gt;EQP_ID, FACTOR_ID, FACTOR_VALUE&lt;/BODY&gt;&lt;/MESSAGE&gt;
/// </summary>
public static class RmsXml
{
    public const string TimeFormat = "yyyy-MM-dd'T'HH:mm:ss.fff";

    /// 02 XML 노드 파싱 — 형식 오류 / 필수 노드 누락 / 값 형식 오류를 구분한다
    public static ParseResult Parse(string xml)
    {
        XElement root;
        try
        {
            root = XElement.Parse(xml);
        }
        catch (XmlException ex)
        {
            return new ParseResult(null, ResultCodes.XmlFormatError, $"XML 파싱 실패 (줄 {ex.LineNumber}, 위치 {ex.LinePosition}): {ex.Message}", "", "", null);
        }

        var header = root.Element("HEADER");
        var body = root.Element("BODY");
        string rule = Text(header, "RULE_NAME");
        string time = Text(header, "EVENT_TIME");
        string eqp = Text(body, "EQP_ID");
        string factorId = Text(body, "FACTOR_ID");
        string factorValue = Text(body, "FACTOR_VALUE");

        if (root.Name.LocalName != "MESSAGE")
            return new ParseResult(null, ResultCodes.XmlFormatError, $"루트 노드가 MESSAGE가 아닙니다: <{root.Name.LocalName}>", rule, time, NullIfEmpty(eqp));

        // 누락된 항목을 결과 메시지에 표시 (Kafka p.11)
        var missing = new List<string>();
        if (header == null) missing.Add("HEADER");
        if (body == null) missing.Add("BODY");
        if (header != null && rule.Length == 0) missing.Add("HEADER/RULE_NAME");
        if (header != null && time.Length == 0) missing.Add("HEADER/EVENT_TIME");
        if (body != null && eqp.Length == 0) missing.Add("BODY/EQP_ID");
        if (body != null && factorId.Length == 0) missing.Add("BODY/FACTOR_ID");
        if (body != null && factorValue.Length == 0) missing.Add("BODY/FACTOR_VALUE");
        if (missing.Count > 0)
            return new ParseResult(null, ResultCodes.MissingNode, "필수 노드 누락: " + string.Join(", ", missing), rule, time, NullIfEmpty(eqp));

        if (!DateTime.TryParse(time, CultureInfo.InvariantCulture, DateTimeStyles.None, out var eventTime))
            return new ParseResult(null, ResultCodes.InvalidValue, $"EVENT_TIME 형식 오류: {time}", rule, time, eqp);

        return new ParseResult(new RmsRequest(rule, time, eventTime, eqp, factorId, factorValue), "", "", rule, time, eqp);
    }

    /// 04 응답 XML 생성 — 정상·오류 모두 같은 형식
    public static string Build(RmsResponse r)
    {
        var body = new XElement("BODY");
        if (!string.IsNullOrEmpty(r.EqpId)) body.Add(new XElement("EQP_ID", r.EqpId));
        if (!string.IsNullOrEmpty(r.RecipeId)) body.Add(new XElement("RECIPE_ID", r.RecipeId));
        if (r.Parameters.Count > 0)
        {
            body.Add(new XElement("PARAMETERS",
                r.Parameters.Select(p => new XElement("PARAMETER",
                    new XElement("PARAMETER_ID", p.ParameterId),
                    new XElement("PARAMETER_VALUE", p.ParameterValue)))));
        }

        return new XElement("MESSAGE",
            new XElement("HEADER",
                new XElement("RULE_NAME", r.RuleName),
                new XElement("RESULT_CODE", r.ResultCode),
                new XElement("RESULT_MESSAGE", r.ResultMessage),
                new XElement("EVENT_TIME", r.EventTime)),
            body).ToString();
    }

    private static string Text(XElement? parent, string name) => parent?.Element(name)?.Value.Trim() ?? "";

    private static string? NullIfEmpty(string value) => value.Length == 0 ? null : value;
}
