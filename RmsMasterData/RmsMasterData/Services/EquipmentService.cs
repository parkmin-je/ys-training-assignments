using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RmsMasterData.Common;
using RmsMasterData.Data.Entities;

namespace RmsMasterData.Services;

/// <summary>
/// 설비 기준정보 CRUD (TB_EQUIPMENT)
///   · 설비 ID 중복 방지
///   · 사용 중인 설비(연결된 레시피가 있는 설비) 삭제 처리: 삭제하지 않고 이유를 알린다
///   · Entity와 DbContext로 반영 (SaveChangesAsync)
/// </summary>
public sealed class EquipmentService
{
    /// 조회: 설비 ID · 설비명 (포함 검색, 비우면 전체)
    public async Task<List<TbEquipment>> SearchAsync(string? equipId, string? equipName)
    {
        await using var db = AppConfig.CreateDbContext();
        var query = db.TbEquipments.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(equipId)) query = query.Where(e => e.EquipId.Contains(equipId.Trim()));
        if (!string.IsNullOrWhiteSpace(equipName)) query = query.Where(e => e.EquipName.Contains(equipName.Trim()));
        return await query.OrderBy(e => e.EquipId).ToListAsync();
    }

    /// 레시피 화면의 설비 선택 목록
    public async Task<List<string>> GetEquipIdsAsync()
    {
        await using var db = AppConfig.CreateDbContext();
        return await db.TbEquipments.AsNoTracking().OrderBy(e => e.EquipId).Select(e => e.EquipId).ToListAsync();
    }

    /// 신규 등록
    public async Task CreateAsync(string equipId, string equipName, string useYn)
    {
        equipId = Validate(equipId, equipName, useYn);

        await using var db = AppConfig.CreateDbContext();
        if (await db.TbEquipments.AnyAsync(e => e.EquipId == equipId))
            throw new BusinessException($"설비 ID '{equipId}'는 이미 등록되어 있습니다.");

        db.TbEquipments.Add(new TbEquipment { EquipId = equipId, EquipName = equipName.Trim(), UseYn = useYn });
        await SaveAsync(db);
    }

    /// 수정 (설비 ID는 PK라 바꾸지 않는다)
    public async Task UpdateAsync(string equipId, string equipName, string useYn)
    {
        equipId = Validate(equipId, equipName, useYn);

        await using var db = AppConfig.CreateDbContext();
        var equipment = await db.TbEquipments.FindAsync(equipId)
                        ?? throw new BusinessException($"설비 ID '{equipId}'를 찾을 수 없습니다. 다시 조회하세요.");
        equipment.EquipName = equipName.Trim();
        equipment.UseYn = useYn;
        equipment.UpdatedAt = DateTime.Now;
        await SaveAsync(db);
    }

    /// 삭제: 이 설비를 쓰는 레시피가 있으면 삭제하지 않는다 (사용 중인 설비)
    public async Task DeleteAsync(string equipId)
    {
        await using var db = AppConfig.CreateDbContext();
        var equipment = await db.TbEquipments.FindAsync(equipId)
                        ?? throw new BusinessException($"설비 ID '{equipId}'를 찾을 수 없습니다. 다시 조회하세요.");

        int recipeCount = await db.TbRmsRecipes.CountAsync(r => r.EquipId == equipId);
        if (recipeCount > 0)
            throw new BusinessException($"설비 '{equipId}'를 사용하는 레시피가 {recipeCount}건 있어 삭제할 수 없습니다.\n" +
                                        "레시피를 먼저 삭제하거나, 설비의 사용 여부를 'N'으로 변경하세요.");

        db.TbEquipments.Remove(equipment);
        await SaveAsync(db);
    }

    private static string Validate(string equipId, string equipName, string useYn)
    {
        equipId = (equipId ?? "").Trim();
        if (equipId.Length == 0) throw new BusinessException("설비 ID를 입력하세요.");
        if (equipId.Length > 50) throw new BusinessException("설비 ID는 50자 이하로 입력하세요.");
        if (string.IsNullOrWhiteSpace(equipName)) throw new BusinessException("설비명을 입력하세요.");
        if (equipName.Trim().Length > 100) throw new BusinessException("설비명은 100자 이하로 입력하세요.");
        if (useYn is not ("Y" or "N")) throw new BusinessException("사용 여부는 Y 또는 N입니다.");
        return equipId;
    }

    /// 저장 실패(제약조건 위반 등)를 사용자에게 보여 줄 메시지로 바꾼다
    internal static async Task SaveAsync(DbContext db)
    {
        try
        {
            await db.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.GetBaseException() is SqlException sql)
        {
            throw new BusinessException(sql.Number switch
            {
                2627 or 2601 => "이미 같은 값이 등록되어 있어 저장하지 못했습니다. (중복)\n" + sql.Message,
                547 => "다른 데이터가 참조하고 있어 처리하지 못했습니다. (FK 제약)\n" + sql.Message,
                _ => "DB 저장에 실패했습니다.\n" + sql.Message
            });
        }
    }
}
