# 2차 시스템 구축 — 생산 이력 추적 및 RMS Recipe 검증

과제: `와이에스시스템_2차 교육_시스템구축_과제.pptx`
A · B · C 인라인의 LOT 작업 이력·설비별 구동 이력을 저장하고, 설비 구동 전 RMS_CHECK로 Recipe와 Parameter를 기준정보와 비교해 종합·상세 판정을 저장·조회한다.

## 구성

| 프로젝트 | 역할 |
|---|---|
| `src/YsEdu.Core` | 공통 업무 처리 — Entity·DbContext, XML 메시지, 이벤트 처리, RMS 검증, Kafka 미들웨어 루프, 시뮬레이터 |
| `src/YsEdu.Middleware` | 미들웨어 화면 — `YSEDU.LOT.REQUEST` 수신 → 처리 → `YSEDU.LOT.RESPONSE` 응답 |
| `src/YsEdu.Simulator` | 설비 / CIM 시뮬레이터 — 완료 확인 시나리오 6종 버튼, XML 직접 편집·송신·재송신 |
| `src/YsEdu.Viewer` | 조회 화면 — LOT 작업 상태, 설비 구동 상태, 이벤트·구동 이력, RMS 종합·상세 판정 |
| `tests/YsEdu.ScenarioTest` | p.12 완료 확인 시나리오 자동 검증 (실제 Kafka + DB) |

UI(WinForms)는 화면 표시만 하고, 업무 처리는 모두 `YsEdu.Core`에 있다.

## 실행 방법

1. **DB** — SSMS에서 `Database` 폴더 스크립트를 순서대로 실행 (Windows 인증, localhost)
   - `01_create_database.sql` — YsEduLotDB 생성
   - `02_tables.sql` — 기준정보·생산 이력·RMS 이력·메시지 테이블
   - `03_seed.sql` — PRODUCT_A / STEP_020 / EQP_A·B·C / RCP_A01·B01·C01, 각 Recipe TEMP=120 · PRESSURE=30 · SPEED=450
2. **Kafka** — localhost:9092 실행 후 Topic 생성
   - `YSEDU.LOT.REQUEST`, `YSEDU.LOT.RESPONSE` (partitions 3)
3. **실행** — `YsEduLotRms.sln`을 Visual Studio 2022로 열고 시작 프로젝트를 여러 개로 지정
   - `YsEdu.Middleware` → [수신 시작]
   - `YsEdu.Simulator` → 시나리오 버튼 또는 XML 직접 송신
   - `YsEdu.Viewer` → LOTID·설비·기간으로 조회
4. **자동 검증** — `dotnet run --project tests/YsEdu.ScenarioTest`
   - Samples XML 형식 확인 + p.12 시나리오 6종 (정상 인라인, 중간 설비, RMS 불일치·재검증, 구성 오류, 순서·중복, 처리 오류)

## 설정 위치

| 항목 | 위치 |
|---|---|
| DB 연결 | `config/appsettings.json` → `ConnectionStrings:YsEduLotDB` |
| Kafka 서버 · Topic · Consumer Group · 응답 대기 시간 | `config/appsettings.json` → `Kafka` |
| 파일 로그 폴더 | `config/appsettings.json` → `Logging:FileDirectory` |

`config/appsettings.json` 하나를 각 프로젝트가 빌드 출력 폴더로 복사해서 사용한다 (코드에 연결 정보를 쓰지 않음).

## XML 예제 (`Samples`)

| 파일 | 내용 | 기대 결과 |
|---|---|---|
| `01`~`04` | A: LOT_START → RMS_CHECK → PROCESS_START → PROCESS_END | OK |
| `05` | B: RMS_CHECK SPEED=500 (p.6 예제) | NG / SPEED_MISMATCH |
| `06` | B: 값 수정 후 새 MessageName으로 재검증 | OK |
| `07` | B: PROCESS_START — p.7 예제 그대로 (숫자형 EventTime `2026100514000000`) | OK |
| `08`~`12` | B 구동 종료, C 검증·구동, C에서 LOT_END | OK |
| `13` | `<MessageID>` 태그로 보낸 요청 (p.5 표 이름) | OK |
| `21`~`24` | 파라미터 누락 / 추가 / 중복, RecipeID 불일치 | NG + 상세 사유 |
| `25` | 숫자 변환 오류 | ERROR |
| `26` | 같은 설비의 START 없는 END | ERROR / NO_PROCESS_START |
| `27` | 미등록 설비 | ERROR / UNREGISTERED_EQP |
| `28` | XML 형식 오류 | ERROR / XML_FORMAT_ERROR |
| `29` | 필수 노드(STEPID) 누락 | ERROR / MISSING_STEPID |
| `90` | 응답 예제 (`ProductionEventResponse`) | — |

시뮬레이터의 [직접 송신] 영역에 붙여 넣어 송신할 수 있다. 같은 파일을 두 번 보내면 중복 처리(DUPLICATE_MESSAGE)를 확인할 수 있다.

설계 설명(테이블 관계, Entity 구성, 역할 분리, 오류·중복 처리)은 [`docs/DESIGN.md`](docs/DESIGN.md)에 있다.
