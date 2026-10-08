namespace RmsMasterData;

/// <summary>
/// RMS 기준정보 관리 — 필수 화면 2개 (기준정보관리 생성.pptx p.2)
///   탭 1: 설비 기준정보 화면 (EquipmentControl)
///   탭 2: 레시피 Master–Detail 화면 (RecipeControl)
/// </summary>
public partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();
    }

    /// 레시피 탭으로 넘어갈 때 설비 목록을 다시 읽는다 (설비 화면에서 등록·삭제한 내용 반영)
    private async void tabMain_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (tabMain.SelectedTab == tabRecipe) await recipeControl.RefreshEquipmentListAsync();
    }
}
