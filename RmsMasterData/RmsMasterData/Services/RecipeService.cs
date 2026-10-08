using Microsoft.EntityFrameworkCore;
using RmsMasterData.Common;
using RmsMasterData.Data.Entities;

namespace RmsMasterData.Services;

/// 화면 ↔ 서비스 사이에서 주고받는 파라미터 1줄
public sealed class ParameterRow
{
    public string ParameterId { get; set; } = "";
    public string ParameterValue { get; set; } = "";
}

/// 레시피 마스터 입력값
public sealed record RecipeInput(string RecipeId, string EquipId, string FactorId, string FactorValue, string UseYn);

/// <summary>
/// 레시피 기준정보 Master–Detail CRUD (TB_RMS_RECIPE + TB_RMS_RECIPE_PARAMETER)
///   · 레시피 ID 중복 방지
///   · 설비 ID · 팩터 ID · 팩터 값 조합 중복 방지 (UQ_RECIPE_CONDITION)
///   · 동일 레시피의 파라미터 ID 중복 방지
///   · 마스터와 디테일을 하나의 작업으로 저장 — SaveChangesAsync 1회 (EF Core가 한 트랜잭션으로 실행)
///   · 레시피 삭제 시 파라미터도 함께 삭제 (FK ON DELETE CASCADE)
/// </summary>
public sealed class RecipeService
{
    /// 조회: 설비 ID(일치) · 레시피 ID(포함), 비우면 전체
    public async Task<List<TbRmsRecipe>> SearchAsync(string? equipId, string? recipeId)
    {
        await using var db = AppConfig.CreateDbContext();
        var query = db.TbRmsRecipes.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(equipId)) query = query.Where(r => r.EquipId == equipId.Trim());
        if (!string.IsNullOrWhiteSpace(recipeId)) query = query.Where(r => r.RecipeId.Contains(recipeId.Trim()));
        return await query.OrderBy(r => r.EquipId).ThenBy(r => r.RecipeId).ToListAsync();
    }

    /// 선택한 레시피의 파라미터 (디테일)
    public async Task<List<ParameterRow>> GetParametersAsync(string recipeId)
    {
        await using var db = AppConfig.CreateDbContext();
        return await db.TbRmsRecipeParameters.AsNoTracking()
            .Where(p => p.RecipeId == recipeId)
            .OrderBy(p => p.ParameterId)
            .Select(p => new ParameterRow { ParameterId = p.ParameterId, ParameterValue = p.ParameterValue })
            .ToListAsync();
    }

    /// 신규 등록: 마스터 + 디테일
    public async Task CreateAsync(RecipeInput input, IReadOnlyList<ParameterRow> parameters)
    {
        var (recipe, rows) = Validate(input, parameters);

        await using var db = AppConfig.CreateDbContext();
        if (await db.TbRmsRecipes.AnyAsync(r => r.RecipeId == recipe.RecipeId))
            throw new BusinessException($"레시피 ID '{recipe.RecipeId}'는 이미 등록되어 있습니다.");
        await CheckReferencesAsync(db, recipe, isNew: true);

        var entity = new TbRmsRecipe
        {
            RecipeId = recipe.RecipeId, EquipId = recipe.EquipId, FactorId = recipe.FactorId,
            FactorValue = recipe.FactorValue, UseYn = recipe.UseYn
        };
        foreach (var row in rows)
            entity.TbRmsRecipeParameters.Add(new TbRmsRecipeParameter { ParameterId = row.ParameterId, ParameterValue = row.ParameterValue });

        db.TbRmsRecipes.Add(entity);
        await EquipmentService.SaveAsync(db);               // 마스터 + 디테일 INSERT를 한 번에
    }

    /// 수정: 마스터 변경 + 디테일을 화면 내용과 같게 맞춤 (추가·변경·삭제) — 한 번에 저장
    public async Task UpdateAsync(RecipeInput input, IReadOnlyList<ParameterRow> parameters)
    {
        var (recipe, rows) = Validate(input, parameters);

        await using var db = AppConfig.CreateDbContext();
        var entity = await db.TbRmsRecipes.Include(r => r.TbRmsRecipeParameters)
                         .FirstOrDefaultAsync(r => r.RecipeId == recipe.RecipeId)
                     ?? throw new BusinessException($"레시피 ID '{recipe.RecipeId}'를 찾을 수 없습니다. 다시 조회하세요.");
        await CheckReferencesAsync(db, recipe, isNew: false);

        entity.EquipId = recipe.EquipId;
        entity.FactorId = recipe.FactorId;
        entity.FactorValue = recipe.FactorValue;
        entity.UseYn = recipe.UseYn;
        entity.UpdatedAt = DateTime.Now;

        var wanted = rows.ToDictionary(r => r.ParameterId);
        foreach (var existing in entity.TbRmsRecipeParameters.ToList())
        {
            if (!wanted.TryGetValue(existing.ParameterId, out var row))
            {
                db.TbRmsRecipeParameters.Remove(existing);          // 화면에서 지운 파라미터
            }
            else if (existing.ParameterValue != row.ParameterValue)
            {
                existing.ParameterValue = row.ParameterValue;       // 값이 바뀐 파라미터
                existing.UpdatedAt = DateTime.Now;
            }
        }
        var have = entity.TbRmsRecipeParameters.Select(p => p.ParameterId).ToHashSet();
        foreach (var row in rows.Where(r => !have.Contains(r.ParameterId)))
            entity.TbRmsRecipeParameters.Add(new TbRmsRecipeParameter { ParameterId = row.ParameterId, ParameterValue = row.ParameterValue });

        await EquipmentService.SaveAsync(db);               // 마스터 UPDATE + 디테일 INSERT/UPDATE/DELETE를 한 번에
    }

    /// 삭제: 레시피와 파라미터를 함께 삭제
    public async Task DeleteAsync(string recipeId)
    {
        await using var db = AppConfig.CreateDbContext();
        var entity = await db.TbRmsRecipes.Include(r => r.TbRmsRecipeParameters)
                         .FirstOrDefaultAsync(r => r.RecipeId == recipeId)
                     ?? throw new BusinessException($"레시피 ID '{recipeId}'를 찾을 수 없습니다. 다시 조회하세요.");
        db.TbRmsRecipeParameters.RemoveRange(entity.TbRmsRecipeParameters);
        db.TbRmsRecipes.Remove(entity);
        await EquipmentService.SaveAsync(db);
    }

    /// 설비 존재 여부 + 설비·팩터 ID·팩터 값 조합 중복 (자기 자신 제외)
    private static async Task CheckReferencesAsync(DbContext db, RecipeInput recipe, bool isNew)
    {
        var ctx = (Data.RmsDbContext)db;
        if (!await ctx.TbEquipments.AnyAsync(e => e.EquipId == recipe.EquipId))
            throw new BusinessException($"설비 ID '{recipe.EquipId}'가 설비 기준정보에 없습니다.");

        var same = await ctx.TbRmsRecipes.AsNoTracking()
            .Where(r => r.EquipId == recipe.EquipId && r.FactorId == recipe.FactorId && r.FactorValue == recipe.FactorValue)
            .Where(r => isNew || r.RecipeId != recipe.RecipeId)
            .Select(r => r.RecipeId)
            .FirstOrDefaultAsync();
        if (same != null)
            throw new BusinessException($"설비 '{recipe.EquipId}'에 팩터 {recipe.FactorId}={recipe.FactorValue} 조건의 레시피({same})가 이미 있습니다.");
    }

    /// 필수값 · 길이 · 파라미터 ID 중복 검사
    private static (RecipeInput Recipe, List<ParameterRow> Rows) Validate(RecipeInput input, IReadOnlyList<ParameterRow> parameters)
    {
        var recipe = input with
        {
            RecipeId = (input.RecipeId ?? "").Trim(), EquipId = (input.EquipId ?? "").Trim(),
            FactorId = (input.FactorId ?? "").Trim(), FactorValue = (input.FactorValue ?? "").Trim()
        };
        if (recipe.RecipeId.Length == 0) throw new BusinessException("레시피 ID를 입력하세요.");
        if (recipe.RecipeId.Length > 50) throw new BusinessException("레시피 ID는 50자 이하로 입력하세요.");
        if (recipe.EquipId.Length == 0) throw new BusinessException("설비 ID를 선택하세요.");
        if (recipe.FactorId.Length == 0 || recipe.FactorId.Length > 50) throw new BusinessException("팩터 ID를 1~50자로 입력하세요.");
        if (recipe.FactorValue.Length == 0 || recipe.FactorValue.Length > 200) throw new BusinessException("팩터 값을 1~200자로 입력하세요.");
        if (recipe.UseYn is not ("Y" or "N")) throw new BusinessException("사용 여부는 Y 또는 N입니다.");

        // 그리드의 빈 줄은 무시
        var rows = parameters
            .Where(p => !string.IsNullOrWhiteSpace(p.ParameterId) || !string.IsNullOrWhiteSpace(p.ParameterValue))
            .Select(p => new ParameterRow { ParameterId = (p.ParameterId ?? "").Trim(), ParameterValue = (p.ParameterValue ?? "").Trim() })
            .ToList();

        foreach (var row in rows)
        {
            if (row.ParameterId.Length == 0) throw new BusinessException($"파라미터 ID가 빈 줄이 있습니다. (값: {row.ParameterValue})");
            if (row.ParameterId.Length > 50) throw new BusinessException($"파라미터 ID '{row.ParameterId}'는 50자 이하로 입력하세요.");
            if (row.ParameterValue.Length == 0) throw new BusinessException($"파라미터 '{row.ParameterId}'의 값을 입력하세요.");
            if (row.ParameterValue.Length > 500) throw new BusinessException($"파라미터 '{row.ParameterId}'의 값은 500자 이하로 입력하세요.");
        }

        var duplicated = rows.GroupBy(r => r.ParameterId, StringComparer.OrdinalIgnoreCase).Where(g => g.Count() > 1).Select(g => g.Key).ToList();
        if (duplicated.Count > 0)
            throw new BusinessException($"같은 레시피에 파라미터 ID가 중복되었습니다: {string.Join(", ", duplicated)}");

        return (recipe, rows);
    }
}
