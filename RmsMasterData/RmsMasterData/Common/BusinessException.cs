namespace RmsMasterData.Common;

/// 업무 규칙 위반 (중복, 필수값 누락, 사용 중 데이터 삭제 등). 화면은 이 메시지를 그대로 사용자에게 보여 준다.
public sealed class BusinessException(string message) : Exception(message);
