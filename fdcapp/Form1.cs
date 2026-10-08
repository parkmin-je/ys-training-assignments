using fdcapp.Data;
using fdcapp.Dtos;
using fdcapp.Models;
using fdcapp.Services;

namespace fdcapp
{
    public partial class Form1 : Form
    {
        // 설비 데이터 처리를 담당하는 서비스 (DB 연결 포함)
        private readonly EquipmentService _service;

        // 가상 센서 시뮬레이터 엔진
        private readonly SensorSimulator _simulator;

        // 시뮬레이터 취소 토큰 소스 (시작 시 생성, 정지 시 Cancel)
        private CancellationTokenSource? _simCts;

        public Form1()
        {
            InitializeComponent();
            // 서비스 생성 (AppDbContext를 주입해 DB 연결)
            _service = new EquipmentService(new AppDbContext());
            // 시뮬레이터 엔진 생성 (같은 서비스 공유)
            _simulator = new SensorSimulator(_service);
            // 라인/상태/파라미터 콤보박스와 설비 목록 초기화
            LoadLines();
            LoadStatusOptions();
            LoadParamOptions();
            LoadEquipmentList();
        }

        // 라인 콤보박스에 LINE-A, LINE-B를 채우고 첫 항목을 선택함
        private void LoadLines()
        {
            cboLine.Items.Add("LINE-A");
            cboLine.Items.Add("LINE-B");
            cboLine.SelectedIndex = 0;
        }

        // 상태 변경 콤보박스에 변경 가능한 상태 값(IDLE, RUN, STOP, ALARM, DOWN)을 채움
        private void LoadStatusOptions()
        {
            cboStatus.Items.AddRange(new object[] { "IDLE", "RUN", "STOP", "ALARM", "DOWN" });
        }

        // FDC 판정 콤보박스에 측정 파라미터(TEMP, PRESSURE)를 채우고 첫 항목을 선택함
        private void LoadParamOptions()
        {
            cboParam.Items.Add("TEMP");
            cboParam.Items.Add("PRESSURE");
            cboParam.SelectedIndex = 0;
        }

        // 서비스에서 전체 설비 목록을 조회해 마스터 그리드에 표시함
        private void LoadEquipmentList()
        {
            // 서비스로 전체 설비 목록 조회
            List<Equipment> equipmentList = _service.GetEquipmentList();
            // 기존 바인딩 해제 후 새 목록을 그리드에 바인딩
            dgvEquipment.DataSource = null;
            dgvEquipment.DataSource = equipmentList;
            // 컬럼 헤더와 폭 가중치 설정
            SetEquipmentGridHeaders();
            // DOWN 상태 행을 빨간색으로 강조
            ApplyEquipmentRowColors();
        }

        // 설비 상태가 DOWN인 행을 빨간색(LightCoral)으로 강조함
        private void ApplyEquipmentRowColors()
        {
            foreach (DataGridViewRow row in dgvEquipment.Rows)
            {
                // 행의 설비 객체 가져오기
                if (row.DataBoundItem is not Equipment equipment)
                {
                    continue;
                }

                // DOWN이면 빨간색, 아니면 기본색으로 복원 (선택 시에도 유지)
                bool isDown = equipment.Status == "DOWN";
                row.DefaultCellStyle.BackColor = isDown ? Color.LightCoral : Color.White;
                row.DefaultCellStyle.SelectionBackColor = isDown ? Color.LightCoral : SystemColors.Highlight;
            }
        }

        // 설비 그리드의 컬럼 헤더를 한글로 바꾸고 폭 가중치를 지정함
        private void SetEquipmentGridHeaders()
        {
            // 컬럼이 생성되지 않았으면 처리 중단
            if (dgvEquipment.Columns.Count == 0)
            {
                return;
            }

            // 컬럼 헤더를 한글로 표시
            dgvEquipment.Columns["EquipId"]!.HeaderText = "설비코드";
            dgvEquipment.Columns["EquipName"]!.HeaderText = "설비명";
            dgvEquipment.Columns["LineName"]!.HeaderText = "라인명";
            dgvEquipment.Columns["Status"]!.HeaderText = "상태";
            dgvEquipment.Columns["CreateDt"]!.HeaderText = "생성일시";
            // 컬럼 폭 가중치 지정 (생성일시를 상대적으로 넓게)
            dgvEquipment.Columns["EquipId"]!.FillWeight = 1.0f;
            dgvEquipment.Columns["EquipName"]!.FillWeight = 1.2f;
            dgvEquipment.Columns["LineName"]!.FillWeight = 0.8f;
            dgvEquipment.Columns["Status"]!.FillWeight = 0.6f;
            dgvEquipment.Columns["CreateDt"]!.FillWeight = 1.5f;
            // 생성일시는 초 단위를 빼고 짧게 표시해 잘림 방지
            dgvEquipment.Columns["CreateDt"]!.DefaultCellStyle.Format = "yyyy-MM-dd HH:mm";
        }

