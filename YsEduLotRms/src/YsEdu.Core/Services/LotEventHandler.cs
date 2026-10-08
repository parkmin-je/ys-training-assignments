using Microsoft.EntityFrameworkCore;
using YsEdu.Core.Data;
using YsEdu.Core.Data.Entities;
using YsEdu.Core.Messaging;

namespace YsEdu.Core.Services;

/// <summary>
/// 생산 이력 (2차 과제 p.4) — LOT 작업과 설비별 구동을 구분해서 저장한다.
///   LOT_START     : LOT 작업 시작 이력 및 상태 저장           → TB_LOT (RUN)
///   PROCESS_START : 설비별 구동 시작 이력 및 상태 저장         → TB_EQP_RUN 새 행 (RUN), 설비 RUN
///   PROCESS_END   : 같은 설비의 구동 시작과 연결하여 저장      → TB_EQP_RUN 같은 행 (DONE), 설비 IDLE
///   LOT_END       : LOT 작업 종료 이력 및 상태 저장           → TB_LOT (DONE)
/// 규칙
///   · B의 자체 LOT_START 수신 여부를 PROCESS 처리의 필수 조건으로 사용하지 않는다.
///   · 같은 설비의 START 없는 END는 ERROR.
///   · 호출한 쪽(MessageProcessor)이 연 트랜잭션 안에서 실행 → 상태와 이력이 함께 성공하거나 함께 취소.
/// </summary>
public sealed class LotEventHandler
{
    public HandleResult Handle(YsEduDbContext db, ProductionEvent e)
    {
        return e.EventType switch
        {
            EventTypes.LotStart => LotStart(db, e),
            EventTypes.ProcessStart => ProcessStart(db, e),
            EventTypes.ProcessEnd => ProcessEnd(db, e),
            EventTypes.LotEnd => LotEnd(db, e),
            _ => HandleResult.Error(Reasons.InvalidEventType)
        };
    }

    private static HandleResult LotStart(YsEduDbContext db, ProductionEvent e)
    {
        var lot = db.TbLots.Find(e.LotId);
        if (lot != null)
        {
            if (lot.LotStatus == "DONE") return HandleResult.Error(Reasons.LotAlreadyDone);
            if (lot.StartTime != null) return HandleResult.Error(Reasons.LotAlreadyStarted);
            if (lot.ProductId != e.ProductId || lot.StepId != e.StepId) return HandleResult.Error(Reasons.LotProductMismatch);

            // 다른 설비의 PROCESS가 먼저 도착해 LOT 행이 이미 있는 경우: 시작 정보만 채운다
            lot.StartTime = e.EventTime;
            lot.StartMsgId = e.MessageName;
            lot.CurEqpId = e.EqpId;
            lot.UpdatedAt = DateTime.Now;
            return HandleResult.Ok();
        }

        db.TbLots.Add(new TbLot
        {
            LotId = e.LotId,
            ProductId = e.ProductId,
            StepId = e.StepId,
            LotStatus = "RUN",
            CurEqpId = e.EqpId,
            StartTime = e.EventTime,
            StartMsgId = e.MessageName,
            UpdatedAt = DateTime.Now
        });
        return HandleResult.Ok();
    }

