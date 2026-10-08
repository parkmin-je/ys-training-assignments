/* ============================================================================
   03. 초기 기준정보 (2차 과제 p.3 생산 시나리오와 초기 데이터)
     제품 / LOT / STEP : PRODUCT_A / LOT_001 / STEP_020
     인라인 설비       : A=EQP_A, B=EQP_B, C=EQP_C
     기준 Recipe       : A=RCP_A01, B=RCP_B01, C=RCP_C01
     파라미터 기준값   : 각 Recipe에 TEMP=120, PRESSURE=30, SPEED=450
   실행할 때마다 이력을 비우고 기준정보를 처음 상태로 되돌린다.
   ============================================================================ */
USE YsEduLotDB;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

SET XACT_ABORT ON;
BEGIN TRAN;
    DELETE FROM dbo.TB_MSG_LOG;
    DELETE FROM dbo.TB_MSG_PROCESSED;
    DELETE FROM dbo.TB_RMS_CHECK_DTL;
    DELETE FROM dbo.TB_RMS_CHECK;
    DELETE FROM dbo.TB_LOT_EVENT;
    DELETE FROM dbo.TB_EQP_RUN;
    DELETE FROM dbo.TB_LOT;
    DELETE FROM dbo.TB_RECIPE_PARAM;
    DELETE FROM dbo.TB_RECIPE;
    DELETE FROM dbo.TB_EQUIPMENT;
    DELETE FROM dbo.TB_STEP;
    DELETE FROM dbo.TB_PRODUCT;

    INSERT INTO dbo.TB_PRODUCT (PRODUCT_ID, PRODUCT_NAME) VALUES (N'PRODUCT_A', N'교육용 제품 A');
    INSERT INTO dbo.TB_STEP (STEP_ID, STEP_NAME) VALUES (N'STEP_020', N'인라인 공정 020');

    INSERT INTO dbo.TB_EQUIPMENT (EQP_ID, EQP_NAME, LINE_ID)
    VALUES (N'EQP_A', N'인라인 설비 A', N'LINE_01'),
           (N'EQP_B', N'인라인 설비 B', N'LINE_01'),
           (N'EQP_C', N'인라인 설비 C', N'LINE_01');

    INSERT INTO dbo.TB_RECIPE (RECIPE_ID, PRODUCT_ID, STEP_ID, EQP_ID)
    VALUES (N'RCP_A01', N'PRODUCT_A', N'STEP_020', N'EQP_A'),
           (N'RCP_B01', N'PRODUCT_A', N'STEP_020', N'EQP_B'),
           (N'RCP_C01', N'PRODUCT_A', N'STEP_020', N'EQP_C');

    INSERT INTO dbo.TB_RECIPE_PARAM (RECIPE_ID, PARAM_ID, BASE_VALUE)
    VALUES (N'RCP_A01', N'TEMP', N'120'), (N'RCP_A01', N'PRESSURE', N'30'), (N'RCP_A01', N'SPEED', N'450'),
           (N'RCP_B01', N'TEMP', N'120'), (N'RCP_B01', N'PRESSURE', N'30'), (N'RCP_B01', N'SPEED', N'450'),
           (N'RCP_C01', N'TEMP', N'120'), (N'RCP_C01', N'PRESSURE', N'30'), (N'RCP_C01', N'SPEED', N'450');
COMMIT;
GO

SELECT R.RECIPE_ID, R.PRODUCT_ID, R.STEP_ID, R.EQP_ID, P.PARAM_ID, P.BASE_VALUE
FROM dbo.TB_RECIPE R JOIN dbo.TB_RECIPE_PARAM P ON P.RECIPE_ID = R.RECIPE_ID
ORDER BY R.EQP_ID, P.PARAM_ID;
GO
