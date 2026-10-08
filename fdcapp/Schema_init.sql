/* ============================================================================
   TrainingDB - DDL 스크립트
   ----------------------------------------------------------------------------
   서버         : localhost
   데이터베이스 : TrainingDB
   계정         : test / 1234 (SQL 인증)
   ----------------------------------------------------------------------------
   생성 테이블
     1) dbo.TB_EQUIPMENT   : 설비 마스터              (시스템 구축 연습)
     2) dbo.TB_EQUIP_LOG   : 설비 로그                (시스템 구축 연습)
     3) dbo.TB_EQUIP_DATA  : 설비 센서 수집 데이터    (FDC & 멀티스레드 과제 2)
     4) dbo.TB_PARAM_LIMIT : 파라미터 임계치 규칙     (FDC & 멀티스레드 과제 1)
   ----------------------------------------------------------------------------
   규칙
     - 문자열 컬럼은 다국어·한글 깨짐 방지를 위해 모두 NVARCHAR(200)
     - 물리 FK는 걸지 않는다 (EQUIP_ID 연관 조회는 코드에서 처리)
     - 다시 실행해도 오류·중복 없이 끝나도록 IF 조건으로 감싼다
   ============================================================================ */

USE [TrainingDB];
GO

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO


/* ----------------------------------------------------------------------------
   (선택) 기존 테이블을 삭제하고 새로 만들려면 아래 주석을 해제하세요.
   ※ 실행하면 기존 데이터가 모두 삭제됩니다.
   ---------------------------------------------------------------------------- */
-- IF OBJECT_ID(N'dbo.TB_EQUIP_DATA', N'U') IS NOT NULL DROP TABLE dbo.TB_EQUIP_DATA;
-- GO
-- IF OBJECT_ID(N'dbo.TB_PARAM_LIMIT', N'U') IS NOT NULL DROP TABLE dbo.TB_PARAM_LIMIT;
-- GO
-- IF OBJECT_ID(N'dbo.TB_EQUIP_LOG', N'U') IS NOT NULL DROP TABLE dbo.TB_EQUIP_LOG;
-- GO
-- IF OBJECT_ID(N'dbo.TB_EQUIPMENT', N'U') IS NOT NULL DROP TABLE dbo.TB_EQUIPMENT;
-- GO


/* ============================================================================
   1. TB_EQUIPMENT : 설비 마스터
   ============================================================================ */
