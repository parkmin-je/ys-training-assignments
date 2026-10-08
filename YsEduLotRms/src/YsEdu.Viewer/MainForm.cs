using YsEdu.Core.Config;
using YsEdu.Core.Data;
using YsEdu.Core.Data.Entities;
using YsEdu.Core.Messaging;
using YsEdu.Core.Services;

namespace YsEdu.Viewer;

/// <summary>
/// WinForm 조회 화면 (2차 과제 p.10)
///   · LOT 검색 : LOTID로 작업 시작·종료와 A · B · C 설비별 구동 이력 조회, 설비와 기간 조건 제공
///   · 상태와 이력 : 제품·LOT·STEP·설비·Recipe·이벤트·발생시간·처리결과 표시,
///                  LOT 작업 상태와 개별 설비 구동 상태를 구분 (탭 1의 위·아래 표)
///   · RMS 상세 : 검증 이력 선택 → 종합 판정과 Parameter별 기준값·수신값·판정·사유,
///                같은 LOT의 여러 설비·재검증을 검증 식별키(CHECK_ID)로 구분
/// 화면은 조회 결과 표시만 하고 DB 접근은 HistoryQueryService(Core)가 한다 (UI와 업무 처리 분리).
/// </summary>
public partial class MainForm : Form
{
    private const string AllEquipments = "(전체)";
    private HistoryQueryService? _query;
    private List<TbRmsCheck> _checks = [];
    private List<TbMsgLog> _messages = [];
    private bool _searching;

    public MainForm()
    {
        InitializeComponent();
    }

