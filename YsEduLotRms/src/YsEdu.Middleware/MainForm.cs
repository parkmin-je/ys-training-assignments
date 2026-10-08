using System.ComponentModel;
using YsEdu.Core.Config;
using YsEdu.Core.Logging;
using YsEdu.Core.Messaging;
using YsEdu.Core.Services;

namespace YsEdu.Middleware;

/// <summary>
/// 미들웨어 실행 화면. 업무 처리는 MiddlewareWorker(Core)가 하고, 화면은 시작·중지와 처리 결과 표시만 한다.
///   · 수신 대기는 백그라운드 Task에서 → UI가 멈추지 않는다
///   · 수신 스레드의 결과는 BeginInvoke로 UI 스레드에서 그린다
///   · 창을 닫으면 수신 작업이 실제로 끝난 것을 확인한 뒤 종료한다
/// </summary>
public partial class MainForm : Form
{
    private readonly BindingList<ProcessedInfo> _rows = new();
    private AppSettings? _settings;
    private FileLog? _log;
    private MiddlewareWorker? _worker;
    private int _ok, _ng, _error, _duplicate;
    private bool _closing;

    public MainForm()
    {
        InitializeComponent();
    }

    private void MainForm_Load(object sender, EventArgs e)
    {
        try
        {
            _settings = AppSettings.Load();
        }
        catch (Exception ex)
        {
            MessageBox.Show("appsettings.json을 읽지 못했습니다.\n" + ex.Message, "설정 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            btnStart.Enabled = false;
            return;
        }

        _log = new FileLog(_settings.LogDirectory, "Middleware");
        _log.Written += OnLogWritten;
        _worker = new MiddlewareWorker(_settings, _log);
        _worker.Processed += OnProcessed;

        dgvMessages.AutoGenerateColumns = false;
        AddColumn("시간", nameof(ProcessedInfo.Time), "HH:mm:ss.fff");
        AddColumn("Partition", nameof(ProcessedInfo.Partition));
        AddColumn("Offset", nameof(ProcessedInfo.Offset));
        AddColumn("MessageName", nameof(ProcessedInfo.MessageName));
        AddColumn("EventType", nameof(ProcessedInfo.EventType));
        AddColumn("EQPID", nameof(ProcessedInfo.EqpId));
        AddColumn("LOTID", nameof(ProcessedInfo.LotId));
        AddColumn("Result", nameof(ProcessedInfo.Result));
        AddColumn("Reason", nameof(ProcessedInfo.Reason));
        AddColumn("중복", nameof(ProcessedInfo.Duplicate));
        dgvMessages.DataSource = _rows;
        dgvMessages.RowPrePaint += (_, args) =>
        {
            var row = _rows[args.RowIndex];
            dgvMessages.Rows[args.RowIndex].DefaultCellStyle.BackColor =
                row.Result == Results.ERROR ? Color.MistyRose : row.Result == Results.NG ? Color.LemonChiffon
                : row.Duplicate ? Color.Gainsboro : Color.White;
        };

        Text += $"   [{_settings.BootstrapServers}]";
    }

    private void AddColumn(string header, string property, string? format = null)
    {
        var column = new DataGridViewTextBoxColumn { HeaderText = header, DataPropertyName = property, Name = "col" + property };
        if (format != null) column.DefaultCellStyle.Format = format;
        dgvMessages.Columns.Add(column);
    }

    private void btnStart_Click(object sender, EventArgs e)
    {
        _worker?.Start();
        SetRunning(true);
    }

    private async void btnStop_Click(object sender, EventArgs e)
    {
        btnStop.Enabled = false;
        lblStatus.Text = "… 중지 중";
        if (_worker != null) await _worker.StopAsync();
        SetRunning(false);
    }

    private void SetRunning(bool running)
    {
        btnStart.Enabled = !running;
        btnStop.Enabled = running;
        lblStatus.Text = running ? "● 수신 중" : "■ 중지";
        lblStatus.ForeColor = running ? Color.ForestGreen : Color.DimGray;
    }

    /// 수신 스레드에서 호출 → UI 스레드로 넘긴다
    private void OnProcessed(ProcessedInfo info)
    {
        if (InvokeRequired)
        {
            if (IsDisposed || !IsHandleCreated) return;
            BeginInvoke(new Action<ProcessedInfo>(OnProcessed), info);
            return;
        }

        _rows.Insert(0, info);
        while (_rows.Count > 1000) _rows.RemoveAt(_rows.Count - 1);

        if (info.Duplicate) _duplicate++;
        else if (info.Result == Results.OK) _ok++;
        else if (info.Result == Results.NG) _ng++;
        else _error++;
        lblCount.Text = $"처리 {_ok + _ng + _error + _duplicate}  ·  OK {_ok}  ·  NG {_ng}  ·  ERROR {_error}  ·  중복 {_duplicate}";
    }

    private void OnLogWritten(string line)
    {
        if (InvokeRequired)
        {
            if (IsDisposed || !IsHandleCreated) return;
            BeginInvoke(new Action<string>(OnLogWritten), line);
            return;
        }
        lstLog.Items.Insert(0, line);
        while (lstLog.Items.Count > 1000) lstLog.Items.RemoveAt(lstLog.Items.Count - 1);
    }

    private void dgvMessages_SelectionChanged(object sender, EventArgs e)
    {
        if (dgvMessages.CurrentRow?.DataBoundItem is not ProcessedInfo info) return;
        txtXml.Text = $"[요청 {info.Topic} P{info.Partition}:{info.Offset}]\r\n{info.RequestXml.Replace("\n", "\r\n")}\r\n\r\n[응답]\r\n{info.ResponseXml.Replace("\n", "\r\n")}";
    }

    /// 창을 닫을 때: 수신 작업 종료를 확인한 뒤 닫는다
    private async void MainForm_FormClosing(object sender, FormClosingEventArgs e)
    {
        if (_closing || _worker == null || !_worker.IsRunning) return;
        e.Cancel = true;
        _closing = true;
        lblStatus.Text = "… 종료 중";
        await _worker.StopAsync();
        Close();
    }
}
