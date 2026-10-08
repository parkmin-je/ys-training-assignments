using System.ComponentModel;
using RmsMasterData.Common;
using RmsMasterData.Data.Entities;
using RmsMasterData.Services;

namespace RmsMasterData.Controls;

/// <summary>
/// 레시피 기준정보 화면 (기준 p.5) — 레시피(Master)와 파라미터(Detail)를 한 화면에서 관리
///   · 레시피 행 선택 → 그 레시피의 파라미터를 아래 표에 표시 (Master–Detail)
///   · [저장] → 레시피 + 파라미터를 하나의 작업으로 저장 (RecipeService)
/// </summary>
public partial class RecipeControl : UserControl
{
    private const string All = "(전체)";
    private readonly RecipeService _service = new();
    private readonly EquipmentService _equipments = new();
    private BindingList<ParameterRow> _parameters = new();
    private bool _isNew = true;
    private bool _busy;
    private int _detailVersion;

    public RecipeControl()
    {
        InitializeComponent();
        dgvRecipe.AutoGenerateColumns = false;
        dgvParameter.AutoGenerateColumns = false;
        dgvParameter.DataSource = _parameters;
    }

    private async void RecipeControl_Load(object sender, EventArgs e)
    {
        if (DesignMode) return;
        await RunAsync(async () =>
        {
            await LoadEquipmentListAsync();
            SetNewMode();
            await LoadListAsync();
        });
    }

    /// 설비 화면에서 바뀐 설비 목록을 다시 읽는다 (탭 전환 시 호출)
    public async Task RefreshEquipmentListAsync() => await RunAsync(LoadEquipmentListAsync);

    private async Task LoadEquipmentListAsync()
    {
        var ids = await _equipments.GetEquipIdsAsync();
        string? searchSelected = cboSearchEquipId.SelectedItem as string;
        string? editSelected = cboEquipId.SelectedItem as string;

        cboSearchEquipId.Items.Clear();
        cboSearchEquipId.Items.Add(All);
        cboSearchEquipId.Items.AddRange([.. ids]);
        cboSearchEquipId.SelectedItem = searchSelected != null && cboSearchEquipId.Items.Contains(searchSelected) ? searchSelected : All;

        cboEquipId.Items.Clear();
        cboEquipId.Items.AddRange([.. ids]);
        if (editSelected != null && cboEquipId.Items.Contains(editSelected)) cboEquipId.SelectedItem = editSelected;
    }

    private async void btnSearch_Click(object sender, EventArgs e) => await RunAsync(() => LoadListAsync());