    private async void MainForm_Load(object sender, EventArgs e)
    {
        ResetConditions();
        try
        {
            var settings = AppSettings.Load();
            _query = new HistoryQueryService(new DbFactory(settings.ConnectionString));
            cboEqp.Items.Add(AllEquipments);
            foreach (var id in await _query.GetEquipmentIdsAsync()) cboEqp.Items.Add(id);
            cboEqp.SelectedIndex = 0;
            await SearchAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show("DB에 연결하지 못했습니다.\n" + ex.GetBaseException().Message, "연결 실패", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ResetConditions()
    {
        txtLotId.Text = "";
        if (cboEqp.Items.Count > 0) cboEqp.SelectedIndex = 0;
        dtpFrom.Value = DateTime.Today.AddDays(-1);
        dtpTo.Value = DateTime.Today.AddDays(1).AddMinutes(-1);
    }

    private async void btnSearch_Click(object sender, EventArgs e) => await SearchAsync();

    private void txtLotId_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter) btnSearch.PerformClick();
    }

    private void btnClear_Click(object sender, EventArgs e)
    {
        ResetConditions();
        ClearGrids();
        lblResult.Text = "조회 조건을 초기화했습니다.";
    }

    private void ClearGrids()
    {
        foreach (var grid in new[] { dgvLots, dgvEquipments, dgvEvents, dgvRuns, dgvChecks, dgvCheckDetails, dgvMsgLog })
            grid.DataSource = null;
        txtRawXml.Clear();
        _checks = [];
        _messages = [];
    }

    /// 조회 중에는 버튼을 막아 중복 실행을 방지하고, 재조회 시 기존 표를 비운다
    private async Task SearchAsync()
    {
        if (_query == null || _searching) return;
        if (dtpFrom.Value > dtpTo.Value)
        {
            MessageBox.Show("기간 시작이 종료보다 늦습니다.");
            return;
        }

        _searching = true;
        btnSearch.Enabled = false;
        lblResult.Text = "조회 중…";
        ClearGrids();
        try
        {
            var filter = new HistoryFilter(txtLotId.Text.Trim(),
                                           cboEqp.Text == AllEquipments ? null : cboEqp.Text,
                                           new DateTimeOffset(dtpFrom.Value), new DateTimeOffset(dtpTo.Value));
            var result = await _query.SearchAsync(filter);
            _messages = await _query.GetMessageLogAsync(filter);
            Bind(result);
            lblResult.Text = result.Events.Count == 0
                ? "조회 결과가 없습니다."
                : $"LOT {result.Lots.Count} · 이벤트 {result.Events.Count} · 구동 {result.Runs.Count} · RMS 검증 {result.Checks.Count} · 수신 {_messages.Count}";
        }
        catch (Exception ex)
        {
            lblResult.Text = "조회 실패";
            MessageBox.Show("조회 중 오류가 발생했습니다.\n" + ex.GetBaseException().Message, "조회 실패", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnSearch.Enabled = true;
            _searching = false;
        }
    }

    private void Bind(HistoryResult r)
    {
        dgvLots.DataSource = r.Lots.Select(l => new
        {
            LOTID = l.LotId, 제품 = l.ProductId, STEP = l.StepId, LOT작업상태 = l.LotStatus, 현재설비 = l.CurEqpId,
            작업시작 = Time(l.StartTime), 작업종료 = Time(l.EndTime), 시작MessageName = l.StartMsgId, 종료MessageName = l.EndMsgId
        }).ToList();

        dgvEquipments.DataSource = r.Equipments.Select(q => new
        {
            설비 = q.EqpId, 설비명 = q.EqpName, 라인 = q.LineId, 설비구동상태 = q.EqpStatus, 구동중LOT = q.CurLotId,
            최종변경 = q.UpdatedAt?.ToString("yyyy-MM-dd HH:mm:ss")
        }).ToList();

        dgvEvents.DataSource = r.Events.Select(v => new
        {
            발생시간 = Time(v.EventTime), 이벤트 = v.EventType, 제품 = v.ProductId, LOTID = v.LotId, STEP = v.StepId, 설비 = v.EqpId,
            Recipe = v.RecipeId, 처리결과 = v.Result, 사유 = v.Reason, 구동ID = v.RunId, 검증ID = v.CheckId, MessageName = v.MessageId
        }).ToList();

        dgvRuns.DataSource = r.Runs.Select(u => new
        {
            구동ID = u.RunId, LOTID = u.LotId, 제품 = u.ProductId, STEP = u.StepId, 설비 = u.EqpId, Recipe = u.RecipeId,
            구동상태 = u.RunStatus, 구동시작 = Time(u.StartTime), 구동종료 = Time(u.EndTime),
            소요초 = u.EndTime == null ? (double?)null : Math.Round((u.EndTime.Value - u.StartTime).TotalSeconds, 1),
            검증ID = u.CheckId, 시작MessageName = u.StartMsgId, 종료MessageName = u.EndMsgId
        }).ToList();

        _checks = r.Checks;
        dgvChecks.DataSource = _checks.Select(c => new
        {
            검증ID = c.CheckId, 발생시간 = Time(c.EventTime), LOTID = c.LotId, 설비 = c.EqpId, 제품 = c.ProductId, STEP = c.StepId,
            수신Recipe = c.RecvRecipeId, 기준Recipe = c.BaseRecipeId, 종합판정 = c.Result, 사유 = c.Reason, MessageName = c.MessageId
        }).ToList();

        dgvMsgLog.DataSource = _messages.Select(m => new
        {
            수신시각 = m.LoggedAt.ToString("yyyy-MM-dd HH:mm:ss.fff"), 처리결과 = m.Result, 사유 = m.Reason, 이벤트 = m.EventType,
            설비 = m.EqpId, LOTID = m.LotId, MessageName = m.MessageId, 위치 = $"P{m.PartitionNo}:{m.OffsetNo}"
        }).ToList();

        Color(dgvEvents, "처리결과");
        Color(dgvChecks, "종합판정");
        Color(dgvMsgLog, "처리결과");
    }

    private static string? Time(DateTimeOffset? t) => t?.ToString("yyyy-MM-dd HH:mm:ss.fff zzz");

    /// 판정 열 값에 따라 행 색상 표시 (NG 노랑, ERROR 빨강)
    private static void Color(DataGridView grid, string column)
    {
        foreach (DataGridViewRow row in grid.Rows)
        {
            var value = row.Cells[column].Value?.ToString();
            row.DefaultCellStyle.BackColor = value == Results.ERROR ? System.Drawing.Color.MistyRose
                                           : value == Results.NG ? System.Drawing.Color.LemonChiffon
                                           : System.Drawing.Color.White;
        }
    }

    /// 마스터(검증 이력) 선택 → 디테일(항목별 판정) 조회
    private async void dgvChecks_SelectionChanged(object sender, EventArgs e)
    {
        dgvCheckDetails.DataSource = null;
        if (_query == null || dgvChecks.CurrentRow == null) return;
        int index = dgvChecks.CurrentRow.Index;
        if (index < 0 || index >= _checks.Count) return;

        var check = _checks[index];
        grpCheckDetails.Text = $"검증ID {check.CheckId} · {check.EqpId} · 종합 {check.Result} — Parameter별 기준값 · 수신값 · 판정 · 사유";
        try
        {
            var details = await _query.GetCheckDetailsAsync(check.CheckId);
            if (dgvChecks.CurrentRow?.Index != index) return;      // 그 사이 다른 행을 선택했으면 버림
            dgvCheckDetails.DataSource = details.Select(d => new
            {
                구분 = d.ItemType, 항목 = d.ItemId, 기준값 = d.BaseValue, 수신값 = d.RecvValue, 판정 = d.Judge, 사유 = d.Reason
            }).ToList();
            Color(dgvCheckDetails, "판정");
        }
        catch (Exception ex)
        {
            MessageBox.Show("상세 조회 실패\n" + ex.GetBaseException().Message);
        }
    }

    private void dgvMsgLog_SelectionChanged(object sender, EventArgs e)
    {
        if (dgvMsgLog.CurrentRow == null) return;
        int index = dgvMsgLog.CurrentRow.Index;
        if (index < 0 || index >= _messages.Count) return;
        var m = _messages[index];
        txtRawXml.Text = $"[{m.Result}] {m.Reason}\r\n\r\n{m.RawXml.Replace("\n", "\r\n")}";
    }
}