IF OBJECT_ID(N'dbo.TB_EQUIPMENT', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TB_EQUIPMENT
    (
        EQUIP_ID    NVARCHAR(200) NOT NULL,     -- 설비 고유 코드
        EQUIP_NAME  NVARCHAR(200) NOT NULL,     -- 설비명
        LINE_NAME   NVARCHAR(200) NOT NULL,     -- 배치 라인명
        STATUS      NVARCHAR(200) NULL          -- 설비 상태 (IDLE, RUN 등)
        CONSTRAINT DF_TB_EQUIPMENT_STATUS DEFAULT (N'IDLE'),
        CREATE_DT   DATETIME      NULL,         -- 생성 일시

        CONSTRAINT PK_TB_EQUIPMENT PRIMARY KEY CLUSTERED (EQUIP_ID)
    );
END
GO

/* ----------------------------------------------------------------------------
   (기존 테이블이 이미 있는 경우) CREATE_DT 컬럼 추가
   ---------------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.TB_EQUIPMENT', N'U') IS NOT NULL
   AND COL_LENGTH(N'dbo.TB_EQUIPMENT', N'CREATE_DT') IS NULL
BEGIN
    ALTER TABLE dbo.TB_EQUIPMENT ADD CREATE_DT DATETIME NULL;
END
GO


/* ============================================================================
   2. TB_EQUIP_LOG : 설비 로그
   ============================================================================ */
IF OBJECT_ID(N'dbo.TB_EQUIP_LOG', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TB_EQUIP_LOG
    (
        LOG_ID     BIGINT        IDENTITY(1,1) NOT NULL,    -- 로그 ID (자동 증가)
        EQUIP_ID   NVARCHAR(200) NULL,                      -- 설비 고유 코드
        LOG_TYPE   NVARCHAR(200) NULL,                      -- 로그 유형 (INFO, STATUS_CHG, ALARM 등)
        LOG_MSG    NVARCHAR(200) NULL,                      -- 로그 메시지
        OCCUR_DT   DATETIME      NULL                       -- 발생 일시
            CONSTRAINT DF_TB_EQUIP_LOG_OCCUR_DT DEFAULT (GETDATE()),

        CONSTRAINT PK_TB_EQUIP_LOG PRIMARY KEY CLUSTERED (LOG_ID)
    );
END
GO

/* ----------------------------------------------------------------------------
   (기존 테이블이 이미 있는 경우) 규칙에 맞게 보정
     - LOG_TYPE을 NVARCHAR(50) → NVARCHAR(200)으로 확장 (기존 데이터 유지)
     - OCCUR_DT를 넣지 않고 INSERT하면 현재 시각이 들어가도록 기본값 추가
   ---------------------------------------------------------------------------- */
IF COL_LENGTH(N'dbo.TB_EQUIP_LOG', N'LOG_TYPE') < 400      -- COL_LENGTH는 바이트 단위: NVARCHAR(200) = 400
BEGIN
    ALTER TABLE dbo.TB_EQUIP_LOG ALTER COLUMN LOG_TYPE NVARCHAR(200) NULL;
END
GO

IF OBJECT_ID(N'dbo.TB_EQUIP_LOG', N'U') IS NOT NULL
   AND NOT EXISTS (SELECT 1 FROM sys.default_constraints
                   WHERE parent_object_id = OBJECT_ID(N'dbo.TB_EQUIP_LOG')
                     AND COL_NAME(parent_object_id, parent_column_id) = N'OCCUR_DT')
BEGIN
    ALTER TABLE dbo.TB_EQUIP_LOG ADD CONSTRAINT DF_TB_EQUIP_LOG_OCCUR_DT DEFAULT (GETDATE()) FOR OCCUR_DT;
END
GO


/* ============================================================================
   3. TB_EQUIP_DATA : 설비 센서 수집 데이터
   ----------------------------------------------------------------------------
   ※ EQUIP_ID는 설비 식별 코드지만 외래키(FK)는 걸지 않습니다.
   ============================================================================ */
IF OBJECT_ID(N'dbo.TB_EQUIP_DATA', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TB_EQUIP_DATA
    (
        DATA_ID      BIGINT         IDENTITY(1,1) NOT NULL,   -- 수집 데이터 일련번호
        EQUIP_ID     NVARCHAR(200)  NOT NULL,                 -- 설비 식별 코드 (FK 없음)
        TEMP_VAL     FLOAT          NOT NULL,                 -- 온도 측정 센서값 (℃)
        PRESS_VAL    FLOAT          NOT NULL,                 -- 압력 측정 센서값 (bar)
        IS_FAULT     NVARCHAR(200)  NULL                      -- 판정 결과 (NORMAL / ALARM)
            CONSTRAINT DF_TB_EQUIP_DATA_IS_FAULT DEFAULT (N'NORMAL'),
        COLLECT_DT   DATETIME       NULL                      -- 데이터 수집 일시
            CONSTRAINT DF_TB_EQUIP_DATA_COLLECT_DT DEFAULT (GETDATE()),

        CONSTRAINT PK_TB_EQUIP_DATA PRIMARY KEY CLUSTERED (DATA_ID)
    );
END
GO


/* ============================================================================
   4. TB_PARAM_LIMIT : 파라미터 임계치 규칙
   ----------------------------------------------------------------------------
   ※ EQUIP_ID는 설비 식별 코드지만 외래키(FK)는 걸지 않습니다.
   ============================================================================ */
IF OBJECT_ID(N'dbo.TB_PARAM_LIMIT', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.TB_PARAM_LIMIT
    (
        LIMIT_ID     BIGINT         IDENTITY(1,1) NOT NULL,   -- 임계치 규칙 번호
        EQUIP_ID     NVARCHAR(200)  NOT NULL,                 -- 설비 식별 코드 (FK 없음)
        PARAM_NAME   NVARCHAR(200)  NOT NULL,                 -- 측정 파라미터 (TEMP, PRESS)
        LOWER_LIMIT  FLOAT          NOT NULL,                 -- 정상 하한값 (예: 20.0)
        UPPER_LIMIT  FLOAT          NOT NULL,                 -- 정상 상한값 (예: 80.0)
        CREATE_DT    DATETIME       NULL                      -- 룰 등록 일시
            CONSTRAINT DF_TB_PARAM_LIMIT_CREATE_DT DEFAULT (GETDATE()),

        CONSTRAINT PK_TB_PARAM_LIMIT PRIMARY KEY CLUSTERED (LIMIT_ID)
    );
END
GO


/* ============================================================================
   4-1. 파라미터 임계치 규칙 초기 데이터 등록 (설비별 TEMP/PRESS 상·하한)
   ----------------------------------------------------------------------------
   ※ 파라미터명은 과제 PPT의 INSERT 예시대로 TEMP / PRESS 를 사용 (코드와 반드시 일치)
   ※ LIMIT_ID가 IDENTITY라 PK 중복 오류가 나지 않으므로,
      같은 설비·파라미터 규칙이 없을 때만 넣어 재실행 시 중복을 막는다
   ============================================================================ */
INSERT INTO dbo.TB_PARAM_LIMIT (EQUIP_ID, PARAM_NAME, LOWER_LIMIT, UPPER_LIMIT)
SELECT V.EQUIP_ID, V.PARAM_NAME, V.LOWER_LIMIT, V.UPPER_LIMIT
FROM (VALUES
    (N'EQP_001', N'TEMP', 20.0, 80.0),
    (N'EQP_001', N'PRESS', 1.0, 5.0),
    (N'EQP_002', N'TEMP', 20.0, 80.0),
    (N'EQP_002', N'PRESS', 1.0, 5.0),
    (N'EQP_003', N'TEMP', 20.0, 80.0),
    (N'EQP_003', N'PRESS', 1.0, 5.0)
) AS V (EQUIP_ID, PARAM_NAME, LOWER_LIMIT, UPPER_LIMIT)
WHERE NOT EXISTS (SELECT 1 FROM dbo.TB_PARAM_LIMIT P
                  WHERE P.EQUIP_ID = V.EQUIP_ID AND P.PARAM_NAME = V.PARAM_NAME);
GO


/* ============================================================================
   5. 설비 기초 마스터 데이터 등록 (3건, 과제 PPT 예시)
   ----------------------------------------------------------------------------
   ※ 이미 등록된 설비 ID는 건너뛰어 재실행 시 PK 중복 오류를 막는다
   ============================================================================ */
INSERT INTO dbo.TB_EQUIPMENT (EQUIP_ID, EQUIP_NAME, LINE_NAME, STATUS, CREATE_DT)
SELECT V.EQUIP_ID, V.EQUIP_NAME, V.LINE_NAME, V.STATUS, GETDATE()
FROM (VALUES
    (N'EQP_001', N'설비 1호기', N'LINE_A', N'IDLE'),
    (N'EQP_002', N'설비 2호기', N'LINE_A', N'RUN'),
    (N'EQP_003', N'설비 3호기', N'LINE_B', N'DOWN')
) AS V (EQUIP_ID, EQUIP_NAME, LINE_NAME, STATUS)
WHERE NOT EXISTS (SELECT 1 FROM dbo.TB_EQUIPMENT E WHERE E.EQUIP_ID = V.EQUIP_ID);
GO


/* ============================================================================
   6. 초기 가동 및 알람 로그 등록 (샘플 4건)
   ----------------------------------------------------------------------------
   ※ 샘플 로그가 하나도 없을 때만 등록한다 (재실행 시 중복 방지)
   ============================================================================ */
IF NOT EXISTS (SELECT 1 FROM dbo.TB_EQUIP_LOG WHERE OCCUR_DT = '2026-09-13 09:00:00' AND EQUIP_ID = N'EQP_001')
BEGIN
    -- 설비 등록 완료 (INFO)
    INSERT INTO dbo.TB_EQUIP_LOG (EQUIP_ID, LOG_TYPE, LOG_MSG, OCCUR_DT)
    VALUES (N'EQP_001', N'INFO', N'설비 등록 완료', '2026-09-13 09:00:00');

    -- 가동 시작 (STATUS_CHG)
    INSERT INTO dbo.TB_EQUIP_LOG (EQUIP_ID, LOG_TYPE, LOG_MSG, OCCUR_DT)
    VALUES (N'EQP_001', N'STATUS_CHG', N'가동 시작 (IDLE -> RUN)', '2026-09-13 09:05:00');

    -- 모터 과열 (ALARM)
    INSERT INTO dbo.TB_EQUIP_LOG (EQUIP_ID, LOG_TYPE, LOG_MSG, OCCUR_DT)
    VALUES (N'EQP_002', N'ALARM', N'모터 과열 경보', '2026-09-13 09:10:00');

    -- 설비 등록 완료 (INFO)
    INSERT INTO dbo.TB_EQUIP_LOG (EQUIP_ID, LOG_TYPE, LOG_MSG, OCCUR_DT)
    VALUES (N'EQP_003', N'INFO', N'설비 등록 완료', '2026-09-13 09:15:00');
END
GO


/* ============================================================================
   7. 조회 및 정합성 검증
   ============================================================================ */

-- 7-1. 전체 설비 조회
SELECT EQUIP_ID, EQUIP_NAME, LINE_NAME, STATUS, CREATE_DT
FROM dbo.TB_EQUIPMENT
ORDER BY EQUIP_ID;
GO

-- 7-2. 전체 로그 조회
SELECT LOG_ID, EQUIP_ID, LOG_TYPE, LOG_MSG, OCCUR_DT
FROM dbo.TB_EQUIP_LOG
ORDER BY LOG_ID;
GO

-- 7-3. 설비–로그 JOIN 조회 (EQUIP_ID 기준, 설비별 최신 로그 먼저)
SELECT E.EQUIP_ID, E.EQUIP_NAME, E.LINE_NAME, E.STATUS,
       L.LOG_ID, L.LOG_TYPE, L.LOG_MSG, L.OCCUR_DT
FROM dbo.TB_EQUIPMENT E
INNER JOIN dbo.TB_EQUIP_LOG L ON L.EQUIP_ID = E.EQUIP_ID
ORDER BY E.EQUIP_ID, L.OCCUR_DT DESC;
GO

-- 7-4. 임계치 규칙 조회
SELECT LIMIT_ID, EQUIP_ID, PARAM_NAME, LOWER_LIMIT, UPPER_LIMIT, CREATE_DT
FROM dbo.TB_PARAM_LIMIT
ORDER BY EQUIP_ID, PARAM_NAME;
GO
