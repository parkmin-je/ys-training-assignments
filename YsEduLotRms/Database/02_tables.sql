/* ============================================================================
   02. 테이블 생성
   ----------------------------------------------------------------------------
   [기준정보]  처리할 때 참조하는 사전 정의 — 관리자가 등록·수정
     TB_PRODUCT, TB_STEP, TB_EQUIPMENT, TB_RECIPE, TB_RECIPE_PARAM
   [현재 상태] "지금 어떤 상태인가?" — 행을 수정(UPDATE)
     TB_LOT (LOT 작업 상태), TB_EQUIPMENT.EQP_STATUS (설비 구동 상태)
   [이력정보]  "언제 어떤 일이 발생했는가?" — 이벤트마다 새 행(INSERT)
     TB_LOT_EVENT     : 반영된 모든 이벤트 (LOT·PROCESS·RMS_CHECK)
     TB_EQP_RUN       : 설비별 구동 1회 = 1행 (RUN_ID = 구동 식별키, START·END 연결)
     TB_RMS_CHECK     : RMS 검증 1회 = 1행 (CHECK_ID = 검증 식별키, 종합 판정)
     TB_RMS_CHECK_DTL : 항목별 상세 (검증 당시 기준값·수신값·판정·사유)
   [메시지]
     TB_MSG_PROCESSED : 반영 완료한 MessageName (PK) — 동일 MessageName 중복 반영 방지
     TB_MSG_LOG       : 송수신 원문 XML과 처리결과 (ERROR 사유 확인용)
   ============================================================================ */
USE YsEduLotDB;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;   -- 필터 인덱스(UX_TB_EQP_RUN_OPEN)에 필요. SSMS는 기본 ON, sqlcmd는 기본 OFF
GO

DROP TABLE IF EXISTS dbo.TB_MSG_LOG;
DROP TABLE IF EXISTS dbo.TB_MSG_PROCESSED;
DROP TABLE IF EXISTS dbo.TB_RMS_CHECK_DTL;
DROP TABLE IF EXISTS dbo.TB_RMS_CHECK;
DROP TABLE IF EXISTS dbo.TB_LOT_EVENT;
DROP TABLE IF EXISTS dbo.TB_EQP_RUN;
DROP TABLE IF EXISTS dbo.TB_LOT;
DROP TABLE IF EXISTS dbo.TB_RECIPE_PARAM;
DROP TABLE IF EXISTS dbo.TB_RECIPE;
DROP TABLE IF EXISTS dbo.TB_EQUIPMENT;
DROP TABLE IF EXISTS dbo.TB_STEP;
DROP TABLE IF EXISTS dbo.TB_PRODUCT;
GO

/* ---------------------------------------------------------------------------
   기준정보
   --------------------------------------------------------------------------- */
CREATE TABLE dbo.TB_PRODUCT
(
    PRODUCT_ID    NVARCHAR(50)  NOT NULL,
    PRODUCT_NAME  NVARCHAR(100) NOT NULL,
    USE_YN        CHAR(1)       NOT NULL CONSTRAINT DF_TB_PRODUCT_USE_YN DEFAULT ('Y'),
    CREATED_AT    DATETIME2(3)  NOT NULL CONSTRAINT DF_TB_PRODUCT_CREATED_AT DEFAULT (SYSDATETIME()),
    CONSTRAINT PK_TB_PRODUCT PRIMARY KEY (PRODUCT_ID),
    CONSTRAINT CK_TB_PRODUCT_USE_YN CHECK (USE_YN IN ('Y', 'N'))
);

CREATE TABLE dbo.TB_STEP
(
    STEP_ID     NVARCHAR(50)  NOT NULL,
    STEP_NAME   NVARCHAR(100) NOT NULL,
    USE_YN      CHAR(1)       NOT NULL CONSTRAINT DF_TB_STEP_USE_YN DEFAULT ('Y'),
    CREATED_AT  DATETIME2(3)  NOT NULL CONSTRAINT DF_TB_STEP_CREATED_AT DEFAULT (SYSDATETIME()),
    CONSTRAINT PK_TB_STEP PRIMARY KEY (STEP_ID),
    CONSTRAINT CK_TB_STEP_USE_YN CHECK (USE_YN IN ('Y', 'N'))
);

