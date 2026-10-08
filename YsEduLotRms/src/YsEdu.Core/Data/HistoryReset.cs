using Microsoft.EntityFrameworkCore;

namespace YsEdu.Core.Data;

/// <summary>
/// 시연용: 이력·현재 상태만 비우고 기준정보는 그대로 둔다 (03_seed.sql 재실행과 같은 효과의 이력 부분)
/// </summary>
public static class HistoryReset
{
    public static async Task RunAsync(DbFactory factory)
    {
        await using var db = factory.Create();
        await using var tx = await db.Database.BeginTransactionAsync();
        await db.Database.ExecuteSqlRawAsync(@"
            DELETE FROM dbo.TB_MSG_LOG;
            DELETE FROM dbo.TB_MSG_PROCESSED;
            DELETE FROM dbo.TB_RMS_CHECK_DTL;
            DELETE FROM dbo.TB_RMS_CHECK;
            DELETE FROM dbo.TB_LOT_EVENT;
            DELETE FROM dbo.TB_EQP_RUN;
            DELETE FROM dbo.TB_LOT;
            UPDATE dbo.TB_EQUIPMENT SET EQP_STATUS = N'IDLE', CUR_LOT_ID = NULL, UPDATED_AT = SYSDATETIME();");
        await tx.CommitAsync();
    }
}
