/* ============================================================================
   Kafka 기반 RMS Middleware — 02. 요청 처리 이력 (직접 설계)
   ----------------------------------------------------------------------------
   제공 테이블에 없는 두 가지를 위해 추가한다.
     1) 동일 메시지의 중복 처리 방지 (Kafka p.5)
        요청 XML에는 메시지 ID가 없으므로 RULE_NAME·EVENT_TIME·EQP_ID·FACTOR_ID·FACTOR_VALUE를
        이어 붙인 값의 SHA-256(MESSAGE_KEY)으로 같은 요청을 식별한다.
        정상 처리(SUCCESS)된 키는 한 번만 들어갈 수 있다 (필터 유니크 인덱스).
     2) 오류 결과 기록 (Kafka p.11: "오류 결과를 기록하고 메시지 처리 상태 유지")
        정상·오류 모두 요청·응답 원문과 결과 코드를 남긴다.
   ============================================================================ */
USE YsRmsDB;
GO
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;
GO

CREATE TABLE dbo.TB_RMS_REQUEST_LOG (
  LOG_ID          BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
  MESSAGE_KEY     NVARCHAR(64)  NULL,                 -- 파싱 실패 시 NULL
  KAFKA_TOPIC     NVARCHAR(100) NOT NULL,
  KAFKA_PARTITION INT           NOT NULL,
  KAFKA_OFFSET    BIGINT        NOT NULL,
  EQUIP_ID        NVARCHAR(50)  NULL,
  RESULT_CODE     NVARCHAR(30)  NOT NULL,             -- SUCCESS / DUPLICATE / XML_FORMAT_ERROR ...
  RESULT_MESSAGE  NVARCHAR(500) NULL,
  RESULT_ID       BIGINT        NULL,                 -- 저장한 TB_RMS_RESULT (SUCCESS·DUPLICATE)
  REQUEST_XML     NVARCHAR(MAX) NOT NULL,
  RESPONSE_XML    NVARCHAR(MAX) NULL,
  RECEIVED_AT     DATETIME2(3)  NOT NULL DEFAULT SYSDATETIME()
);
CREATE UNIQUE INDEX UX_RMS_REQUEST_LOG_SUCCESS ON dbo.TB_RMS_REQUEST_LOG (MESSAGE_KEY) WHERE RESULT_CODE = N'SUCCESS';
GO
