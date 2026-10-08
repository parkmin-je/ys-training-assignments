using YsEdu.Core.Messaging;

namespace YsEdu.Core.Simulation;

/// 시나리오 1단계 결과
public sealed record ScenarioStep(string Title, string ExpectedResult, string ExpectedReason, ProductionEventResponse? Response)
{
    public bool Pass => Response != null && Response.Result == ExpectedResult
                        && (ExpectedReason.Length == 0 || Response.Reason.Contains(ExpectedReason));
    public string Actual => Response == null ? "응답 없음" : $"{Response.Result} {Response.Reason}".Trim();
}

/// <summary>
/// 설비 / CIM 시뮬레이터 (2차 과제 p.2·3·8)
///   · A · B · C 인라인: A에서 LOT 시작, 각 설비 구동 시작·종료, C에서 LOT 종료
///   · 각 설비는 본인의 EQPID와 RecipeID를 송신, 동일 LOTID와 STEPID 사용
///   · 구동 전 RMS_CHECK — OK일 때만 구동, NG / ERROR면 구동 보류
///   · 값을 수정한 검증은 새 MessageName으로 송신. PROCESS 이벤트는 실제 구동 통지
/// 아래 시나리오는 2차 과제 p.12 "완료 확인 시나리오"와 1:1로 대응한다.
/// </summary>
public sealed class InlineSimulator
{
    public const string ProductId = "PRODUCT_A";
    public const string StepId = "STEP_020";

    public static readonly IReadOnlyList<(string EqpId, string RecipeId)> Line =
        [("EQP_A", "RCP_A01"), ("EQP_B", "RCP_B01"), ("EQP_C", "RCP_C01")];

    /// 설비에 내려받은 기준 레시피 값 (2차 p.3: TEMP=120, PRESSURE=30, SPEED=450)
    public static IReadOnlyList<EventParameter> DefaultParameters() =>
        [new("TEMP", "120"), new("PRESSURE", "30"), new("SPEED", "450")];

    private readonly EquipmentClient _client;

    public InlineSimulator(EquipmentClient client) => _client = client;

    /// 단계가 끝날 때마다 발생 (호출 스레드)
    public event Action<ScenarioStep>? StepCompleted;

    public ProductionEvent NewEvent(string lotId, string eqpId, string recipeId, string eventType, IEnumerable<EventParameter>? parameters = null)
    {
        var e = new ProductionEvent
        {
            MessageName = _client.NewMessageName(eqpId),
            ProductId = ProductId,
            LotId = lotId,
            StepId = StepId,
            EqpId = eqpId,
            RecipeId = recipeId,
            EventType = eventType,
            EventTime = DateTimeOffset.Now
        };
        if (parameters != null) e.Parameters.AddRange(parameters);
        return e;
    }

    // ------------------------------------------------------------------ 1. 정상 인라인
    public async Task<List<ScenarioStep>> NormalInlineAsync(string lotId)
    {
        var steps = new List<ScenarioStep>();
        await Step(steps, "A LOT_START", Results.OK, "", NewEvent(lotId, "EQP_A", "RCP_A01", EventTypes.LotStart));
        foreach (var (eqp, recipe) in Line)
            await RunEquipmentAsync(steps, lotId, eqp, recipe, DefaultParameters());
        await Step(steps, "C LOT_END", Results.OK, "", NewEvent(lotId, "EQP_C", "RCP_C01", EventTypes.LotEnd));
        return steps;
    }

    // ------------------------------------------------------------------ 2. 중간 설비
    /// B만 단독으로: 어느 설비의 LOT_START도 없이 RMS_CHECK → PROCESS_START → PROCESS_END
    public async Task<List<ScenarioStep>> MiddleEquipmentAsync(string lotId)
    {
        var steps = new List<ScenarioStep>();
        await RunEquipmentAsync(steps, lotId, "EQP_B", "RCP_B01", DefaultParameters());
        return steps;
    }

    // ------------------------------------------------------------------ 3. RMS 불일치 → 재검증
    public async Task<List<ScenarioStep>> RmsMismatchAsync(string lotId)
    {
        var steps = new List<ScenarioStep>();
        var wrong = new List<EventParameter> { new("TEMP", "120"), new("PRESSURE", "30"), new("SPEED", "500") };
        var r = await Step(steps, "B RMS_CHECK SPEED=500 → 구동 보류", Results.NG, "SPEED_MISMATCH",
                           NewEvent(lotId, "EQP_B", "RCP_B01", EventTypes.RmsCheck, wrong));
        if (r?.Result == Results.OK) return steps;

        // 레시피 재다운로드 후 값을 고쳐 새 MessageName으로 재검증
        await RunEquipmentAsync(steps, lotId, "EQP_B", "RCP_B01", DefaultParameters(), "재검증 SPEED=450");
        return steps;
    }

