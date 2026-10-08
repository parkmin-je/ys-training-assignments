# 신입 교육 과제 모음

C# WinForms + EF Core + MSSQL(+ Kafka) 기반 제조 시스템 교육 과제 5종입니다.
과제 PPT 6종을 폴더 5개로 제출합니다. `시스템 구축 연습`과 `FDC & 멀티스레드`는 FDC PPT의 "Part 1 ➔ Part 2" 로드맵대로 같은 프로젝트에 이어서 구현했습니다.
각 폴더는 독립된 솔루션(.sln)이며, 폴더 안 `Database` 스크립트 → 솔루션 실행 순서로 동작합니다.

| # | 폴더 | 과제 | 주요 기술 |
|---|---|---|---|
| 1 | [fdcapp](fdcapp) | 시스템 구축 연습 + FDC & 멀티스레드 — 설비 마스터-디테일, 파라미터 임계치 판정, 센서 수집 시뮬레이터 | WinForms, EF Core, Task/CancellationToken |
| 2 | [RmsMasterData](RmsMasterData) | RMS 기준정보 관리 — 설비·레시피·파라미터 CRUD | WinForms, EF Core |
| 3 | [RmsKafkaMiddleware](RmsKafkaMiddleware) | Kafka 기반 RMS Middleware — XML 요청 검증·응답·결과 저장 | Kafka, XML, EF Core |
| 4 | [RmsResultViewer](RmsResultViewer) | RMS 결과 조회 — Master–Detail 화면 | WinForms, EF Core |
| 5 | [YsEduLotRms](YsEduLotRms) | 2차 시스템 구축 — 생산 이력 추적 및 RMS Recipe 검증 | Kafka, 미들웨어, 시뮬레이터 |

## 공통 환경

- .NET 9 SDK, Visual Studio 2022
- SQL Server (localhost)
- Kafka (localhost:9092) — 3, 5번 과제

DB 연결 문자열은 각 프로젝트의 `appsettings.json` 또는 `DbContext`에 있으며, 로컬 개발용 설정입니다.
2~4번은 같은 DB(YsRmsDB)를 공유하므로 2 → 3 → 4 순서로 실행합니다.
