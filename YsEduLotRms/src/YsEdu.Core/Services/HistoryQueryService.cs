using Microsoft.EntityFrameworkCore;
using YsEdu.Core.Data;
using YsEdu.Core.Data.Entities;

namespace YsEdu.Core.Services;

/// 조회 조건 (2차 과제 p.10: LOTID, 설비, 기간)
public sealed record HistoryFilter(string? LotId, string? EqpId, DateTimeOffset From, DateTimeOffset To);

/// 조회 결과 묶음
public sealed record HistoryResult(
    List<TbLot> Lots,
    List<TbEquipment> Equipments,
    List<TbLotEvent> Events,
    List<TbEqpRun> Runs,
    List<TbRmsCheck> Checks);

/// <summary>
/// WinForm 조회 화면용 조회 서비스 (DB에 저장한 결과를 조회한다. 화면에서 DB를 직접 다루지 않는다)
/// 조회 1회 = DbContext 1개, 읽기 전용(AsNoTracking)
/// </summary>
public sealed class HistoryQueryService
{
    private readonly DbFactory _db;

    public HistoryQueryService(DbFactory db) => _db = db;

    public async Task<HistoryResult> SearchAsync(HistoryFilter f, CancellationToken token = default)
    {
        await using var db = _db.Create();
        string? lot = string.IsNullOrWhiteSpace(f.LotId) ? null : f.LotId.Trim();
        string? eqp = string.IsNullOrWhiteSpace(f.EqpId) ? null : f.EqpId;

        // LOT 작업 상태: 기간 안에 이벤트가 있는 LOT
        var lotIds = db.TbLotEvents.AsNoTracking()
            .Where(e => e.EventTime >= f.From && e.EventTime <= f.To)
            .Where(e => lot == null || e.LotId == lot)
            .Where(e => eqp == null || e.EqpId == eqp)
            .Select(e => e.LotId).Distinct();
        var lots = await db.TbLots.AsNoTracking()
            .Where(l => lotIds.Contains(l.LotId))
            .OrderByDescending(l => l.UpdatedAt)
            .ToListAsync(token);

        var equipments = await db.TbEquipments.AsNoTracking()
            .Where(q => eqp == null || q.EqpId == eqp)
            .OrderBy(q => q.EqpId)
            .ToListAsync(token);

        var events = await db.TbLotEvents.AsNoTracking()
            .Where(e => e.EventTime >= f.From && e.EventTime <= f.To)
            .Where(e => lot == null || e.LotId == lot)
            .Where(e => eqp == null || e.EqpId == eqp)
            .OrderBy(e => e.EventTime).ThenBy(e => e.EventSeq)
            .Take(2000)
            .ToListAsync(token);

        var runs = await db.TbEqpRuns.AsNoTracking()
            .Where(r => r.StartTime >= f.From && r.StartTime <= f.To)
            .Where(r => lot == null || r.LotId == lot)
            .Where(r => eqp == null || r.EqpId == eqp)
            .OrderBy(r => r.StartTime).ThenBy(r => r.RunId)
            .ToListAsync(token);

        var checks = await db.TbRmsChecks.AsNoTracking()
            .Where(c => c.EventTime >= f.From && c.EventTime <= f.To)
            .Where(c => lot == null || c.LotId == lot)
            .Where(c => eqp == null || c.EqpId == eqp)
            .OrderBy(c => c.EventTime).ThenBy(c => c.CheckId)
            .ToListAsync(token);

        return new HistoryResult(lots, equipments, events, runs, checks);
    }

    /// RMS 검증 이력 선택 → 항목별 상세 (기준값·수신값·판정·사유)
    public async Task<List<TbRmsCheckDtl>> GetCheckDetailsAsync(long checkId, CancellationToken token = default)
    {
        await using var db = _db.Create();
        return await db.TbRmsCheckDtls.AsNoTracking()
            .Where(d => d.CheckId == checkId)
            .OrderBy(d => d.DtlSeq)
            .ToListAsync(token);
    }

    /// 수신 메시지 로그 (ERROR 사유 확인용)
    public async Task<List<TbMsgLog>> GetMessageLogAsync(HistoryFilter f, CancellationToken token = default)
    {
        await using var db = _db.Create();
        string? lot = string.IsNullOrWhiteSpace(f.LotId) ? null : f.LotId.Trim();
        string? eqp = string.IsNullOrWhiteSpace(f.EqpId) ? null : f.EqpId;
        DateTime from = f.From.LocalDateTime, to = f.To.LocalDateTime;
        return await db.TbMsgLogs.AsNoTracking()
            .Where(m => m.Direction == "IN" && m.LoggedAt >= from && m.LoggedAt <= to)
            .Where(m => lot == null || m.LotId == lot)
            .Where(m => eqp == null || m.EqpId == eqp)
            .OrderByDescending(m => m.LogSeq)
            .Take(500)
            .ToListAsync(token);
    }

    public async Task<List<string>> GetEquipmentIdsAsync()
    {
        await using var db = _db.Create();
        return await db.TbEquipments.AsNoTracking().OrderBy(q => q.EqpId).Select(q => q.EqpId).ToListAsync();
    }
}
