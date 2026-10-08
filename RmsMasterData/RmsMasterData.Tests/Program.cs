using Microsoft.EntityFrameworkCore;
using RmsMasterData.Common;
using RmsMasterData.Data.Entities;
using RmsMasterData.Services;

// ============================================================================
// RMS 기준정보 관리 — 검증 결과 (기준정보관리 생성.pptx p.6·9)
//   중복 입력 · 삭제 · 저장 실패 상황의 처리 결과를 실제 DB(YsRmsDB)로 확인한다.
//   시험 데이터는 TST_ 로 시작하고, 끝나면 지운다.
// ============================================================================
Console.OutputEncoding = System.Text.Encoding.UTF8;
var equipments = new EquipmentService();
var recipes = new RecipeService();
int pass = 0, fail = 0;

void Check(bool ok, string text)
{
    if (ok) pass++; else fail++;
    Console.WriteLine($"{(ok ? "  ✔" : "  ✘")} {text}");
}

async Task Expect(string text, Func<Task> action)
{
    try
    {
        await action();
        Check(false, $"{text} — 예외가 나야 하는데 저장됨");
    }
    catch (BusinessException ex)
    {
        Check(true, $"{text} → \"{ex.Message.Split('\n')[0]}\"");
    }
}

async Task Cleanup()
{
    await using var db = AppConfig.CreateDbContext();
    await db.TbRmsRecipeParameters.Where(p => p.RecipeId.StartsWith("TST_")).ExecuteDeleteAsync();
    await db.TbRmsRecipes.Where(r => r.RecipeId.StartsWith("TST_")).ExecuteDeleteAsync();
    await db.TbEquipments.Where(e => e.EquipId.StartsWith("TST_")).ExecuteDeleteAsync();
}

List<ParameterRow> P(params (string Id, string Value)[] rows)
    => rows.Select(r => new ParameterRow { ParameterId = r.Id, ParameterValue = r.Value }).ToList();

await Cleanup();

Console.WriteLine("■ 설비 기준정보");
await equipments.CreateAsync("TST_EQP_1", "시험 설비 1", "Y");
await equipments.CreateAsync("TST_EQP_2", "시험 설비 2", "Y");
Check((await equipments.SearchAsync("TST_EQP", null)).Count == 2, "신규 등록 2건 → 조회 2건");
await Expect("설비 ID 중복 등록", () => equipments.CreateAsync("TST_EQP_1", "중복", "Y"));
await Expect("설비명 누락", () => equipments.CreateAsync("TST_EQP_9", " ", "Y"));
await equipments.UpdateAsync("TST_EQP_2", "시험 설비 2(수정)", "N");
var eq2 = (await equipments.SearchAsync("TST_EQP_2", null)).Single();
Check(eq2.EquipName == "시험 설비 2(수정)" && eq2.UseYn == "N" && eq2.UpdatedAt != null, "수정 → 설비명·사용 여부·수정 일시 반영");

Console.WriteLine("\n■ 레시피 Master–Detail");
await recipes.CreateAsync(new RecipeInput("TST_RCP_1", "TST_EQP_1", "MODEL", "A100", "Y"),
                          P(("TEMP_LIMIT", "85"), ("PRESS_LIMIT", "3.2"), ("SPEED", "450")));
Check((await recipes.GetParametersAsync("TST_RCP_1")).Count == 3, "레시피 + 파라미터 3건을 한 번에 등록");
await Expect("레시피 ID 중복", () => recipes.CreateAsync(new RecipeInput("TST_RCP_1", "TST_EQP_1", "MODEL", "Z999", "Y"), P(("A", "1"))));
await Expect("설비·팩터 ID·팩터 값 조합 중복", () => recipes.CreateAsync(new RecipeInput("TST_RCP_2", "TST_EQP_1", "MODEL", "A100", "Y"), P(("A", "1"))));
await Expect("같은 레시피에 파라미터 ID 중복", () => recipes.CreateAsync(new RecipeInput("TST_RCP_3", "TST_EQP_1", "MODEL", "C300", "Y"), P(("TEMP", "1"), ("TEMP", "2"))));
Check((await recipes.SearchAsync(null, "TST_RCP_3")).Count == 0, "파라미터 중복으로 거부된 레시피는 마스터도 저장되지 않음");
await Expect("등록되지 않은 설비로 레시피 등록", () => recipes.CreateAsync(new RecipeInput("TST_RCP_4", "TST_NONE", "MODEL", "D400", "Y"), P(("A", "1"))));

