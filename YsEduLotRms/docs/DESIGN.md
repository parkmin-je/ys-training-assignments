# 설계 설명

2차 과제 p.13 "설계 설명" 항목: 테이블 관계와 Entity 구성, 프로그램 역할 분리, 오류 및 중복 처리 방식, 선택한 구조의 이유.

## 1. 테이블 관계

```
[기준정보]
TB_PRODUCT ─┐
TB_STEP ────┼─< TB_RECIPE (PRODUCT_ID, STEP_ID, EQP_ID UNIQUE) ─< TB_RECIPE_PARAM (RECIPE_ID, PARAM_ID)
TB_EQUIPMENT┘

[현재 상태]                     [이력]
TB_LOT  (LOT 작업 상태)          TB_EQP_RUN      설비별 구동 1회 = 1행 (START + END)
TB_EQUIPMENT.EQP_STATUS         TB_RMS_CHECK    RMS 종합 판정 1회 = 1행 ─< TB_RMS_CHECK_DTL 항목별 상세
 (설비 구동 상태)                TB_LOT_EVENT    반영된 이벤트 (이벤트·발생시간·처리결과)

[메시지]
TB_MSG_PROCESSED  반영된 MessageName (PK) — 중복 반영 방지
TB_MSG_LOG        수신·응답 원문과 결과 — ERROR 포함 전체
```

| 구분 | 테이블 | 키 | 설계 이유 |
|---|---|---|---|
| 기준정보 | TB_RECIPE | RECIPE_ID, UQ(PRODUCT_ID, STEP_ID, EQP_ID) | p.8 "ProductID · STEPID · EQPID로 기준 Recipe 조회" — 조건 1개에 기준 Recipe 1개 |
| 기준정보 | TB_RECIPE_PARAM | (RECIPE_ID, PARAM_ID) | 같은 Recipe 안에서 ParameterID 중복 불가, Recipe 삭제 시 CASCADE |
| 상태 | TB_LOT | LOT_ID | LOT 작업 상태(RUN/DONE)를 설비 구동 상태와 분리 (p.10) |
| 상태 | TB_EQUIPMENT.EQP_STATUS | EQP_ID | 설비 구동 상태(IDLE/RUN) |
| 이력 | TB_EQP_RUN | RUN_ID (IDENTITY) | 같은 LOT·설비의 반복 구동을 구분하는 식별키 (p.9). 열린 구동은 필터 유니크 인덱스로 1건만 |
| 이력 | TB_RMS_CHECK / DTL | CHECK_ID (IDENTITY) | 재검증마다 새 행 — NG 후 OK를 각각 조회 (p.12). 검증 당시 기준값·수신값·판정·사유를 그대로 남김 |
| 메시지 | TB_MSG_PROCESSED | MESSAGE_ID | MessageName 중복 반영 방지 (p.11) |
| 메시지 | TB_MSG_LOG | LOG_SEQ | 실패 사유 확인 (p.11). ERROR도 원문과 사유를 남김 |

시간 컬럼은 `DATETIMEOFFSET(3)` — EventTime이 "시간대 포함 발생시간"(p.5)이므로 시간대를 잃지 않게 저장한다.

## 2. Entity 구성

`src/YsEdu.Core/Data/Entities`에 테이블 1개당 클래스 1개 (p.9 "각 테이블을 별도 .cs Entity 클래스로").
`YsEduDbContext`가 테이블 매핑·키·관계를 정의하고, `DbFactory`가 작업 1건마다 DbContext를 새로 만든다.

## 3. 프로그램 역할 분리

```
Simulator(설비/CIM) ── YSEDU.LOT.REQUEST ──▶ Middleware ──▶ MSSQL
        ▲                                       │
        └────────── YSEDU.LOT.RESPONSE ◀────────┘          Viewer ──▶ MSSQL (조회만)
```

| 계층 | 클래스 | 하는 일 |
|---|---|---|
| 수신 루프 | `MiddlewareWorker` | Kafka 수신·응답·Commit, 별도 Task에서 실행 |
| 메시지 처리 | `MessageProcessor` | XML 파싱·검증 → 중복 확인 → 이벤트 처리 → 상태·이력·처리 기록을 한 트랜잭션으로 저장 |
| 업무 규칙 | `LotEventHandler`, `RmsValidator` | LOT/PROCESS 이벤트 규칙, RMS 비교·판정 |
| 조회 | `HistoryQueryService` | Viewer용 조회 |
| 화면 | 각 `MainForm` | 표시만 담당. 백그라운드 결과는 `BeginInvoke`로 UI 스레드에서 갱신 |

## 4. 오류 및 중복 처리

| 상황 | 처리 | 근거 |
|---|---|---|
| 같은 MessageName 재수신 | TB_MSG_PROCESSED에 있으면 다시 반영하지 않고 처음 결과로 응답 (DUPLICATE_MESSAGE) | p.11 |
| 새 검증 · 새 구동 | 새 MessageName → 새 CHECK_ID / RUN_ID 행 | p.9 |
| START 없는 END | ERROR / NO_PROCESS_START, 이력 미반영 | p.11 |
| B의 LOT_START 없음 | PROCESS 처리의 필수 조건으로 쓰지 않음 | p.4 |
| RMS 판정 | Double 허용오차 없이 비교. 전체 일치 OK, ID·값 불일치·누락·추가·중복 NG, 숫자 변환·미등록·DB 실패 ERROR | p.8 |
| 구동 전 검증 | 시뮬레이터는 RMS_CHECK가 OK일 때만 PROCESS_START 송신, NG/ERROR면 보류 | p.8 |
| 상태 + 이력 저장 | SaveChanges 1회 = 트랜잭션 1개. 실패하면 전부 취소하고 ERROR / DB_ERROR로 응답 (OK로 응답하지 않음) | p.11 |
| DB에 기록할 수 없는 오류 | 파일 로그(`logs/`)에 원문과 사유 기록 | p.11 |
| 처리·Commit 오류 | 수신 루프를 멈추지 않음. 처리 오류는 같은 위치 재처리, 재처리 시 중복 확인으로 한 번만 반영 | p.12 "이후 수신 지속" |
| 동일 DbContext 동시 사용 | DbContext를 메시지 1건·조회 1회마다 생성·폐기 | p.11 |
| 중지·종료 | CancellationToken으로 루프 종료 후 Consumer Close, "수신 작업 종료 확인" 기록 | p.11 |

### EventTime 형식

PPT 안에서 두 형식이 함께 나온다.

- p.5·6: `2026-10-05T09:04:00+09:00` (시간대 포함) → 그대로 저장
- p.7: `2026100514000000` (yyyyMMddHHmmss + 1/100초) → 시간대가 없으므로 로컬(KST, +09:00)로 저장

ISO 형식인데 시간대만 빠진 값(`2026-10-05T14:00:00`)은 p.5 "시간대 포함" 위반으로 ERROR / INVALID_EVENT_TIME.

### 메시지 식별 태그

p.6·7 예제는 `<MessageName>`, p.5 표는 `MessageID`로 표기한다. 둘 다 받아서 같은 식별값으로 쓴다 (`<MessageName>` 우선).

## 5. 구현 범위

- 필수: 이벤트 5종 처리, RMS 종합·상세 저장, Kafka 연계 미들웨어, 시뮬레이터, 조회 화면, p.12 시나리오 6종
- 기준정보 CRUD 화면은 만들지 않음 — p.2 "기준정보는 초기 SQL로 준비해도 된다"
