namespace YsEdu.Core.Messaging;

/// 이벤트 종류 (2차 과제 p.4 필수 이벤트)
public static class EventTypes
{
    public const string LotStart = "LOT_START";           // 첫 설비 A에서 작업 시작
    public const string ProcessStart = "PROCESS_START";   // 각 설비의 실제 구동 시작
    public const string ProcessEnd = "PROCESS_END";       // 각 설비의 실제 구동 종료
    public const string LotEnd = "LOT_END";               // 마지막 설비 C에서 작업 종료
    public const string RmsCheck = "RMS_CHECK";           // 각 설비의 구동 전 검증 요청

    public static readonly IReadOnlyList<string> All = [LotStart, ProcessStart, ProcessEnd, LotEnd, RmsCheck];
}

/// 처리 결과 (V2 p.20: OK 기준 충족 / NG 기준 불일치 / ERROR 처리 실패)
public static class Results
{
    public const string OK = "OK";
    public const string NG = "NG";
    public const string ERROR = "ERROR";
}

/// 응답 Reason 코드
public static class Reasons
{
    public const string XmlFormatError = "XML_FORMAT_ERROR";
    public const string InvalidEventType = "INVALID_EVENT_TYPE";
    public const string InvalidEventTime = "INVALID_EVENT_TIME";
    public const string InvalidParameter = "INVALID_PARAMETER";
    public const string UnregisteredProduct = "UNREGISTERED_PRODUCT";
    public const string UnregisteredStep = "UNREGISTERED_STEP";
    public const string UnregisteredEquipment = "UNREGISTERED_EQP";
    public const string NoBaseRecipe = "NO_BASE_RECIPE";
    public const string LotAlreadyStarted = "LOT_ALREADY_STARTED";
    public const string LotAlreadyDone = "LOT_ALREADY_DONE";
    public const string LotNotFound = "LOT_NOT_FOUND";
    public const string LotProductMismatch = "LOT_PRODUCT_MISMATCH";
    public const string AlreadyRunning = "ALREADY_RUNNING";
    public const string NoProcessStart = "NO_PROCESS_START";
    public const string EquipmentStillRunning = "EQP_STILL_RUNNING";
    public const string DuplicateMessage = "DUPLICATE_MESSAGE";
    public const string DbError = "DB_ERROR";

    public static string Missing(string field) => $"MISSING_{field}";
}