/* 설비 기준정보 + 현재 구동 상태 */
CREATE TABLE dbo.TB_EQUIPMENT
(
    EQP_ID      NVARCHAR(50)  NOT NULL,
    EQP_NAME    NVARCHAR(100) NOT NULL,
    LINE_ID     NVARCHAR(50)  NOT NULL,
    EQP_STATUS  NVARCHAR(10)  NOT NULL CONSTRAINT DF_TB_EQUIPMENT_EQP_STATUS DEFAULT (N'IDLE'),  -- IDLE / RUN
    CUR_LOT_ID  NVARCHAR(50)  NULL,                                                             -- 구동 중인 LOT
    USE_YN      CHAR(1)       NOT NULL CONSTRAINT DF_TB_EQUIPMENT_USE_YN DEFAULT ('Y'),
    CREATED_AT  DATETIME2(3)  NOT NULL CONSTRAINT DF_TB_EQUIPMENT_CREATED_AT DEFAULT (SYSDATETIME()),
    UPDATED_AT  DATETIME2(3)  NULL,
    CONSTRAINT PK_TB_EQUIPMENT PRIMARY KEY (EQP_ID),
    CONSTRAINT CK_TB_EQUIPMENT_USE_YN CHECK (USE_YN IN ('Y', 'N')),
    CONSTRAINT CK_TB_EQUIPMENT_EQP_STATUS CHECK (EQP_STATUS IN (N'IDLE', N'RUN'))
);

/* 기준 Recipe: 제품 · STEP · 설비 조합당 1개 */
CREATE TABLE dbo.TB_RECIPE
(
    RECIPE_ID   NVARCHAR(50)  NOT NULL,
    PRODUCT_ID  NVARCHAR(50)  NOT NULL,
    STEP_ID     NVARCHAR(50)  NOT NULL,
    EQP_ID      NVARCHAR(50)  NOT NULL,
    USE_YN      CHAR(1)       NOT NULL CONSTRAINT DF_TB_RECIPE_USE_YN DEFAULT ('Y'),
    CREATED_AT  DATETIME2(3)  NOT NULL CONSTRAINT DF_TB_RECIPE_CREATED_AT DEFAULT (SYSDATETIME()),
    UPDATED_AT  DATETIME2(3)  NULL,
    CONSTRAINT PK_TB_RECIPE PRIMARY KEY (RECIPE_ID),
    CONSTRAINT FK_TB_RECIPE_PRODUCT FOREIGN KEY (PRODUCT_ID) REFERENCES dbo.TB_PRODUCT (PRODUCT_ID),
    CONSTRAINT FK_TB_RECIPE_STEP FOREIGN KEY (STEP_ID) REFERENCES dbo.TB_STEP (STEP_ID),
    CONSTRAINT FK_TB_RECIPE_EQUIPMENT FOREIGN KEY (EQP_ID) REFERENCES dbo.TB_EQUIPMENT (EQP_ID),
    CONSTRAINT UQ_TB_RECIPE_CONDITION UNIQUE (PRODUCT_ID, STEP_ID, EQP_ID),
    CONSTRAINT CK_TB_RECIPE_USE_YN CHECK (USE_YN IN ('Y', 'N'))
);

/* Recipe별 ParameterID와 기준값 (문자열 보관, 비교할 때 Double 변환) */
CREATE TABLE dbo.TB_RECIPE_PARAM
(
    RECIPE_ID   NVARCHAR(50)  NOT NULL,
    PARAM_ID    NVARCHAR(50)  NOT NULL,
    BASE_VALUE  NVARCHAR(100) NOT NULL,
    CREATED_AT  DATETIME2(3)  NOT NULL CONSTRAINT DF_TB_RECIPE_PARAM_CREATED_AT DEFAULT (SYSDATETIME()),
    CONSTRAINT PK_TB_RECIPE_PARAM PRIMARY KEY (RECIPE_ID, PARAM_ID),
    CONSTRAINT FK_TB_RECIPE_PARAM_RECIPE FOREIGN KEY (RECIPE_ID) REFERENCES dbo.TB_RECIPE (RECIPE_ID) ON DELETE CASCADE
);
GO

