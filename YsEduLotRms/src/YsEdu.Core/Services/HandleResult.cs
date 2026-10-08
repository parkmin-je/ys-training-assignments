using YsEdu.Core.Messaging;

namespace YsEdu.Core.Services;

/// 업무 처리 결과. ERROR면 아무것도 반영하지 않는다(트랜잭션 롤백).
public sealed record HandleResult(string Result, string Reason, long? RunId = null, long? CheckId = null)
{
    public static HandleResult Ok(long? runId = null, long? checkId = null) => new(Results.OK, "", runId, checkId);
    public static HandleResult Error(string reason) => new(Results.ERROR, reason);
}
