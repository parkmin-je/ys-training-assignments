using System.ComponentModel;
using YsEdu.Core.Config;
using YsEdu.Core.Data;
using YsEdu.Core.Messaging;
using YsEdu.Core.Simulation;

namespace YsEdu.Simulator;

/// <summary>
/// 설비 / CIM 시뮬레이터 화면.
///   · 시나리오 버튼: 2차 과제 p.12 완료 확인 시나리오를 그대로 송신 (RMS_CHECK OK일 때만 구동)
///   · 직접 송신: XML을 만들거나 고쳐서 보내고, 같은 XML을 다시 보내 중복 처리를 확인
/// 통신은 백그라운드에서 일어나므로 결과는 BeginInvoke로 UI 스레드에서 그린다.
/// </summary>
public partial class MainForm : Form
{
    private readonly BindingList<TraceItem> _trace = new();
    private AppSettings? _settings;
    private EquipmentClient? _client;
    private InlineSimulator? _sim;
    private string? _lastXml, _lastMessageName, _lastEqp, _lastEventType;

    public MainForm()
    {
        InitializeComponent();
    }

    private async void MainForm_Load(object sender, EventArgs e)
    {
        cboEqp.Items.AddRange(["EQP_A", "EQP_B", "EQP_C", "EQP_X(미등록)"]);
        cboEqp.SelectedIndex = 0;
        cboEventType.Items.AddRange([.. EventTypes.All]);
        cboEventType.SelectedItem = EventTypes.RmsCheck;

        dgvTrace.AutoGenerateColumns = false;
        AddColumn("시간", nameof(TraceItem.Time), "HH:mm:ss.fff");
        AddColumn("방향", nameof(TraceItem.Direction));
        AddColumn("EQPID", nameof(TraceItem.EqpId));
        AddColumn("MessageName", nameof(TraceItem.MessageName));
        AddColumn("EventType", nameof(TraceItem.EventType));
        AddColumn("Result", nameof(TraceItem.Result));
        AddColumn("Reason", nameof(TraceItem.Reason));
        dgvTrace.DataSource = _trace;

        SetButtons(false);
        try
        {
            _settings = AppSettings.Load();
            _client = new EquipmentClient(_settings);
            _client.Traced += OnTraced;
            await _client.StartAsync();
            _sim = new InlineSimulator(_client);
            _sim.StepCompleted += OnStepCompleted;
            lblConnection.Text = $"● Kafka {_settings.BootstrapServers}\n요청 {_settings.RequestTopic} / 응답 {_settings.ResponseTopic}";
            lblConnection.ForeColor = Color.ForestGreen;
            SetButtons(true);
        }
        catch (Exception ex)
        {
            lblConnection.Text = "× Kafka 연결 실패 — C:\\kafka\\start-kafka.bat 실행 확인";
            lblConnection.ForeColor = Color.Firebrick;
            MessageBox.Show(ex.Message, "연결 실패", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void AddColumn(string header, string property, string? format = null)
    {
        var column = new DataGridViewTextBoxColumn { HeaderText = header, DataPropertyName = property, Name = "col" + property };
        if (format != null) column.DefaultCellStyle.Format = format;
        dgvTrace.Columns.Add(column);
    }

    private void SetButtons(bool enabled)
    {
        foreach (var button in new[] { btnNormal, btnMiddle, btnMismatch, btnStructure, btnOrderDup, btnErrors, btnResetHistory, btnBuild, btnSend, btnResend })
            button.Enabled = enabled;
    }

    // ------------------------------------------------------------------ 시나리오
    private void btnNormal_Click(object sender, EventArgs e) => RunScenario("① 정상 인라인", lot => _sim!.NormalInlineAsync(lot));
    private void btnMiddle_Click(object sender, EventArgs e) => RunScenario("② 중간 설비", lot => _sim!.MiddleEquipmentAsync(lot));
    private void btnMismatch_Click(object sender, EventArgs e) => RunScenario("③ RMS 불일치", lot => _sim!.RmsMismatchAsync(lot));
    private void btnStructure_Click(object sender, EventArgs e) => RunScenario("④ 구성 오류", lot => _sim!.StructureErrorsAsync(lot));
    private void btnOrderDup_Click(object sender, EventArgs e) => RunScenario("⑤ 순서 · 중복", lot => _sim!.OrderAndDuplicateAsync(lot));
    private void btnErrors_Click(object sender, EventArgs e) => RunScenario("⑥ 처리 오류", lot => _sim!.ProcessingErrorsAsync(lot));

    /// 시나리오 실행 중에는 버튼을 막아 중복 실행을 방지한다
    private async void RunScenario(string title, Func<string, Task<List<ScenarioStep>>> scenario)
    {
        string lot = txtLotId.Text.Trim();
        if (lot.Length == 0)
        {
            MessageBox.Show("LOTID를 입력하세요.");
            return;
        }

        SetButtons(false);
        lstSteps.Items.Insert(0, $"■ {title} 시작 — {lot}");
        try
        {
            var steps = await Task.Run(() => scenario(lot));
            int failed = steps.Count(s => !s.Pass);
            lstSteps.Items.Insert(0, $"■ {title} 완료 — {steps.Count}단계, 기대와 다름 {failed}건");
            txtLotId.Text = NextLotId(lot);     // 다음 시나리오는 새 LOT으로 (끝난 LOT에 이어 보내면 LOT_ALREADY_DONE)
        }
        catch (Exception ex)
        {
            lstSteps.Items.Insert(0, $"× {title} 실패: {ex.Message}");
        }
        finally
        {
            SetButtons(true);
        }
    }

    /// LOT_001 → LOT_002
    private static string NextLotId(string lot)
    {
        int i = lot.Length;
        while (i > 0 && char.IsDigit(lot[i - 1])) i--;
        if (i == lot.Length) return lot;
        string digits = lot[i..];
        return lot[..i] + (long.Parse(digits) + 1).ToString().PadLeft(digits.Length, '0');
    }

    private async void btnResetHistory_Click(object sender, EventArgs e)
    {
        if (MessageBox.Show("LOT·구동·RMS·메시지 이력을 모두 지웁니다. 기준정보는 유지됩니다.\n계속할까요?", "이력 초기화",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;
        try
        {
            await HistoryReset.RunAsync(new DbFactory(_settings!.ConnectionString));
            lstSteps.Items.Insert(0, "■ 이력 초기화 완료");
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "초기화 실패", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // ------------------------------------------------------------------ 직접 송신
    private void btnBuild_Click(object sender, EventArgs e)
    {
        string eqp = cboEqp.Text.Split('(')[0];
        string eventType = cboEventType.Text;
        string recipe = eqp switch { "EQP_A" => "RCP_A01", "EQP_B" => "RCP_B01", "EQP_C" => "RCP_C01", _ => "RCP_X01" };
        var parameters = eventType == EventTypes.RmsCheck ? ParseParams(txtParams.Text) : null;

        var ev = _sim!.NewEvent(txtLotId.Text.Trim(), eqp, recipe, eventType, parameters);
        txtXml.Text = XmlMessage.Build(ev).Replace("\n", "\r\n");
    }

    private static List<EventParameter> ParseParams(string text)
        => text.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
               .Select(p => p.Split('=', 2))
               .Select(kv => new EventParameter(kv[0].Trim(), kv.Length > 1 ? kv[1].Trim() : ""))
               .ToList();

    private async void btnSend_Click(object sender, EventArgs e)
    {
        string xml = txtXml.Text;
        if (string.IsNullOrWhiteSpace(xml))
        {
            MessageBox.Show("먼저 [XML 만들기]를 누르거나 XML을 입력하세요.");
            return;
        }
        // 응답의 MessageName으로 요청과 응답을 연결 (구조가 깨진 XML이면 응답 MessageName이 빈 값)
        var parsed = XmlMessage.TryParse(xml, out _);
        _lastXml = xml;
        _lastMessageName = parsed?.MessageName ?? "";
        _lastEqp = parsed?.EqpId is { Length: > 0 } id ? id : cboEqp.Text.Split('(')[0];
        _lastEventType = parsed?.EventType ?? cboEventType.Text;
        await SendLastAsync();
    }

    private async void btnResend_Click(object sender, EventArgs e)
    {
        if (_lastXml == null)
        {
            MessageBox.Show("먼저 한 번 송신하세요.");
            return;
        }
        await SendLastAsync();
    }

    private async Task SendLastAsync()
    {
        SetButtons(false);
        try
        {
            await Task.Run(() => _client!.SendRawAsync(_lastMessageName!, _lastEqp!, _lastEventType!, _lastXml!));
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "송신 실패", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            SetButtons(true);
        }
    }

    // ------------------------------------------------------------------ 표시 (통신 스레드 → UI 스레드)
    private void OnTraced(TraceItem item)
    {
        if (InvokeRequired)
        {
            if (IsDisposed || !IsHandleCreated) return;
            BeginInvoke(new Action<TraceItem>(OnTraced), item);
            return;
        }
        _trace.Insert(0, item);
        while (_trace.Count > 1000) _trace.RemoveAt(_trace.Count - 1);
    }

    private void OnStepCompleted(ScenarioStep step)
    {
        if (InvokeRequired)
        {
            if (IsDisposed || !IsHandleCreated) return;
            BeginInvoke(new Action<ScenarioStep>(OnStepCompleted), step);
            return;
        }
        lstSteps.Items.Insert(0, $"  {(step.Pass ? "✔" : "✘")} {step.Title} — 기대 {step.ExpectedResult} {step.ExpectedReason} / 실제 {step.Actual}");
    }

    private void dgvTrace_SelectionChanged(object sender, EventArgs e)
    {
        if (dgvTrace.CurrentRow?.DataBoundItem is TraceItem item && item.Xml.Length > 0)
            txtXml.Text = item.Xml.Replace("\n", "\r\n");
    }

    private async void MainForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (_client == null) return;
        var client = _client;
        _client = null;
        await client.DisposeAsync();
    }
}
