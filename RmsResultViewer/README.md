# RMS 결과 조회 (WinForm Master–Detail)

과제: `와이에스시스템(주) 결과조회 생성.pptx`
순서: ① 기준정보관리 생성 → ② Kafka RMS Middleware → **③ RMS 결과 조회** (각각 별도 솔루션)
선행: ② 과제가 저장한 TB_RMS_RESULT / TB_RMS_RESULT_PARAMETER (YsRmsDB)

## 실행 방법

1. (선택) `Database/01_index_review.sql` — EVENT_TIME · EQUIP_ID 조회 인덱스 추가 (p.8 인덱스 검토)
2. `RmsResultViewer.sln` → F5 → 이벤트 기간 확인 → [조회] → 결과 행 선택 → 아래에 파라미터
3. 검증 프로그램: `dotnet run --project RmsResultViewer.Tests` — 기간(밀리초 경계 포함)·설비·레시피·복수 조건, 최신순 정렬, Detail 연결 (10/10 PASS)

설정 위치: `RmsResultViewer/appsettings.json` (ConnectionStrings:YsRmsDB)

## EF Core 모델 생성 (p.7 "결과 테이블에서 Entity와 DbContext를 Scaffold")

```
dotnet ef dbcontext scaffold "Server=localhost;Database=YsRmsDB;Integrated Security=True;TrustServerCertificate=True;" ^
  Microsoft.EntityFrameworkCore.SqlServer --table TB_RMS_RESULT --table TB_RMS_RESULT_PARAMETER ^
  --context RmsResultDbContext --context-dir Data --output-dir Data/Entities --no-onconfiguring
```

## 소스 읽는 순서 (학습용)

1. `Data/` — Scaffold로 만든 TbRmsResult(Master) · TbRmsResultParameter(Detail) · RmsResultDbContext
2. `Services/ResultQueryService.cs` — 기간(필수)·설비·레시피 조건 → Master 조회(최신 순), RESULT_ID → Detail 조회
3. `MainForm.cs` — 조회 버튼 잠금(중복 실행 방지), 재조회 시 Grid 정리, 결과 없음 안내, DB 오류 표시, Master 선택 → Detail 자동 조회

## 요구사항 대응

| PPT | 구현 |
|---|---|
| p.6 이벤트 시작·종료 시간으로 조회 / 설비 ID·레시피 ID / 복수 조건 / 초기화 | 조건 패널 + [초기화] |
| p.6 Master 선택 시 Detail 자동 조회 | `dgvMaster_SelectionChanged` |
| p.6 결과 없으면 빈 Grid와 안내 / 재조회 시 Grid 정리 / 중복 실행 방지 | `btnSearch_Click` |
| p.7 Scaffold · Master 조회 · Detail 조회 · Grid 표시 · 최신 EVENT_TIME 먼저 | `ResultQueryService` |
| p.8 기간 필수, 기간 밖 조회 안 함, 조회 중 버튼 잠금 후 복구, DB 실패 표시, 인덱스 검토 | 기간 조건 항상 적용 · `01_index_review.sql` |
