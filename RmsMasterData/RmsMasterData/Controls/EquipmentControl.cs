using RmsMasterData.Common;
using RmsMasterData.Data.Entities;
using RmsMasterData.Services;

namespace RmsMasterData.Controls;

/// <summary>
/// 설비 기준정보 화면 (기준 p.4) — 조회 · 신규 · 저장 · 삭제
/// 화면은 입력과 표시만 하고, 검증·저장은 EquipmentService가 한다.
/// </summary>
public partial class EquipmentControl : UserControl
{
    private readonly EquipmentService _service = new();
    private bool _isNew = true;
    private bool _busy;

    public EquipmentControl()
    {
        InitializeComponent();
        dgvEquipment.AutoGenerateColumns = false;
    }

    private async void EquipmentControl_Load(object sender, EventArgs e)
    {
        if (DesignMode) return;
        SetNewMode();
        await SearchAsync();
    }

    private async void btnSearch_Click(object sender, EventArgs e) => await SearchAsync();

    private void txtSearch_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Enter) btnSearch.PerformClick();
    }

    private Task SearchAsync() => RunAsync(() => LoadListAsync());

    /// 목록 다시 읽기 (저장·삭제 뒤에도 호출 — RunAsync 안에서 쓰므로 잠금 없이)
    private async Task LoadListAsync(string? selectId = null)
    {
        var list = await _service.SearchAsync(txtSearchEquipId.Text, txtSearchEquipName.Text);
        dgvEquipment.DataSource = list;
        lblStatus.Text = list.Count == 0 ? "조회 결과가 없습니다." : $"설비 {list.Count}건";
        if (selectId != null) SelectRow(selectId);
        else if (list.Count == 0) SetNewMode();
    }

    private void SelectRow(string equipId)
    {
        foreach (DataGridViewRow row in dgvEquipment.Rows)
        {
            if (row.DataBoundItem is TbEquipment eq && eq.EquipId == equipId)
            {
                row.Selected = true;
                dgvEquipment.CurrentCell = row.Cells[0];
                return;
            }
        }
    }

    /// 그리드에서 고른 설비를 편집 영역에 표시 (설비 ID는 PK라 수정 불가)
    private void dgvEquipment_SelectionChanged(object sender, EventArgs e)
    {
        if (dgvEquipment.CurrentRow?.DataBoundItem is not TbEquipment eq) return;
        _isNew = false;
        lblMode.Text = "[수정]";
        txtEquipId.Text = eq.EquipId;
        txtEquipId.ReadOnly = true;
        txtEquipName.Text = eq.EquipName;
        cboUseYn.SelectedItem = eq.UseYn;
    }

    private void btnNew_Click(object sender, EventArgs e)
    {
        dgvEquipment.ClearSelection();
        SetNewMode();
        txtEquipId.Focus();
    }

    private void SetNewMode()
    {
        _isNew = true;
        lblMode.Text = "[신규]";
        txtEquipId.ReadOnly = false;
        txtEquipId.Clear();
        txtEquipName.Clear();
        cboUseYn.SelectedItem = "Y";
    }

    private async void btnSave_Click(object sender, EventArgs e)
    {
        string id = txtEquipId.Text.Trim();
        await RunAsync(async () =>
        {
            if (_isNew) await _service.CreateAsync(id, txtEquipName.Text, cboUseYn.Text);
            else await _service.UpdateAsync(id, txtEquipName.Text, cboUseYn.Text);
            await LoadListAsync(id);
            lblStatus.Text = $"설비 '{id}' 저장 완료";
        });
    }

    private async void btnDelete_Click(object sender, EventArgs e)
    {
        if (_isNew || string.IsNullOrEmpty(txtEquipId.Text))
        {
            MessageBox.Show("삭제할 설비를 목록에서 선택하세요.");
            return;
        }
        string id = txtEquipId.Text;
        if (MessageBox.Show($"설비 '{id}'를 삭제할까요?", "삭제 확인", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
            return;

        await RunAsync(async () =>
        {
            await _service.DeleteAsync(id);
            SetNewMode();
            await LoadListAsync();
            lblStatus.Text = $"설비 '{id}' 삭제 완료";
        });
    }

    /// 처리 중 버튼 잠금 + 오류 표시 (업무 규칙 위반은 안내, 그 밖의 오류는 오류로)
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
        btnSearch.Enabled = btnNew.Enabled = btnSave.Enabled = btnDelete.Enabled = enabled;
    }
}