// 수정: 팩터 값 변경 + 파라미터 1건 값 변경 + 1건 삭제 + 1건 추가를 한 번에
await recipes.UpdateAsync(new RecipeInput("TST_RCP_1", "TST_EQP_1", "MODEL", "A101", "Y"),
                          P(("TEMP_LIMIT", "90"), ("PRESS_LIMIT", "3.2"), ("HUMIDITY", "40")));
var after = await recipes.GetParametersAsync("TST_RCP_1");
Check(after.Count == 3 && after.Any(p => p.ParameterId == "TEMP_LIMIT" && p.ParameterValue == "90")
      && after.All(p => p.ParameterId != "SPEED") && after.Any(p => p.ParameterId == "HUMIDITY"),
      $"수정: 값 변경·삭제·추가가 함께 반영 ({string.Join(", ", after.Select(p => $"{p.ParameterId}={p.ParameterValue}"))})");
Check((await recipes.SearchAsync("TST_EQP_1", "TST_RCP_1")).Single().FactorValue == "A101", "수정: 팩터 값 A100 → A101");

Console.WriteLine("\n■ 저장 실패 시 하나의 작업으로 취소");
try
{
    await using var db = AppConfig.CreateDbContext();
    var bad = new TbRmsRecipe { RecipeId = "TST_RCP_5", EquipId = "TST_EQP_1", FactorId = "MODEL", FactorValue = "E500", UseYn = "Y" };
    bad.TbRmsRecipeParameters.Add(new TbRmsRecipeParameter { ParameterId = "OK", ParameterValue = "1" });
    bad.TbRmsRecipeParameters.Add(new TbRmsRecipeParameter { ParameterId = "TOO_LONG", ParameterValue = new string('9', 600) });   // 컬럼 500자 초과
    db.TbRmsRecipes.Add(bad);
    await db.SaveChangesAsync();
    Check(false, "디테일 저장 실패인데 저장됨");
}
catch (DbUpdateException)
{
    Check((await recipes.SearchAsync(null, "TST_RCP_5")).Count == 0 && (await recipes.GetParametersAsync("TST_RCP_5")).Count == 0,
          "디테일 1건이 DB에서 실패 → 마스터와 다른 디테일도 저장되지 않음 (SaveChanges 1회 = 트랜잭션 1개)");
}

Console.WriteLine("\n■ 사용 중인 설비와 연결 데이터 삭제");
await Expect("레시피가 연결된 설비 삭제", () => equipments.DeleteAsync("TST_EQP_1"));
Check((await recipes.SearchAsync("TST_EQP_1", null)).Count == 1, "설비 삭제 거부 후 레시피 그대로 유지");
await recipes.DeleteAsync("TST_RCP_1");
await using (var db = AppConfig.CreateDbContext())
    Check(!await db.TbRmsRecipeParameters.AnyAsync(p => p.RecipeId == "TST_RCP_1"), "레시피 삭제 → 연결된 파라미터도 함께 삭제");
await equipments.DeleteAsync("TST_EQP_1");
Check((await equipments.SearchAsync("TST_EQP_1", null)).Count == 0, "연결 레시피가 없어진 설비는 삭제됨");

await Cleanup();
Console.WriteLine($"\nRESULT: {(fail == 0 ? "PASS" : "FAIL")}  (통과 {pass} / 실패 {fail})");
return fail == 0 ? 0 : 1;