/* ---------------------------------------------------------------------------
   현재 상태 — LOT 작업
   --------------------------------------------------------------------------- */
CREATE TABLE dbo.TB_LOT
(
    LOT_ID        NVARCHAR(50)        NOT NULL,
    PRODUCT_ID    NVARCHAR(50)        NOT NULL,
    STEP_ID       NVARCHAR(50)        NOT NULL,
    LOT_STATUS    NVARCHAR(10)        NOT NULL,      -- RUN(작업 중) / DONE(작업 종료)
    CUR_EQP_ID    NVARCHAR(50)        NULL,          -- 마지막으로 이벤트를 보낸 설비
    START_TIME    DATETIMEOFFSET(3)   NULL,          -- LOT_START 발생시간
    END_TIME      DATETIMEOFFSET(3)   NULL,          -- LOT_END 발생시간
    START_MSG_ID  NVARCHAR(50)        NULL,
    END_MSG_ID    NVARCHAR(50)        NULL,
    CREATED_AT    DATETIME2(3)        NOT NULL CONSTRAINT DF_TB_LOT_CREATED_AT DEFAULT (SYSDATETIME()),
    UPDATED_AT    DATETIME2(3)        NOT NULL CONSTRAINT DF_TB_LOT_UPDATED_AT DEFAULT (SYSDATETIME()),
    CONSTRAINT PK_TB_LOT PRIMARY KEY (LOT_ID),
    CONSTRAINT CK_TB_LOT_STATUS CHECK (LOT_STATUS IN (N'RUN', N'DONE'))
);
GO

/* ---------------------------------------------------------------------------
   이력 — 설비별 구동 (PROCESS_START 1건 + PROCESS_END 1건 = 1행)
   --------------------------------------------------------------------------- */
CREATE TABLE dbo.TB_EQP_RUN
(
    RUN_ID        BIGINT              IDENTITY(1,1) NOT NULL,   -- 구동 식별키 (같은 LOT·설비 반복 구동 구분)
    LOT_ID        NVARCHAR(50)        NOT NULL,
    PRODUCT_ID    NVARCHAR(50)        NOT NULL,
    STEP_ID       NVARCHAR(50)        NOT NULL,
    EQP_ID        NVARCHAR(50)        NOT NULL,
    RECIPE_ID     NVARCHAR(50)        NOT NULL,
    RUN_STATUS    NVARCHAR(10)        NOT NULL,                 -- RUN(구동 중) / DONE(구동 종료)
    START_TIME    DATETIMEOFFSET(3)   NOT NULL,
    END_TIME      DATETIMEOFFSET(3)   NULL,
    START_MSG_ID  NVARCHAR(50)        NOT NULL,
    END_MSG_ID    NVARCHAR(50)        NULL,
    CHECK_ID      BIGINT              NULL,                     -- 구동 직전 OK 판정을 받은 RMS 검증
    CREATED_AT    DATETIME2(3)        NOT NULL CONSTRAINT DF_TB_EQP_RUN_CREATED_AT DEFAULT (SYSDATETIME()),
    UPDATED_AT    DATETIME2(3)        NOT NULL CONSTRAINT DF_TB_EQP_RUN_UPDATED_AT DEFAULT (SYSDATETIME()),
    CONSTRAINT PK_TB_EQP_RUN PRIMARY KEY (RUN_ID),
    CONSTRAINT CK_TB_EQP_RUN_STATUS CHECK (RUN_STATUS IN (N'RUN', N'DONE'))
);
/* 같은 LOT·STEP·설비에서 동시에 열린(RUN) 구동은 1건만 */
CREATE UNIQUE INDEX UX_TB_EQP_RUN_OPEN ON dbo.TB_EQP_RUN (LOT_ID, STEP_ID, EQP_ID) WHERE RUN_STATUS = N'RUN';
CREATE INDEX IX_TB_EQP_RUN_LOT ON dbo.TB_EQP_RUN (LOT_ID, START_TIME);
GO

