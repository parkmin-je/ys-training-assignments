# 설비 관리 + FDC & 멀티스레드 (WinForms)

과제: `와이에스시스템(주) 시스템 구축 연습.pptx` → `와이에스시스템(주) 과제 (FDC & 멀티스레드).pptx`
FDC PPT의 "Part 1 ➔ Part 2" 로드맵대로 구축 연습 결과물 위에 FDC 과제를 이어서 구현했다.

## 실행 방법

1. SSMS에서 `Schema_init.sql` 실행 — TrainingDB에 테이블 4개 생성 + 초기 데이터 (다시 실행해도 오류·중복 없음)
   - TB_EQUIPMENT(설비 3건: EQP_001~003), TB_EQUIP_LOG(로그 4건), TB_PARAM_LIMIT(설비별 TEMP 20~80, PRESS 1~5), TB_EQUIP_DATA
2. `fdcapp.sln` → 시작 프로젝트 `fdcapp` → F5
3. 검증 프로그램: `dotnet run --project fdcapp.Tests` (24/24 PASS)

DB 연결: `Data/AppDbContext.cs`의 `OnConfiguring` (localhost / TrainingDB / test 계정)

## 구조

```
Form1 (UI) ➜ EquipmentService (업무 로직) ➜ AppDbContext (EF Core) ➜ MSSQL
                    ▲
SensorSimulator ────┘ (Task.Run 백그라운드, 1.5초 주기)
```

| 폴더 | 내용 |
|---|---|
| `Models` | Equipment · EquipLog · ParamLimit · EquipData — 테이블 1개당 Entity 1개 |
| `Data` | AppDbContext — DbSet 4개, 접속 문자열 |
| `Dtos` | FdcResult(판정 결과) · SensorCycleResult(시뮬레이터 1주기 결과) |
| `Services` | IEquipmentService · EquipmentService · SensorSimulator |

## 요구사항 대응

| PPT | 구현 |
|---|---|
| 구축 1-1·1-2 테이블 설계, 초기 INSERT, JOIN 검증 | `Schema_init.sql` |
| 구축 2-1 Entity · DbContext | `Models`, `Data/AppDbContext.cs` |
| 구축 2-2 GetEquipmentList · AddEquipment(중복 체크) · GetLogsByEquipId(역순) · UpdateEquipmentStatus | `EquipmentService` |
| 구축 3 txtEquipId · txtEquipName · cboLine · btnAdd · dgvEquipment · cboStatus · btnChangeStatus · dgvLogs | `Form1` |
| 구축 4 선택 설비 로그 즉시 바인딩, 상태 변경 UPDATE + "상태 변경 [IDLE ➔ RUN]" 로그 INSERT | `dgvEquipment_SelectionChanged`, `UpdateEquipmentStatus` (SaveChanges 1회) |
| FDC 1-1 TB_PARAM_LIMIT | `Schema_init.sql`, `Models/ParamLimit.cs` |
| FDC 1-2 CheckParameterAlarm — 이탈 시 DOWN + ALARM 로그, [판정] 버튼 단위 테스트 | `EquipmentService`, `btnFdcCheck_Click` |
| FDC 2-1 TB_EQUIP_DATA — 수집과 동시에 FDC 판정해 IS_FAULT 결정 | `CollectSensorData` (온도·압력) |
| FDC 2-2 Task.Run · CancellationTokenSource · 1.5초 · 온도 20~95 · 압력 1~6 | `SensorSimulator`, `btnSimStart_Click` / `btnSimStop_Click` |
| FDC 2-3 InvokeRequired / BeginInvoke, DOWN 행 LightCoral | `OnSimCycle`, `ApplyEquipmentRowColors` |

UI 스레드와 수집 스레드가 같은 서비스를 쓰므로, `EquipmentService`는 lock으로 DbContext 동시 사용을 막는다.