        // 입력값을 검증하고 새 설비를 등록한 뒤 목록을 새로고침함
        private void btnAdd_Click(object? sender, EventArgs e)
        {
            // 입력값의 앞뒤 공백 제거
            string equipId = txtEquipId.Text.Trim();
            string equipName = txtEquipName.Text.Trim();

            // 필수 입력값(설비ID, 설비명)이 비어 있으면 경고 후 중단
            if (equipId.Length == 0 || equipName.Length == 0)
            {
                MessageBox.Show("설비ID와 설비명을 입력하세요.", "입력 오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 입력값으로 새 설비 객체 생성 (상태는 기본값 IDLE)
            Equipment newEquipment = new Equipment
            {
                EquipId = equipId,
                EquipName = equipName,
                LineName = cboLine.SelectedItem?.ToString() ?? "LINE-A",
                Status = "IDLE"
            };

            try
            {
                // 서비스로 설비 등록 (중복 ID면 예외 발생)
                _service.AddEquipment(newEquipment);
                MessageBox.Show("설비가 등록되었습니다.", "등록 완료",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                // 입력란 초기화 후 목록 새로고침
                txtEquipId.Clear();
                txtEquipName.Clear();
                LoadEquipmentList();
            }
            catch (Exception ex)
            {
                // 중복 설비ID 등 예외 발생 시 오류 메시지 표시
                MessageBox.Show($"등록 실패: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 마스터 그리드에서 선택한 설비의 로그를 조회해 디테일 그리드에 표시함
        private void dgvEquipment_SelectionChanged(object? sender, EventArgs e)
        {
            // 선택된 행에서 설비 객체 가져오기
            if (dgvEquipment.CurrentRow?.DataBoundItem is not Equipment equipment)
            {
                // 선택된 설비가 없으면 로그 그리드 비움
                dgvLogs.DataSource = null;
                return;
            }

            // 선택 설비의 로그를 조회해 디테일 그리드에 바인딩
            List<EquipLog> logList = _service.GetLogsByEquipId(equipment.EquipId);
            dgvLogs.DataSource = null;
            dgvLogs.DataSource = logList;
            SetLogGridHeaders();

            // 상태 콤보박스에 현재 설비 상태 반영
            cboStatus.SelectedItem = equipment.Status;
        }

        // 로그 그리드의 컬럼 헤더를 한글로 바꾸고 폭 가중치를 지정함
        private void SetLogGridHeaders()
        {
            // 컬럼이 생성되지 않았으면 처리 중단
            if (dgvLogs.Columns.Count == 0)
            {
                return;
            }

            // 컬럼 헤더를 한글로 표시
            dgvLogs.Columns["LogId"]!.HeaderText = "로그ID";
            dgvLogs.Columns["EquipId"]!.HeaderText = "설비코드";
            dgvLogs.Columns["LogType"]!.HeaderText = "유형";
            dgvLogs.Columns["LogMsg"]!.HeaderText = "메시지";
            dgvLogs.Columns["OccurDt"]!.HeaderText = "발생일시";
            // 컬럼 폭 가중치 지정 (메시지를 상대적으로 넓게)
            dgvLogs.Columns["LogId"]!.FillWeight = 1.0f;
            dgvLogs.Columns["EquipId"]!.FillWeight = 1.0f;
            dgvLogs.Columns["LogType"]!.FillWeight = 0.8f;
            dgvLogs.Columns["LogMsg"]!.FillWeight = 1.5f;
            dgvLogs.Columns["OccurDt"]!.FillWeight = 1.3f;
            // 발생일시는 초 단위를 빼고 짧게 표시해 잘림 방지
            dgvLogs.Columns["OccurDt"]!.DefaultCellStyle.Format = "yyyy-MM-dd HH:mm";
        }

        // 선택한 설비의 상태를 콤보박스 값으로 변경하고 두 그리드를 새로고침함
        private void btnChangeStatus_Click(object? sender, EventArgs e)
        {
            // 선택된 행에서 설비 객체 가져오기
            if (dgvEquipment.CurrentRow?.DataBoundItem is not Equipment equipment)
            {
                MessageBox.Show("설비를 선택하세요.", "안내",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 콤보박스에서 선택한 새 상태 가져오기 (없으면 기본값 IDLE)
            string newStatus = cboStatus.SelectedItem?.ToString() ?? "IDLE";

            try
            {
                // 서비스로 상태 변경 + 변경 이력 로그 추가 (한 트랜잭션)
                _service.UpdateEquipmentStatus(equipment.EquipId, newStatus);
                MessageBox.Show("상태가 변경되었습니다.", "변경 완료",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                // 변경된 설비를 다시 선택해 마스터/디테일 그리드 갱신
                ReloadWithSelection(equipment.EquipId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"상태 변경 실패: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 설비 목록을 새로고침하고 지정한 설비를 다시 선택해 로그 그리드를 갱신함
        private void ReloadWithSelection(string equipId)
        {
            // 마스터 그리드 새로고침 (선택 변경 시 로그 그리드도 자동 갱신됨)
            LoadEquipmentList();

            // 지정한 설비 행을 다시 선택해 해당 설비의 로그를 표시
            foreach (DataGridViewRow row in dgvEquipment.Rows)
            {
                if (row.DataBoundItem is Equipment equipment && equipment.EquipId == equipId)
                {
                    dgvEquipment.CurrentCell = row.Cells[0];
                    break;
                }
            }
        }

        // 선택 설비의 측정값을 FDC 판정하고 결과를 표시한 뒤 그리드를 갱신함
        private void btnFdcCheck_Click(object? sender, EventArgs e)
        {
            // 선택된 행에서 설비 객체 가져오기
            if (dgvEquipment.CurrentRow?.DataBoundItem is not Equipment equipment)
            {
                MessageBox.Show("설비를 선택하세요.", "안내",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 수집값을 숫자로 변환 (실패 시 경고 후 중단)
            if (!double.TryParse(txtFdcValue.Text.Trim(), out double val))
            {
                MessageBox.Show("수집값을 숫자로 입력하세요.", "입력 오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 콤보박스에서 측정 파라미터 가져오기 (없으면 기본값 TEMP)
            string paramName = cboParam.SelectedItem?.ToString() ?? "TEMP";

            try
            {
                // 서비스로 FDC 판정 (이상 시 상태 DOWN + ALARM 로그 기록)
                FdcResult result = _service.CheckParameterAlarm(equipment.EquipId, paramName, val);
                MessageBox.Show(result.Message, result.IsAlarm ? "ALARM 발생" : "정상 판정",
                    MessageBoxButtons.OK, result.IsAlarm ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
                // 판정 결과를 반영해 마스터/디테일 그리드 갱신
                ReloadWithSelection(equipment.EquipId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"판정 실패: {ex.Message}", "오류",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // 선택 설비에 대해 가상 센서 시뮬레이터를 백그라운드로 시작함
        private void btnSimStart_Click(object? sender, EventArgs e)
        {
            // 선택된 행에서 설비 객체 가져오기
            if (dgvEquipment.CurrentRow?.DataBoundItem is not Equipment equipment)
            {
                MessageBox.Show("설비를 선택하세요.", "안내",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 취소 토큰 소스 생성 후 백그라운드 스레드로 수집 루프 시작
            _simCts = new CancellationTokenSource();
            CancellationToken token = _simCts.Token;
            string equipId = equipment.EquipId;

            // 버튼 상태 전환 (시작 비활성, 정지 활성)
            btnSimStart.Enabled = false;
            btnSimStop.Enabled = true;
            lblSimStatus.Text = $"시뮬레이션 중: {equipId}";

            // UI가 멈추지 않도록 별도 스레드에서 수집 루프 구동
            Task.Run(() => _simulator.RunCollector(equipId, token, OnSimCycle), token);
        }

        // 시뮬레이터를 안전하게 종료함 (Cancel 호출로 루프 중단)
        private void btnSimStop_Click(object? sender, EventArgs e)
        {
            StopSimulator();
        }

        // 시뮬레이터를 취소하고 버튼/상태 라벨을 초기 상태로 되돌림
        private void StopSimulator()
        {
            _simCts?.Cancel();
            _simCts?.Dispose();
            _simCts = null;

            // 버튼 상태 전환 (시작 활성, 정지 비활성)
            btnSimStart.Enabled = true;
            btnSimStop.Enabled = false;
            lblSimStatus.Text = "정지됨";
        }

        // 시뮬레이터 한 주기 결과를 UI 스레드로 마샬링해 그리드와 상태를 갱신함
        private void OnSimCycle(SensorCycleResult cycle)
        {
            // 백그라운드 스레드에서 호출되므로 UI 스레드로 전환
            if (InvokeRequired)
            {
                BeginInvoke(new Action<SensorCycleResult>(OnSimCycle), cycle);
                return;
            }

            // 오류 발생 시 상태 표시 (규칙 미설정 등)
            if (cycle.Error != null)
            {
                lblSimStatus.Text = $"오류: {cycle.Error}";
                btnSimStart.Enabled = true;
                btnSimStop.Enabled = false;
                return;
            }

            // 현재 수집값/판정 결과를 상태 라벨에 표시
            string resultText = cycle.Result?.IsAlarm == true ? "ALARM" : "정상";
            lblSimStatus.Text = $"온도 {cycle.TempVal}℃ / 압력 {cycle.PressVal}bar / {resultText}";

            // 선택 설비의 상태/로그를 그리드에 반영
            if (dgvEquipment.CurrentRow?.DataBoundItem is Equipment equipment)
            {
                ReloadWithSelection(equipment.EquipId);
            }

            // 알람(DOWN) 감지 시 시뮬레이터 자동 정지
            if (cycle.Result?.IsAlarm == true)
            {
                lblSimStatus.Text = $"DOWN 감지 - 시뮬레이터 정지: {cycle.Result.Message}";
                StopSimulator();
            }
        }

        // 폼 닫힘 시 시뮬레이터가 실행 중이면 안전하게 종료함
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            _simCts?.Cancel();
            _simCts?.Dispose();
            base.OnFormClosing(e);
        }
    }
}
