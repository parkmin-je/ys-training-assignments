using System.ComponentModel;
using RmsKafkaMiddleware.Common;
using RmsKafkaMiddleware.Messaging;
using RmsKafkaMiddleware.Services;

namespace RmsKafkaMiddleware;

/// <summary>
/// 미들웨어 실행 화면 — 수신 시작·중지와 처리 결과 표시.
/// 수신은 KafkaWorker(백그라운드 Task)가 하므로 화면이 멈추지 않고, 결과는 BeginInvoke로 UI 스레드에서 그린다.
/// </summary>
public partial class MainForm : Form
{
    private readonly BindingList<HandledMessage> _rows = new();
    private KafkaWorker? _worker;
    private int _success, _duplicate, _error;
    private bool _closing;

    public MainForm()
    {
        InitializeComponent();
    }

    private void MainForm_Load(object sender, EventArgs e)
    {
        AppSettings settings;
        try
        {
            settings = AppSettings.Load();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "설정 오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            btnStart.Enabled = false;
            return;
        }

        var log = new FileLog(settings.LogDirectory);
        log.Written += OnLog;
        _worker = new KafkaWorker(settings, log);
        _worker.Handled += OnHandled;

        dgvMessages.AutoGenerateColumns = false;
        AddColumn("처리 시각", nameof(HandledMessage.Time), "HH:mm:ss.fff");
        AddColumn("Partition", nameof(HandledMessage.Partition));
        AddColumn("Offset", nameof(HandledMessage.Offset));
        AddColumn("RESULT_CODE", nameof(HandledMessage.ResultCode));
        AddColumn("EQP_ID", nameof(HandledMessage.EqpId));
        AddColumn("RECIPE_ID", nameof(HandledMessage.RecipeId));
        AddColumn("RESULT_ID", nameof(HandledMessage.ResultId));
        AddColumn("RESULT_MESSAGE", nameof(HandledMessage.ResultMessage));
        dgvMessages.DataSource = _rows;
        dgvMessages.RowPrePaint += (_, a) =>
        {
            string code = _rows[a.RowIndex].ResultCode;
            dgvMessages.Rows[a.RowIndex].DefaultCellStyle.BackColor =
                code == ResultCodes.Success ? Color.White : code == ResultCodes.Duplicate ? Color.Gainsboro : Color.MistyRose;
        };
        Text += $"   [{settings.BootstrapServers}]";
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

    private void OnHandled(HandledMessage m)
    {
        if (InvokeRequired)
        {
            if (IsDisposed || !IsHandleCreated) return;
            BeginInvoke(new Action<HandledMessage>(OnHandled), m);
            return;
        }
        _rows.Insert(0, m);
        while (_rows.Count > 1000) _rows.RemoveAt(_rows.Count - 1);

        if (m.ResultCode == ResultCodes.Success) _success++;
        else if (m.ResultCode == ResultCodes.Duplicate) _duplicate++;
        else _error++;
        lblCount.Text = $"처리 {_success + _duplicate + _error} · 성공 {_success} · 중복 {_duplicate} · 오류 {_error}";
    }

    private void OnLog(string line)
    {
        if (InvokeRequired)
        {
            if (IsDisposed || !IsHandleCreated) return;
            BeginInvoke(new Action<string>(OnLog), line);
            return;
        }
        lstLog.Items.Insert(0, line);
        while (lstLog.Items.Count > 1000) lstLog.Items.RemoveAt(lstLog.Items.Count - 1);
    }

    private void dgvMessages_SelectionChanged(object sender, EventArgs e)
    {
        if (dgvMessages.CurrentRow?.DataBoundItem is not HandledMessage m) return;
        txtXml.Text = $"[요청 P{m.Partition}:{m.Offset}]\r\n{m.RequestXml.Replace("\n", "\r\n")}\r\n\r\n[응답]\r\n{m.ResponseXml.Replace("\n", "\r\n")}";
    }

    /// 창을 닫을 때 수신 작업 종료를 확인한 뒤 닫는다
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