/* ---------------------------------------------------------------------------
   이력 — RMS 검증 (종합 1행 + 항목별 상세 N행)
   --------------------------------------------------------------------------- */
CREATE TABLE dbo.TB_RMS_CHECK
(
    CHECK_ID        BIGINT              IDENTITY(1,1) NOT NULL,  -- 검증 식별키 (재검증 구분)
    MESSAGE_ID      NVARCHAR(50)        NOT NULL,
    PRODUCT_ID      NVARCHAR(50)        NOT NULL,
    LOT_ID          NVARCHAR(50)        NOT NULL,
    STEP_ID         NVARCHAR(50)        NOT NULL,
    EQP_ID          NVARCHAR(50)        NOT NULL,
    RECV_RECIPE_ID  NVARCHAR(50)        NOT NULL,                -- 설비가 보낸 RecipeID
    BASE_RECIPE_ID  NVARCHAR(50)        NOT NULL,                -- 기준정보의 RecipeID
    RESULT          NVARCHAR(10)        NOT NULL,                -- 종합 판정 OK / NG
    REASON          NVARCHAR(400)       NULL,
    EVENT_TIME      DATETIMEOFFSET(3)   NOT NULL,
    CREATED_AT      DATETIME2(3)        NOT NULL CONSTRAINT DF_TB_RMS_CHECK_CREATED_AT DEFAULT (SYSDATETIME()),
    CONSTRAINT PK_TB_RMS_CHECK PRIMARY KEY (CHECK_ID),
    CONSTRAINT CK_TB_RMS_CHECK_RESULT CHECK (RESULT IN (N'OK', N'NG'))
);
CREATE INDEX IX_TB_RMS_CHECK_LOT ON dbo.TB_RMS_CHECK (LOT_ID, EQP_ID, EVENT_TIME);

CREATE TABLE dbo.TB_RMS_CHECK_DTL
(
    DTL_SEQ     BIGINT         IDENTITY(1,1) NOT NULL,
    CHECK_ID    BIGINT         NOT NULL,
    ITEM_TYPE   NVARCHAR(10)   NOT NULL,      -- RECIPE(RecipeID 비교) / PARAM(파라미터 비교)
    ITEM_ID     NVARCHAR(50)   NOT NULL,      -- RecipeID 또는 ParameterID
    BASE_VALUE  NVARCHAR(100)  NULL,          -- 검증 당시 기준값
    RECV_VALUE  NVARCHAR(100)  NULL,          -- 수신값
    JUDGE       NVARCHAR(10)   NOT NULL,      -- OK / NG
    REASON      NVARCHAR(100)  NULL,          -- 예: SPEED_MISMATCH, PRESSURE_MISSING
    CONSTRAINT PK_TB_RMS_CHECK_DTL PRIMARY KEY (DTL_SEQ),
    CONSTRAINT FK_TB_RMS_CHECK_DTL_CHECK FOREIGN KEY (CHECK_ID) REFERENCES dbo.TB_RMS_CHECK (CHECK_ID) ON DELETE CASCADE,
    CONSTRAINT CK_TB_RMS_CHECK_DTL_JUDGE CHECK (JUDGE IN (N'OK', N'NG'))
);
GO

/* ---------------------------------------------------------------------------
   이력 — 반영된 이벤트 (조회 화면의 "이벤트·발생시간·처리결과")
   --------------------------------------------------------------------------- */
