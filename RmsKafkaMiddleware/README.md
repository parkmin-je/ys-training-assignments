# Kafka 기반 RMS Middleware (RMS_TEST)

과제: `와이에스시스템(주) Kafka_XML_Middleware.pptx`
순서: ① 기준정보관리 생성 → **② Kafka RMS Middleware** → ③ RMS 결과 조회 (각각 별도 솔루션)
선행: ① 과제의 YsRmsDB · 기준정보(TB_EQUIPMENT · TB_RMS_RECIPE · 파라미터)

## 실행 방법

1. Kafka 실행: `C:\kafka\start-kafka.bat` (localhost:9092) → Topic 생성: `Tools\create-topics.bat` (RMS.REQUEST / RMS.RESPONSE)
2. DB: `Database\01_create_result_tables.sql`(제공 DDL 원문) → `02_create_request_log.sql`(직접 설계) → `03_test_data.sql`(파라미터 없는 레시피, 오류 시험용)
3. `RmsKafkaMiddleware.sln` → `RmsKafkaMiddleware` 실행 → [수신 시작]
4. `RmsTestClient` 실행 → 예제 선택 → [RMS.REQUEST로 송신] → 오른쪽에 응답 XML
5. 자동 검증: `dotnet run --project RmsKafkaMiddleware.Tests` (정상·오류 5종·중복·DB 저장, 11/11 PASS)

설정 위치: `RmsKafkaMiddleware/appsettings.json` — DB 연결, Kafka 서버, Topic, Consumer Group, 지원 Rule(RMS_TEST)

## 소스 읽는 순서 (학습용)

| 순서 | 파일 | 볼 것 |
|---|---|---|
| 1 | `Messaging/RmsXml.cs` | 요청 XML 파싱(형식 오류·필수 노드 누락 구분), 응답 XML 생성, 결과 코드, 중복 식별 키(SHA-256) |
| 2 | `Services/RmsRequestProcessor.cs` | 처리 흐름 전체: 파싱 → Rule 확인 → 중복 확인 → 레시피 조회 → 파라미터 조회 → Master+Detail 저장(트랜잭션) → 응답 |
| 3 | `Services/KafkaWorker.cs` | Consumer 수신 루프, Producer 응답, 처리 후 Commit, 응답 실패 시 Seek로 재처리 |
| 4 | `MainForm.cs` | 백그라운드 수신 결과를 BeginInvoke로 화면에 표시, 종료 시 수신 작업 종료 확인 |
| 5 | `Database/*.sql` | 제공 DDL + 처리 이력 테이블(필터 유니크 인덱스로 같은 요청 SUCCESS 1건만) |
| 6 | `RmsTestClient/MainForm.cs` | 요청 쪽 Producer와 응답 Consumer (미들웨어를 참조하지 않음) |

## 요구사항 대응

| PPT | 구현 |
|---|---|
| p.4 처리 흐름 01~05 | KafkaWorker(01 수신, 04 전송) + RmsRequestProcessor(02 파싱, 03 조회, 05 저장) |
| p.5 요청 Topic RMS.REQUEST / Consumer로 수신 | `KafkaWorker` Subscribe(RequestTopic) |
| p.5 RULE_NAME이 RMS_TEST인지 확인 | 다르면 `RULE_NOT_SUPPORTED` |
| p.5 동일 메시지 중복 처리 방지 | 요청 값 전체의 SHA-256으로 식별 → 이미 SUCCESS면 저장 없이 `DUPLICATE` 응답(처음 결과 포함) |
| p.5 응답 Topic RMS.RESPONSE / Producer로 전송 | `ProduceAsync(ResponseTopic)`, Key = EQP_ID |
| p.5 정상·오류 결과 코드 포함 | HEADER/RESULT_CODE, RESULT_MESSAGE |
| p.5 Topic·연결정보는 설정파일 | appsettings.json |
| p.7 EQP_ID·FACTOR_ID·VALUE → RECIPE_ID → 파라미터 | 사용 중(Y)인 설비·레시피만 조회 |
| p.10 요청 1건당 Master 1건, RESULT_ID로 Detail 연결, 응답에 포함한 파라미터 저장 | TB_RMS_RESULT + TB_RMS_RESULT_PARAMETER 한 트랜잭션 |
| p.11 필수 오류 6종 | XML_FORMAT_ERROR / MISSING_NODE(누락 항목 표시) / RULE_NOT_SUPPORTED / RECIPE_NOT_FOUND / PARAMETER_NOT_FOUND / DB_ERROR — 모두 TB_RMS_REQUEST_LOG에 기록, DB에도 못 남기면 파일 로그 |

## 설계 선택 (설명용)

- **저장 후 응답**: PPT 흐름은 "04 응답 → 05 저장"이지만, 저장에 실패했는데 SUCCESS를 보내면 응답과 DB가 어긋납니다. 그래서 저장까지 성공해야 SUCCESS를 보내고, 실패하면 DB_ERROR로 응답합니다.
- **Commit 시점**: 처리·응답이 끝난 뒤 수동 Commit. 응답 전송이 실패하면 Commit하지 않고 같은 위치를 다시 처리하며, 이때 중복 확인 덕분에 저장은 1번만 됩니다.
- **처리 이력 테이블 추가**: 요청 XML에 메시지 ID가 없어 중복 판정과 오류 기록을 위해 직접 설계했습니다.
