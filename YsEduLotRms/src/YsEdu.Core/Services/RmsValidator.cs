using System.Globalization;
using Microsoft.EntityFrameworkCore;
using YsEdu.Core.Data;
using YsEdu.Core.Data.Entities;
using YsEdu.Core.Messaging;

namespace YsEdu.Core.Services;

/// <summary>
/// RMS 구동 전 검증 (2차 과제 p.8)
///   기준 조회 : ProductID · STEPID · EQPID로 기준 Recipe를 조회한다.          (없으면 ERROR)
///   비교      : RecipeID와 각 Parameter를 비교한다.
///   판정 규칙 : 숫자는 Double로 허용오차 없이 비교한다.
///               전체 일치 OK / ID·값 불일치 또는 항목 구성 오류(누락·추가·중복) NG
///               숫자 변환 오류·미등록 정보·DB 처리 실패는 ERROR
///   저장      : 종합(TB_RMS_CHECK) + 항목별 상세(TB_RMS_CHECK_DTL) — 검증 당시 기준값·수신값·판정·사유
/// "RecipeID 확인과 Parameter 비교는 모두 필요하다. ID가 같아도 설정값이 다를 수 있다." (V2 p.18)
/// </summary>
public sealed class RmsValidator
{
    private sealed record Item(string Type, string Id, string? BaseValue, string? RecvValue, string Judge, string Reason);

    public HandleResult Validate(YsEduDbContext db, ProductionEvent e)
    {
        // 1) 기준 조회
        var basis = db.TbRecipes
            .Include(r => r.TbRecipeParams)
            .FirstOrDefault(r => r.ProductId == e.ProductId && r.StepId == e.StepId && r.EqpId == e.EqpId && r.UseYn == "Y");
        if (basis == null) return HandleResult.Error(Reasons.NoBaseRecipe);

        var items = new List<Item>();

        // 2) RecipeID 비교
        bool sameRecipe = e.RecipeId == basis.RecipeId;
        items.Add(new Item("RECIPE", "RecipeID", basis.RecipeId, e.RecipeId,
                           sameRecipe ? Results.OK : Results.NG, sameRecipe ? "" : "RECIPE_ID_MISMATCH"));

        var received = e.Parameters.GroupBy(p => p.ParameterId).ToDictionary(g => g.Key, g => g.ToList());

        // 3) 기준 파라미터마다 비교 (누락·중복·값)
        foreach (var bp in basis.TbRecipeParams.OrderBy(p => p.ParamId))
        {
            if (!received.TryGetValue(bp.ParamId, out var values))
            {
                items.Add(new Item("PARAM", bp.ParamId, bp.BaseValue, null, Results.NG, $"{bp.ParamId}_MISSING"));
                continue;
            }
            if (values.Count > 1)
            {
                items.Add(new Item("PARAM", bp.ParamId, bp.BaseValue, string.Join("/", values.Select(v => v.Value)),
                                   Results.NG, $"{bp.ParamId}_DUPLICATED"));
                continue;
            }

            string recv = values[0].Value;
            if (!TryNumber(bp.BaseValue, out double baseValue) || !TryNumber(recv, out double recvValue))
                return HandleResult.Error($"{bp.ParamId}_VALUE_FORMAT_ERROR");      // 숫자 변환 오류 = ERROR

            bool same = baseValue == recvValue;                                     // 허용오차 없이
            items.Add(new Item("PARAM", bp.ParamId, bp.BaseValue, recv, same ? Results.OK : Results.NG,
                               same ? "" : $"{bp.ParamId}_MISMATCH"));
        }

        // 4) 기준에 없는 항목 = 구성 오류 (추가)
        var known = basis.TbRecipeParams.Select(p => p.ParamId).ToHashSet();
        foreach (var (paramId, values) in received.Where(r => !known.Contains(r.Key)))
            items.Add(new Item("PARAM", paramId, null, string.Join("/", values.Select(v => v.Value)), Results.NG, $"{paramId}_NOT_DEFINED"));

        // 5) 종합 판정
        string result = items.Any(i => i.Judge == Results.NG) ? Results.NG : Results.OK;
        string reason = string.Join(",", items.Where(i => i.Judge == Results.NG).Select(i => i.Reason));

        // 6) 종합 + 상세 저장 (호출한 쪽 트랜잭션 안)
        var check = new TbRmsCheck
        {
            MessageId = e.MessageName,
            ProductId = e.ProductId,
            LotId = e.LotId,
            StepId = e.StepId,
            EqpId = e.EqpId,
            RecvRecipeId = e.RecipeId,
            BaseRecipeId = basis.RecipeId,
            Result = result,
            Reason = reason,
            EventTime = e.EventTime
        };
        foreach (var i in items)
        {
            check.TbRmsCheckDtls.Add(new TbRmsCheckDtl
            {
                ItemType = i.Type, ItemId = i.Id, BaseValue = i.BaseValue, RecvValue = i.RecvValue, Judge = i.Judge, Reason = i.Reason
            });
        }
        db.TbRmsChecks.Add(check);
        db.SaveChanges();                       // CHECK_ID(검증 식별키)를 받기 위해 먼저 저장

        return new HandleResult(result, reason, CheckId: check.CheckId);
    }

    private static bool TryNumber(string? text, out double value)
        => double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
}
