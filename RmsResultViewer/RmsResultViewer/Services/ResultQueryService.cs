using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RmsResultViewer.Data;
using RmsResultViewer.Data.Entities;

namespace RmsResultViewer.Services;

/// 조회 조건 — 이벤트 시작·종료 시간은 필수, 설비 ID·레시피 ID는 선택 (조회 p.6·8)
public sealed record ResultFilter(DateTime From, DateTime To, string? EquipId, string? RecipeId);

/// Detail 1줄 (화면 표시용)
public sealed record ParameterView(string ParameterId, string ParameterValue);

/// <summary>
/// RMS 결과 조회 (EF Core Database First, 조회 p.7)
///   02 Master 조회 : 조회조건을 적용해 TB_RMS_RESULT 목록 — 기간 밖은 조회하지 않음, 최신 EVENT_TIME 먼저
///   03 Detail 조회 : 선택한 RESULT_ID로 파라미터 목록
/// 조회 1회 = DbContext 1개, 읽기 전용(AsNoTracking)
/// </summary>
public sealed class ResultQueryService
{
    private static readonly Lazy<string> _cs = new(() =>
        new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory).AddJsonFile("appsettings.json").Build()
            .GetConnectionString("YsRmsDB") ?? throw new InvalidOperationException("ConnectionStrings:YsRmsDB가 없습니다."));

    private static RmsResultDbContext Create()
        => new(new DbContextOptionsBuilder<RmsResultDbContext>().UseSqlServer(_cs.Value).Options);

    public async Task<List<TbRmsResult>> SearchAsync(ResultFilter f, CancellationToken token = default)
    {
        if (f.From > f.To) throw new ArgumentException("이벤트 시작 시간이 종료 시간보다 늦습니다.");

        await using var db = Create();
        var query = db.TbRmsResults.AsNoTracking()
            .Where(r => r.EventTime >= f.From && r.EventTime <= f.To);                        // 기간 밖 조회 안 함
        if (!string.IsNullOrWhiteSpace(f.EquipId)) query = query.Where(r => r.EquipId == f.EquipId.Trim());
        if (!string.IsNullOrWhiteSpace(f.RecipeId)) query = query.Where(r => r.RecipeId == f.RecipeId.Trim());

        return await query.OrderByDescending(r => r.EventTime).ThenByDescending(r => r.ResultId)  // 최신 EVENT_TIME 먼저
                          .ToListAsync(token);
    }

    public async Task<List<ParameterView>> GetParametersAsync(long resultId, CancellationToken token = default)
    {
        await using var db = Create();
        return await db.TbRmsResultParameters.AsNoTracking()
            .Where(p => p.ResultId == resultId)
            .OrderBy(p => p.ParameterId)
            .Select(p => new ParameterView(p.ParameterId, p.ParameterValue))
            .ToListAsync(token);
    }
}
