using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace YsEdu.Core.Messaging;

/// <summary>
/// XML 만들기 · 파싱 · 값 검증 (V2 p.17의 1·2단계, 3단계 업무 검증은 MessageProcessor에서)
/// "XML을 정상적으로 읽었다고 해서 업무 데이터까지 유효한 것은 아니다."
/// </summary>
public static partial class XmlMessage
{
    public const string TimeFormat = "yyyy-MM-dd'T'HH:mm:sszzz";

    /// 시간대 포함 형식 (2차 p.5 "시간대 포함 발생시간", p.6 예제) 예: 2026-10-05T09:04:00+09:00
    [GeneratedRegex(@"^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}(\.\d{1,7})?(Z|[+-]\d{2}:\d{2})$")]
    private static partial Regex TimeWithZone();

    /// 숫자 형식 (2차 p.7 예제) 예: 2026100514000000 = yyyyMMddHHmmss + 1/100초. 시간대가 없으므로 로컬(KST)로 본다
    private static readonly string[] CompactFormats = ["yyyyMMddHHmmssff", "yyyyMMddHHmmss"];

    private static bool TryParseEventTime(string text, out DateTimeOffset time)
    {
        if (TimeWithZone().IsMatch(text))
            return DateTimeOffset.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.None, out time);

        if (DateTime.TryParseExact(text, CompactFormats, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var local))
        {
            time = new DateTimeOffset(local);
            return true;
        }

        time = default;
        return false;
    }

    public static string FormatTime(DateTimeOffset time) => time.ToString(TimeFormat, CultureInfo.InvariantCulture);

    // ---------------------------------------------------------------- 만들기
    public static string Build(ProductionEvent e)
    {
        var root = new XElement("ProductionEvent",
            new XElement("MessageName", e.MessageName),
            new XElement("ProductID", e.ProductId),
            new XElement("LOTID", e.LotId),
            new XElement("STEPID", e.StepId),
            new XElement("EQPID", e.EqpId),
            new XElement("RecipeID", e.RecipeId),
            new XElement("EventType", e.EventType),
            new XElement("EventTime", string.IsNullOrEmpty(e.EventTimeText) ? FormatTime(e.EventTime) : e.EventTimeText));

        // LOT / PROCESS 요청은 Parameters를 생략한다 (2차 p.7)
        if (e.Parameters.Count > 0)
        {
            root.Add(new XElement("Parameters",
                e.Parameters.Select(p => new XElement("Parameter",
                    new XAttribute("ParameterID", p.ParameterId),
                    new XAttribute("Value", p.Value)))));
        }
        return root.ToString();
    }

    public static string Build(ProductionEventResponse r)
        => new XElement("ProductionEventResponse",
            new XElement("MessageName", r.MessageName),
            new XElement("EventType", r.EventType),
            new XElement("LOTID", r.LotId),
            new XElement("EQPID", r.EqpId),
            new XElement("Result", r.Result),
            new XElement("Reason", r.Reason),
            new XElement("EventTime", FormatTime(r.EventTime))).ToString();

    // ---------------------------------------------------------------- 1단계: 파싱
    /// 구조를 읽는다. 태그가 닫히지 않는 등 구조 오류면 null과 오류 내용을 돌려준다.
    public static ProductionEvent? TryParse(string rawXml, out string error)
    {
        XElement root;
        try
        {
            root = XElement.Parse(rawXml);
        }
        catch (XmlException ex)
        {
            error = $"XML 구조 오류 (줄 {ex.LineNumber}, 위치 {ex.LinePosition}): {ex.Message}";
            return null;
        }

        if (root.Name.LocalName != "ProductionEvent")
        {
            error = $"루트 요소가 ProductionEvent가 아님: <{root.Name.LocalName}>";
            return null;
        }

        var e = new ProductionEvent
        {
            // p.6·7 예제는 MessageName, p.5 표는 MessageID — 둘 다 받는다
            MessageName = Text(root, "MessageName") is { Length: > 0 } name ? name : Text(root, "MessageID"),
            ProductId = Text(root, "ProductID"),
            LotId = Text(root, "LOTID"),
            StepId = Text(root, "STEPID"),
            EqpId = Text(root, "EQPID"),
            RecipeId = Text(root, "RecipeID"),
            EventType = Text(root, "EventType"),
            EventTimeText = Text(root, "EventTime")
        };

        var parameters = root.Element("Parameters");
        e.HasParametersElement = parameters != null;
        if (parameters != null)
        {
            foreach (var p in parameters.Elements("Parameter"))
                e.Parameters.Add(new EventParameter(((string?)p.Attribute("ParameterID") ?? "").Trim(),
                                                    ((string?)p.Attribute("Value") ?? "").Trim()));
        }

        error = "";
        return e;
    }

    public static ProductionEventResponse? TryParseResponse(string rawXml)
    {
        try
        {
            var root = XElement.Parse(rawXml);
            if (root.Name.LocalName != "ProductionEventResponse") return null;
            DateTimeOffset.TryParse(Text(root, "EventTime"), CultureInfo.InvariantCulture, DateTimeStyles.None, out var time);
            return new ProductionEventResponse(Text(root, "MessageName"), Text(root, "EventType"), Text(root, "LOTID"),
                Text(root, "EQPID"), Text(root, "Result"), Text(root, "Reason"), time);
        }
        catch (XmlException)
        {
            return null;
        }
    }

    // ---------------------------------------------------------------- 2단계: 값 검증
    /// 필수 항목 누락, 빈 값, 시간 형식, 이벤트 종류, 파라미터 형식. 문제가 없으면 빈 목록.
    public static List<string> Validate(ProductionEvent e)
    {
        var errors = new List<string>();

        // 2차 p.5: 아래 항목은 "모든 요청" 필수
        Require(errors, e.MessageName, "MESSAGENAME");
        Require(errors, e.ProductId, "PRODUCTID");
        Require(errors, e.LotId, "LOTID");
        Require(errors, e.StepId, "STEPID");
        Require(errors, e.EqpId, "EQPID");
        Require(errors, e.RecipeId, "RECIPEID");
        Require(errors, e.EventType, "EVENTTYPE");
        Require(errors, e.EventTimeText, "EVENTTIME");

        if (e.EventType.Length > 0 && !EventTypes.All.Contains(e.EventType))
            errors.Add(Reasons.InvalidEventType);

        if (e.EventTimeText.Length > 0)
        {
            if (TryParseEventTime(e.EventTimeText, out var time))
                e.EventTime = time;
            else
                errors.Add(Reasons.InvalidEventTime);
        }

        // Parameters는 RMS_CHECK만 필수 (ParameterID와 Value를 반복 항목으로)
        if (e.EventType == EventTypes.RmsCheck)
        {
            if (e.Parameters.Count == 0)
                errors.Add(Reasons.Missing("PARAMETERS"));
            else if (e.Parameters.Any(p => p.ParameterId.Length == 0 || p.Value.Length == 0))
                errors.Add(Reasons.InvalidParameter);
        }

        return errors;
    }

    private static void Require(List<string> errors, string value, string field)
    {
        if (string.IsNullOrWhiteSpace(value)) errors.Add(Reasons.Missing(field));
    }

    private static string Text(XElement root, string name) => root.Element(name)?.Value.Trim() ?? "";
}
