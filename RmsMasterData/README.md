# RMS 기준정보 관리 (WinForm CRUD)

과제: `와이에스시스템(주) 기준정보관리 생성.pptx` — 설비·레시피·파라미터 기준정보를 관리하는 WinForm 프로그램
순서: **① 기준정보관리 생성** → ② Kafka RMS Middleware → ③ RMS 결과 조회 (각각 별도 솔루션)

## 실행 방법

1. SQL Server(localhost)에서 `Database` 폴더 스크립트를 순서대로 실행
   - `01_create_database.sql` — YsRmsDB 생성
   - `02_create_tables.sql` — **제공된 CREATE TABLE 원문 그대로** (TB_EQUIPMENT, TB_RMS_RECIPE, TB_RMS_RECIPE_PARAMETER)
   - `03_sample_data.sql` — 시연용 샘플 (선택, Kafka 과제 요청 예제 EQP_001 / MODEL / A100 → RECIPE_001에 맞춤)
2. `RmsMasterData.sln`을 Visual Studio 2022로 열고 시작 프로젝트 `RmsMasterData` → F5
3. 검증 프로그램: 시작 프로젝트를 `RmsMasterData.Tests`로 바꿔 실행 (또는 `dotnet run --project RmsMasterData.Tests`)

DB 연결 설정: `RmsMasterData/appsettings.json` (`ConnectionStrings:YsRmsDB`, Windows 인증)

## EF Core Database First

```
dotnet ef dbcontext scaffold "Server=localhost;Database=YsRmsDB;Integrated Security=True;TrustServerCertificate=True;" ^
  Microsoft.EntityFrameworkCore.SqlServer --context RmsDbContext --context-dir Data --output-dir Data/Entities ^
  --namespace RmsMasterData.Data.Entities --context-namespace RmsMasterData.Data --no-onconfiguring
```
(Visual Studio 패키지 관리자 콘솔에서는 `Scaffold-DbContext`로 같은 결과)

생성 결과: `Data/RmsDbContext.cs`, `Data/Entities/TbEquipment.cs`, `TbRmsRecipe.cs`, `TbRmsRecipeParameter.cs`

## 구조

```
RmsMasterData/
├─ Program.cs / MainForm          탭 2개 (필수 화면)
├─ Controls/EquipmentControl      설비 기준정보 화면 — 조회·신규·저장·삭제
├─ Controls/RecipeControl         레시피 Master–Detail 화면 — 레시피 + 선택한 레시피의 파라미터
├─ Services/EquipmentService      설비 CRUD · 중복 검사 · 사용 중 설비 삭제 처리
├─ Services/RecipeService         레시피 CRUD · 조합 중복 · 파라미터 중복 · 마스터+디테일 한 번에 저장
├─ Common/AppConfig               appsettings.json · DbContext 생성 (작업마다 새로)
└─ Data/                          Scaffold로 생성한 Entity · DbContext
RmsMasterData.Tests/              중복 입력 · 삭제 · 저장 실패 처리 검증 (실제 DB)
```

화면(Controls)은 입력·표시만 하고 DB는 서비스 클래스를 통해서만 다룹니다.

## 필수 구현 / 필수 결과 대응 (p.6)

| PPT 요구 | 구현 |
|---|---|
| 설비 목록 조회·신규 등록·수정·삭제 | 설비 화면 [조회]·[신규]·[저장]·[삭제] |
| 레시피와 파라미터를 한 화면에서 관리 | 레시피 화면 위: 레시피 목록, 아래: 레시피 정보 + 선택한 레시피의 파라미터 |
| 마스터와 디테일을 하나의 작업으로 저장 | `RecipeService.CreateAsync/UpdateAsync` — 마스터·디테일 변경 후 `SaveChangesAsync` 1회 (EF Core가 한 트랜잭션으로 실행) |
| Entity와 DbContext를 이용해 DB 반영 | 모든 등록·수정·삭제를 Entity + DbContext로 처리 |
| 설비 ID와 레시피 ID 중복 방지 | 저장 전 존재 확인 → 안내 메시지 (PK 위반도 메시지로 변환) |
| 설비 ID·팩터 ID·팩터 값 조합 중복 방지 | 저장 전 같은 조합 확인 (자기 자신 제외) + DB UNIQUE 제약 |
| 동일 레시피의 파라미터 ID 중복 방지 | 그리드 입력값에서 중복 ID 검사 → 거부 (마스터도 저장 안 함) |
| 사용 중인 설비와 연결 데이터 삭제 처리 | 레시피가 연결된 설비는 삭제하지 않고 건수와 함께 안내 (레시피 삭제 또는 사용 여부 N 권장). 레시피 삭제 시 파라미터 함께 삭제 (ON DELETE CASCADE) |

## 검증 결과 (`RmsMasterData.Tests` 실행 결과, 17/17 PASS)

- 설비: 신규 등록 / ID 중복 거부 / 필수값 누락 거부 / 수정 시 수정 일시 기록
- 레시피: 마스터 + 파라미터 3건 동시 등록 / 레시피 ID 중복 거부 / 조합 중복 거부 / 파라미터 ID 중복 거부(마스터도 저장 안 됨) / 미등록 설비 거부 / 수정 시 값 변경·삭제·추가 동시 반영
- 저장 실패: 디테일 1건이 DB에서 실패하면 마스터와 나머지 디테일도 저장되지 않음
- 삭제: 레시피가 연결된 설비 삭제 거부 → 레시피 삭제 시 파라미터 함께 삭제 → 그 후 설비 삭제 가능