    // ------------------------------------------------------------------ 4. 구성 오류
    public async Task<List<ScenarioStep>> StructureErrorsAsync(string lotId)
    {
        var steps = new List<ScenarioStep>();
        await Step(steps, "누락: PRESSURE 없이 RMS_CHECK", Results.NG, "PRESSURE_MISSING",
            NewEvent(lotId, "EQP_B", "RCP_B01", EventTypes.RmsCheck, [new("TEMP", "120"), new("SPEED", "450")]));
        await Step(steps, "추가: 기준에 없는 HUMIDITY 포함", Results.NG, "HUMIDITY_NOT_DEFINED",
            NewEvent(lotId, "EQP_B", "RCP_B01", EventTypes.RmsCheck, [.. DefaultParameters(), new("HUMIDITY", "40")]));
        await Step(steps, "중복: SPEED 두 번", Results.NG, "SPEED_DUPLICATED",
            NewEvent(lotId, "EQP_B", "RCP_B01", EventTypes.RmsCheck, [.. DefaultParameters(), new("SPEED", "450")]));
        await Step(steps, "RecipeID 불일치: RCP_X99", Results.NG, "RECIPE_ID_MISMATCH",
            NewEvent(lotId, "EQP_B", "RCP_X99", EventTypes.RmsCheck, DefaultParameters()));
        return steps;
    }

    // ------------------------------------------------------------------ 5. 순서 · 중복
    public async Task<List<ScenarioStep>> OrderAndDuplicateAsync(string lotId)
    {
        var steps = new List<ScenarioStep>();
        await Step(steps, "C PROCESS_END (START 없음)", Results.ERROR, Reasons.NoProcessStart,
            NewEvent(lotId, "EQP_C", "RCP_C01", EventTypes.ProcessEnd));

        // 같은 MessageName의 메시지를 두 번 보낸다 (재전송 상황)
        var same = NewEvent(lotId, "EQP_A", "RCP_A01", EventTypes.RmsCheck, DefaultParameters());
        await Step(steps, "A RMS_CHECK 1회째", Results.OK, "", same);
        await Step(steps, "같은 MessageName 재수신 → 중복 반영 없음", Results.OK, Reasons.DuplicateMessage, same);
        return steps;
    }

    // ------------------------------------------------------------------ 6. 처리 오류
    public async Task<List<ScenarioStep>> ProcessingErrorsAsync(string lotId)
    {
        var steps = new List<ScenarioStep>();

        // 잘못된 XML: <LOTID> 태그가 닫히지 않음
        var broken = XmlMessage.Build(NewEvent(lotId, "EQP_A", "RCP_A01", EventTypes.ProcessStart))
                               .Replace("</LOTID>", "<LOTID>");
        var r1 = await _client.SendRawAsync("", "EQP_A", EventTypes.ProcessStart, broken);
        Add(steps, new ScenarioStep("잘못된 XML (</LOTID> 누락)", Results.ERROR, Reasons.XmlFormatError, r1));

        await Step(steps, "미등록 설비 EQP_X", Results.ERROR, Reasons.UnregisteredEquipment,
            NewEvent(lotId, "EQP_X", "RCP_X01", EventTypes.ProcessStart));

        await Step(steps, "필수값 LOTID 누락", Results.ERROR, Reasons.Missing("LOTID"),
            NewEvent("", "EQP_A", "RCP_A01", EventTypes.ProcessStart));

        var noZone = NewEvent(lotId, "EQP_A", "RCP_A01", EventTypes.ProcessStart);
        // 숫자형(2026100514000000, p.7 예제)은 허용하지만, ISO 형식에서 시간대를 뺀 값은 p.5 "시간대 포함" 위반으로 ERROR
        noZone.EventTimeText = "2026-10-05T14:00:00";
        await Step(steps, "시간대 없는 ISO EventTime", Results.ERROR, Reasons.InvalidEventTime, noZone);

        // DB 실패: LOTID가 컬럼 길이(NVARCHAR(50))를 넘어 INSERT 실패
        await Step(steps, "DB 처리 실패 (LOTID 60자)", Results.ERROR, Reasons.DbError,
            NewEvent(new string('L', 60), "EQP_A", "RCP_A01", EventTypes.LotStart));

        // 오류 뒤에도 수신이 계속되는지
        await Step(steps, "오류 이후 정상 메시지 처리 지속", Results.OK, "",
            NewEvent(lotId, "EQP_A", "RCP_A01", EventTypes.RmsCheck, DefaultParameters()));
        return steps;
    }

    /// 설비 1대: RMS_CHECK → OK일 때만 PROCESS_START → PROCESS_END. NG / ERROR면 구동 보류
    private async Task RunEquipmentAsync(List<ScenarioStep> steps, string lotId, string eqp, string recipe,
                                         IEnumerable<EventParameter> parameters, string label = "RMS_CHECK")
    {
        var check = await Step(steps, $"{eqp[^1..]} {label}", Results.OK, "",
                               NewEvent(lotId, eqp, recipe, EventTypes.RmsCheck, parameters));
        if (check?.Result != Results.OK) return;            // 구동 보류

        await Step(steps, $"{eqp[^1..]} PROCESS_START", Results.OK, "", NewEvent(lotId, eqp, recipe, EventTypes.ProcessStart));
        await Step(steps, $"{eqp[^1..]} PROCESS_END", Results.OK, "", NewEvent(lotId, eqp, recipe, EventTypes.ProcessEnd));
    }

    private async Task<ProductionEventResponse?> Step(List<ScenarioStep> steps, string title, string expected, string reason, ProductionEvent e)
    {
        var response = await _client.SendAsync(e);
        Add(steps, new ScenarioStep(title, expected, reason, response));
        return response;
    }

    private void Add(List<ScenarioStep> steps, ScenarioStep step)
    {
        steps.Add(step);
        StepCompleted?.Invoke(step);
    }
}