CREATE TABLE dbo.TB_LOT_EVENT
(
    EVENT_SEQ   BIGINT              IDENTITY(1,1) NOT NULL,
    MESSAGE_ID  NVARCHAR(50)        NOT NULL,
    EVENT_TYPE  NVARCHAR(20)        NOT NULL,     -- LOT_START / PROCESS_START / PROCESS_END / LOT_END / RMS_CHECK
    PRODUCT_ID  NVARCHAR(50)        NOT NULL,
    LOT_ID      NVARCHAR(50)        NOT NULL,
    STEP_ID     NVARCHAR(50)        NOT NULL,
    EQP_ID      NVARCHAR(50)        NOT NULL,
    RECIPE_ID   NVARCHAR(50)        NOT NULL,
    EVENT_TIME  DATETIMEOFFSET(3)   NOT NULL,     -- 설비 발생시간 (시간대 포함)
    RESULT      NVARCHAR(10)        NOT NULL,     -- OK / NG
    REASON      NVARCHAR(400)       NULL,
    RUN_ID      BIGINT              NULL,         -- PROCESS 이벤트가 연결된 구동
    CHECK_ID    BIGINT              NULL,         -- RMS_CHECK 이벤트의 검증
    CREATED_AT  DATETIME2(3)        NOT NULL CONSTRAINT DF_TB_LOT_EVENT_CREATED_AT DEFAULT (SYSDATETIME()),
    CONSTRAINT PK_TB_LOT_EVENT PRIMARY KEY (EVENT_SEQ)
);
CREATE INDEX IX_TB_LOT_EVENT_LOT ON dbo.TB_LOT_EVENT (LOT_ID, EVENT_TIME);
CREATE INDEX IX_TB_LOT_EVENT_EQP ON dbo.TB_LOT_EVENT (EQP_ID, EVENT_TIME);
GO

/* ---------------------------------------------------------------------------
   메시지
   --------------------------------------------------------------------------- */
/* 반영 완료한 MessageName — 업무 처리와 같은 트랜잭션에서 INSERT → 재수신 시 중복 반영 없음 */
CREATE TABLE dbo.TB_MSG_PROCESSED
(
    MESSAGE_ID    NVARCHAR(50)   NOT NULL,
    EVENT_TYPE    NVARCHAR(20)   NOT NULL,
    EQP_ID        NVARCHAR(50)   NOT NULL,
    RESULT        NVARCHAR(10)   NOT NULL,       -- 최초 처리 결과 OK / NG
    REASON        NVARCHAR(400)  NULL,
    PROCESSED_AT  DATETIME2(3)   NOT NULL CONSTRAINT DF_TB_MSG_PROCESSED_AT DEFAULT (SYSDATETIME()),
    CONSTRAINT PK_TB_MSG_PROCESSED PRIMARY KEY (MESSAGE_ID)
);

/* 송수신 원문 — 정상·오류 모두 그대로 보관 */
CREATE TABLE dbo.TB_MSG_LOG
(
    LOG_SEQ     BIGINT          IDENTITY(1,1) NOT NULL,
    DIRECTION   NVARCHAR(3)     NOT NULL,      -- IN(요청 수신) / OUT(응답 송신)
    TOPIC       NVARCHAR(100)   NOT NULL,
    PARTITION_NO INT            NULL,
    OFFSET_NO   BIGINT          NULL,
    MESSAGE_ID  NVARCHAR(50)    NULL,
    EVENT_TYPE  NVARCHAR(20)    NULL,
    EQP_ID      NVARCHAR(50)    NULL,
    LOT_ID      NVARCHAR(50)    NULL,
    RAW_XML     NVARCHAR(MAX)   NOT NULL,
    RESULT      NVARCHAR(10)    NULL,          -- OK / NG / ERROR (OUT 기준)
    REASON      NVARCHAR(400)   NULL,
    LOGGED_AT   DATETIME2(3)    NOT NULL CONSTRAINT DF_TB_MSG_LOG_LOGGED_AT DEFAULT (SYSDATETIME()),
    CONSTRAINT PK_TB_MSG_LOG PRIMARY KEY (LOG_SEQ)
);
CREATE INDEX IX_TB_MSG_LOG_MESSAGE ON dbo.TB_MSG_LOG (MESSAGE_ID, DIRECTION);
GO