    private static HandleResult ProcessStart(YsEduDbContext db, ProductionEvent e)
    {
        var lot = db.TbLots.Find(e.LotId);
        if (lot != null)
        {
            if (lot.LotStatus == "DONE") return HandleResult.Error(Reasons.LotAlreadyDone);
            if (lot.ProductId != e.ProductId || lot.StepId != e.StepId) return HandleResult.Error(Reasons.LotProductMismatch);
        }
        else
        {
            // LOT_START를 받지 않은 LOT이어도 구동은 처리한다 (작업 상태 RUN, 시작 시간은 LOT_START가 오면 채움)
            lot = new TbLot { LotId = e.LotId, ProductId = e.ProductId, StepId = e.StepId, LotStatus = "RUN" };
            db.TbLots.Add(lot);
        }

        // 같은 LOT·STEP·설비에서 이미 구동 중이거나, 설비가 다른 LOT을 구동 중이면 순서 오류
        bool openRun = db.TbEqpRuns.Any(r => r.LotId == e.LotId && r.StepId == e.StepId && r.EqpId == e.EqpId && r.RunStatus == "RUN");
        var equipment = db.TbEquipments.Find(e.EqpId)!;
        if (openRun || equipment.EqpStatus == "RUN") return HandleResult.Error(Reasons.AlreadyRunning);

        // 구동 직전 OK 판정을 받은 RMS 검증을 연결 (재검증 이력 중 어느 판정으로 구동했는지 추적)
        long? checkId = db.TbRmsChecks
            .Where(c => c.LotId == e.LotId && c.StepId == e.StepId && c.EqpId == e.EqpId && c.Result == Results.OK)
            .OrderByDescending(c => c.CheckId)
            .Select(c => (long?)c.CheckId)
            .FirstOrDefault();

        var run = new TbEqpRun
        {
            LotId = e.LotId,
            ProductId = e.ProductId,
            StepId = e.StepId,
            EqpId = e.EqpId,
            RecipeId = e.RecipeId,
            RunStatus = "RUN",
            StartTime = e.EventTime,
            StartMsgId = e.MessageName,
            CheckId = checkId,
            UpdatedAt = DateTime.Now
        };
        db.TbEqpRuns.Add(run);

        equipment.EqpStatus = "RUN";
        equipment.CurLotId = e.LotId;
        equipment.UpdatedAt = DateTime.Now;
        lot.CurEqpId = e.EqpId;
        lot.UpdatedAt = DateTime.Now;

        db.SaveChanges();                       // RUN_ID(구동 식별키)를 받기 위해 트랜잭션 안에서 먼저 저장
        return HandleResult.Ok(runId: run.RunId);
    }

    private static HandleResult ProcessEnd(YsEduDbContext db, ProductionEvent e)
    {
        // 같은 LOT · STEP · 설비의 "구동 중(RUN)" 행 = 짝이 되는 PROCESS_START
        var run = db.TbEqpRuns
            .Where(r => r.LotId == e.LotId && r.StepId == e.StepId && r.EqpId == e.EqpId && r.RunStatus == "RUN")
            .OrderByDescending(r => r.RunId)
            .FirstOrDefault();
        if (run == null) return HandleResult.Error(Reasons.NoProcessStart);

        run.RunStatus = "DONE";
        run.EndTime = e.EventTime;
        run.EndMsgId = e.MessageName;
        run.UpdatedAt = DateTime.Now;

        var equipment = db.TbEquipments.Find(e.EqpId)!;
        equipment.EqpStatus = "IDLE";
        equipment.CurLotId = null;
        equipment.UpdatedAt = DateTime.Now;

        var lot = db.TbLots.Find(e.LotId);
        if (lot != null)
        {
            lot.CurEqpId = e.EqpId;
            lot.UpdatedAt = DateTime.Now;
        }
        return HandleResult.Ok(runId: run.RunId);
    }

    private static HandleResult LotEnd(YsEduDbContext db, ProductionEvent e)
    {
        var lot = db.TbLots.Find(e.LotId);
        if (lot == null) return HandleResult.Error(Reasons.LotNotFound);
        if (lot.LotStatus == "DONE") return HandleResult.Error(Reasons.LotAlreadyDone);
        if (lot.ProductId != e.ProductId || lot.StepId != e.StepId) return HandleResult.Error(Reasons.LotProductMismatch);
        if (db.TbEqpRuns.Any(r => r.LotId == e.LotId && r.RunStatus == "RUN")) return HandleResult.Error(Reasons.EquipmentStillRunning);

        lot.LotStatus = "DONE";
        lot.EndTime = e.EventTime;
        lot.EndMsgId = e.MessageName;
        lot.CurEqpId = e.EqpId;
        lot.UpdatedAt = DateTime.Now;
        return HandleResult.Ok();
    }
}
