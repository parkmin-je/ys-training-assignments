namespace YsEdu.Core.Messaging;

/// &lt;Parameter ParameterID="TEMP" Value="120" /&gt; 한 줄. 값은 받은 글자 그대로 둔다(숫자 변환은 RMS 판정 단계).
public sealed record EventParameter(string ParameterId, string Value);

/// <summary>
/// 설비 → 미들웨어 요청 (&lt;ProductionEvent&gt;, 2차 과제 p.5·6 XML 메시지 규격)
/// MessageName = 메시지 식별값(표의 MessageID). 새 요청은 새 값, 같은 값 재수신은 중복.
/// </summary>
public sealed class ProductionEvent
{
    public string MessageName { get; set; } = "";
    public string ProductId { get; set; } = "";
    public string LotId { get; set; } = "";
    public string StepId { get; set; } = "";
    public string EqpId { get; set; } = "";
    public string RecipeId { get; set; } = "";
    public string EventType { get; set; } = "";
    public string EventTimeText { get; set; } = "";
    public DateTimeOffset EventTime { get; set; }
    public List<EventParameter> Parameters { get; } = [];

    /// XML에 &lt;Parameters&gt; 요소가 있었는가 (없음과 비어 있음을 구분)
    public bool HasParametersElement { get; set; }
}

/// 미들웨어 → 설비 응답 (&lt;ProductionEventResponse&gt;, 응답 필수 항목: MessageName·EventType·LOTID·EQPID·Result·Reason)
public sealed record ProductionEventResponse(
    string MessageName, string EventType, string LotId, string EqpId, string Result, string Reason, DateTimeOffset EventTime);
