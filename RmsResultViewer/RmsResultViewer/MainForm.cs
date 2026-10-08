using RmsResultViewer.Data.Entities;
using RmsResultViewer.Services;

namespace RmsResultViewer;

/// <summary>
/// RMS 결과 조회 화면 (조회 p.5·6)
///   조건: 이벤트 기간(필수) · 설비 ID · 레시피 ID — 복수 조건 함께 적용, [초기화]
///   Master 행 선택 → Detail 자동 조회 / 결과 없으면 빈 Grid + 안내 / 재조회 시 기존 Grid 정리 / 조회 중 중복 실행 방지
/// </summary>
public partial class MainForm : Form
{
    private readonly ResultQueryService _service = new();
    private List<TbRmsResult> _results = [];
    private bool _searching;
    private int _detailVersion;

    public MainForm()
    {
        InitializeComponent();
        dgvMaster.AutoGenerateColumns = false;
        dgvDetail.AutoGenerateColumns = false;
        ResetConditions();
    }

    private void ResetConditions()
    {
        dtpFrom.Value = DateTime.Today;
        dtpTo.Value = DateTime.Today.AddDays(1).AddSeconds(-1);
        txtEquipId.Clear();
        txtRecipeId.Clear();
    }

    private void btnReset_Click(object sender, EventArgs e)
    {
        ResetConditions();
        ClearGrids();
        lblStatus.Text = "조회조건을 초기화했습니다.";
    }

    private void ClearGrids()
    {
        _detailVersion++;
        _results = [];
        dgvMaster.DataSource = null;
        dgvDetail.DataSource = null;
    }

    private async void btnSearch_Click(object sender, EventArgs e)
    {
        if (_searching) return;                                     // 조회 중 중복 실행 방지
        _searching = true;
        btnSearch.Enabled = false;
        btnSearch.Text = "조회 중…";
        ClearGrids();                                               // 재조회 시 기존 Grid 데이터 정리
        try
        {
            var filter = new ResultFilter(dtpFrom.Value, dtpTo.Value, txtEquipId.Text, txtRecipeId.Text);
            _results = await _service.SearchAsync(filter);
            dgvMaster.DataSource = _results;
            lblStatus.Text = _results.Count == 0
                ? "조회 결과가 없습니다. 기간·설비 ID·레시피 ID를 확인하세요."    // 결과 없으면 빈 Grid와 안내
                : $"결과 {_results.Count}건 (최신 이벤트 시간 순)";
        }
        catch (ArgumentException ex)
        {
            MessageBox.Show(ex.Message, "조회조건 확인", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            lblStatus.Text = ex.Message;
        }
        catch (Exception ex)
        {
            lblStatus.Text = "DB 조회 실패";                             // DB 조회 실패 시 사용자에게 표시
            MessageBox.Show("DB 조회에 실패했습니다.\n" + ex.GetBaseException().Message, "조회 실패", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnSearch.Text = "조회";
            btnSearch.Enabled = true;                                // 완료 후 복구
            _searching = false;
        }
    }

    /// Master 행 선택 시 Detail 자동 조회
    private async void dgvMaster_SelectionChanged(object sender, EventArgs e)
    {
        if (dgvMaster.CurrentRow?.DataBoundItem is not TbRmsResult master) return;
        int version = ++_detailVersion;
        grpDetail.Text = $"선택 결과의 파라미터 — RESULT_ID {master.ResultId} ({master.RecipeId})";
        try
        {
            var rows = await _service.GetParametersAsync(master.ResultId);
            if (version != _detailVersion) return;                  // 그 사이 다른 행을 선택했으면 버림
            dgvDetail.DataSource = rows;
        }
        catch (Exception ex)
        {
            MessageBox.Show("파라미터 조회 실패\n" + ex.GetBaseException().Message, "조회 실패", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