    private void txtSearchRecipeId_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter) btnSearch.PerformClick();
    }

    private async Task LoadListAsync(string? selectId = null)
    {
        string? equip = cboSearchEquipId.Text == All ? null : cboSearchEquipId.Text;
        var list = await _service.SearchAsync(equip, txtSearchRecipeId.Text);
        dgvRecipe.DataSource = list;
        lblStatus.Text = list.Count == 0 ? "조회 결과가 없습니다." : $"레시피 {list.Count}건";
        if (selectId != null)
        {
            foreach (DataGridViewRow row in dgvRecipe.Rows)
            {
                if (row.DataBoundItem is TbRmsRecipe r && r.RecipeId == selectId)
                {
                    dgvRecipe.CurrentCell = row.Cells[0];
                    break;
                }
            }
        }
        else if (list.Count == 0)
        {
            SetNewMode();
        }
    }

    /// Master 선택 → 편집 영역 + Detail(파라미터) 표시
    private async void dgvRecipe_SelectionChanged(object sender, EventArgs e)
    {
        if (dgvRecipe.CurrentRow?.DataBoundItem is not TbRmsRecipe r) return;
        _isNew = false;
        lblMode.Text = "[수정]";
        txtRecipeId.Text = r.RecipeId;
        txtRecipeId.ReadOnly = true;
        if (!cboEquipId.Items.Contains(r.EquipId)) cboEquipId.Items.Add(r.EquipId);
        cboEquipId.SelectedItem = r.EquipId;
        txtFactorId.Text = r.FactorId;
        txtFactorValue.Text = r.FactorValue;
        cboUseYn.SelectedItem = r.UseYn;
        grpParameter.Text = $"선택한 레시피의 파라미터 (Detail) — {r.RecipeId}";

        int version = ++_detailVersion;
        try
        {
            var rows = await _service.GetParametersAsync(r.RecipeId);
            if (version != _detailVersion) return;          // 그 사이 다른 행을 고른 경우 버림
            BindParameters(rows);
        }
        catch (Exception ex)
        {
            MessageBox.Show("파라미터 조회 실패\n" + ex.GetBaseException().Message);
        }
    }

    private void BindParameters(IEnumerable<ParameterRow> rows)
    {
        _parameters = new BindingList<ParameterRow>(rows.ToList());
        dgvParameter.DataSource = _parameters;
    }

    private void btnNew_Click(object sender, EventArgs e)
    {
        dgvRecipe.ClearSelection();
        SetNewMode();
        txtRecipeId.Focus();
    }

    private void SetNewMode()
    {
        _isNew = true;
        _detailVersion++;
        lblMode.Text = "[신규]";
        txtRecipeId.ReadOnly = false;
        txtRecipeId.Clear();
        if (cboSearchEquipId.Text != All && cboEquipId.Items.Contains(cboSearchEquipId.Text)) cboEquipId.SelectedItem = cboSearchEquipId.Text;
        else cboEquipId.SelectedIndex = -1;
        txtFactorId.Clear();
        txtFactorValue.Clear();
        cboUseYn.SelectedItem = "Y";
        grpParameter.Text = "선택한 레시피의 파라미터 (Detail) — 신규";
        BindParameters([]);
    }

    private void btnAddParam_Click(object sender, EventArgs e)
    {
        _parameters.Add(new ParameterRow());
        dgvParameter.CurrentCell = dgvParameter.Rows[^1].Cells[0];
        dgvParameter.BeginEdit(true);
    }

    private void btnRemoveParam_Click(object sender, EventArgs e)
    {
        if (dgvParameter.CurrentRow?.DataBoundItem is ParameterRow row) _parameters.Remove(row);
    }

    private async void btnSave_Click(object sender, EventArgs e)
    {
        dgvParameter.EndEdit();                              // 편집 중인 셀 값 반영
        var input = new RecipeInput(txtRecipeId.Text, cboEquipId.Text, txtFactorId.Text, txtFactorValue.Text, cboUseYn.Text);
        var rows = _parameters.ToList();

        await RunAsync(async () =>
        {
            if (_isNew) await _service.CreateAsync(input, rows);
            else await _service.UpdateAsync(input, rows);
            await LoadListAsync(input.RecipeId.Trim());
            lblStatus.Text = $"레시피 '{input.RecipeId.Trim()}'와 파라미터 {rows.Count(r => !string.IsNullOrWhiteSpace(r.ParameterId))}건 저장 완료";
        });
    }

    private async void btnDelete_Click(object sender, EventArgs e)
    {
        if (_isNew || string.IsNullOrEmpty(txtRecipeId.Text))
        {
            MessageBox.Show("삭제할 레시피를 목록에서 선택하세요.");
            return;
        }
        string id = txtRecipeId.Text;
        if (MessageBox.Show($"레시피 '{id}'와 파라미터 {_parameters.Count}건을 삭제할까요?", "삭제 확인",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

        await RunAsync(async () =>
        {
            await _service.DeleteAsync(id);
            SetNewMode();
            await LoadListAsync();
            lblStatus.Text = $"레시피 '{id}' 삭제 완료";
        });
    }

    private async Task RunAsync(Func<Task> action)
    {
        if (_busy) return;
        _busy = true;
        SetButtons(false);
        try
        {
            await action();
        }
        catch (BusinessException ex)
        {
            MessageBox.Show(ex.Message, "확인", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show("처리 중 오류가 발생했습니다.\n" + ex.GetBaseException().Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            SetButtons(true);
            _busy = false;
        }
    }

    private void SetButtons(bool enabled)
    {
        btnSearch.Enabled = btnNew.Enabled = btnSave.Enabled = btnDelete.Enabled = btnAddParam.Enabled = btnRemoveParam.Enabled = enabled;
    }
}
