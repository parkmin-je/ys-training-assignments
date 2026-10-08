using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using RmsResultViewer.Services;

// ============================================================================
// RMS 결과 조회 검증 (결과조회 생성 p.6·7·8)
//   실제 DB(YsRmsDB)를 사용한다. 시험 데이터는 TST_ 로 시작하고 2001년 시각을 써서
//   실제 결과와 섞이지 않게 하며, 시작·종료 시 정리한다.
// ============================================================================
Console.OutputEncoding = System.Text.Encoding.UTF8;
int pass = 0, fail = 0;

void Check(bool ok, string text)
{
    if (ok) pass++; else fail++;
    Console.WriteLine($"{(ok ? "  ✔" : "  ✘")} {text}");
}

string cs = new ConfigurationBuilder().SetBasePath(AppContext.BaseDirectory).AddJsonFile("appsettings.json").Build()
    .GetConnectionString("YsRmsDB")!;

void Exec(string sql)
{
    using var conn = new SqlConnection(cs);
    conn.Open();
    using var cmd = new SqlCommand(sql, conn);
    cmd.ExecuteNonQuery();
}

long InsertResult(string time, string equipId, string recipeId, params (string Id, string Value)[] parameters)
{
    using var conn = new SqlConnection(cs);
    conn.Open();
    using var cmd = new SqlCommand(
        "INSERT INTO dbo.TB_RMS_RESULT (EVENT_TIME, EQUIP_ID, RECIPE_ID) OUTPUT INSERTED.RESULT_ID VALUES (@t, @e, @r)", conn);
    cmd.Parameters.AddWithValue("@t", DateTime.Parse(time));
    cmd.Parameters.AddWithValue("@e", equipId);
    cmd.Parameters.AddWithValue("@r", recipeId);
    long id = (long)cmd.ExecuteScalar()!;
    foreach (var p in parameters)
        Exec($"INSERT INTO dbo.TB_RMS_RESULT_PARAMETER (RESULT_ID, PARAMETER_ID, PARAMETER_VALUE) VALUES ({id}, N'{p.Id}', N'{p.Value}')");
    return id;
}

void Cleanup()
{
    Exec("DELETE FROM dbo.TB_RMS_RESULT WHERE EQUIP_ID LIKE N'TST[_]%'");   // 파라미터는 ON DELETE CASCADE
    Exec("DELETE FROM dbo.TB_RMS_RECIPE WHERE EQUIP_ID LIKE N'TST[_]%'");
    Exec("DELETE FROM dbo.TB_EQUIPMENT WHERE EQUIP_ID LIKE N'TST[_]%'");
}

Cleanup();
Exec("""
    INSERT INTO dbo.TB_EQUIPMENT (EQUIP_ID, EQUIP_NAME) VALUES (N'TST_EQP_V1', N'조회 시험 1'), (N'TST_EQP_V2', N'조회 시험 2');
    INSERT INTO dbo.TB_RMS_RECIPE (RECIPE_ID, EQUIP_ID, FACTOR_ID, FACTOR_VALUE)
    VALUES (N'TST_RCP_V1', N'TST_EQP_V1', N'MODEL', N'V100'), (N'TST_RCP_V2', N'TST_EQP_V2', N'MODEL', N'V200');
    """);

long r1 = InsertResult("2001-01-01 10:00:00.000", "TST_EQP_V1", "TST_RCP_V1", ("TEMP_LIMIT", "85"), ("PRESS_LIMIT", "3.2"));
long r2 = InsertResult("2001-01-01 10:00:01.123", "TST_EQP_V1", "TST_RCP_V1", ("TEMP_LIMIT", "86"));
long r3 = InsertResult("2001-01-01 11:00:00.000", "TST_EQP_V2", "TST_RCP_V2", ("TEMP_LIMIT", "70"));
long r4 = InsertResult("2001-01-02 09:00:00.000", "TST_EQP_V1", "TST_RCP_V1", ("TEMP_LIMIT", "90"));

var service = new ResultQueryService();
var day1From = new DateTime(2001, 1, 1, 0, 0, 0);
var day1To = new DateTime(2001, 1, 1, 23, 59, 59);

Console.WriteLine("■ Master 조회 — 기간 (p.6·8)");
var all = await service.SearchAsync(new ResultFilter(day1From, day1To, null, null));
Check(all.Select(r => r.ResultId).SequenceEqual([r3, r2, r1]), "기간 안 3건만 조회 (다음 날 결과는 제외), 최신 EVENT_TIME 먼저");

var boundary = await service.SearchAsync(new ResultFilter(new DateTime(2001, 1, 1, 10, 0, 0), new DateTime(2001, 1, 1, 10, 0, 1), null, null));
Check(boundary.Select(r => r.ResultId).SequenceEqual([r2, r1]),
      "종료 시각 10:00:01 → 같은 초의 10:00:01.123 결과도 포함 (밀리초 경계)");

var none = await service.SearchAsync(new ResultFilter(new DateTime(2001, 1, 3), new DateTime(2001, 1, 3, 23, 59, 59), null, null));
Check(none.Count == 0, "결과 없는 기간 → 빈 목록 (화면은 빈 Grid + 안내)");

try
{
    await service.SearchAsync(new ResultFilter(day1To, day1From, null, null));
    Check(false, "시작 > 종료인데 조회됨");
}
catch (ArgumentException ex)
{
    Check(true, $"시작 시간이 종료보다 늦으면 조회 거부 → \"{ex.Message}\"");
}

Console.WriteLine("\n■ Master 조회 — 설비 ID · 레시피 ID · 복수 조건 (p.6)");
Check((await service.SearchAsync(new ResultFilter(day1From, day1To, "TST_EQP_V2", null))).Select(r => r.ResultId).SequenceEqual([r3]),
      "설비 ID 조건 → 해당 설비 결과만");
Check((await service.SearchAsync(new ResultFilter(day1From, day1To, null, "TST_RCP_V1"))).Select(r => r.ResultId).SequenceEqual([r2, r1]),
      "레시피 ID 조건 → 해당 레시피 결과만");
Check((await service.SearchAsync(new ResultFilter(day1From, day1To, "TST_EQP_V1", "TST_RCP_V2"))).Count == 0,
      "설비 + 레시피 복수 조건을 함께 적용 (조합 없음 → 0건)");
Check((await service.SearchAsync(new ResultFilter(day1From, day1To, " TST_EQP_V1 ", null))).Count == 2,
      "조건 앞뒤 공백은 무시");

Console.WriteLine("\n■ Detail 조회 — 선택한 RESULT_ID의 파라미터 (p.7)");
var detail = await service.GetParametersAsync(r1);
Check(detail.Count == 2 && detail.Any(p => p.ParameterId == "TEMP_LIMIT" && p.ParameterValue == "85")
      && detail.Any(p => p.ParameterId == "PRESS_LIMIT" && p.ParameterValue == "3.2"),
      "RESULT_ID로 Master와 연결된 파라미터 2건");
Check((await service.GetParametersAsync(r3)).Single().ParameterValue == "70", "다른 결과를 선택하면 그 결과의 파라미터만");

Cleanup();
Console.WriteLine($"\nRESULT: {(fail == 0 ? "PASS" : "FAIL")}  (통과 {pass} / 실패 {fail})");
return fail == 0 ? 0 : 1;
