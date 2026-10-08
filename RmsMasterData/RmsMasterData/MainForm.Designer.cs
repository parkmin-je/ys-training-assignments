namespace RmsMasterData;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form 디자이너에서 생성한 코드

    private void InitializeComponent()
    {
        tabMain = new TabControl();
        tabEquipment = new TabPage();
        equipmentControl = new Controls.EquipmentControl();
        tabRecipe = new TabPage();
        recipeControl = new Controls.RecipeControl();
        tabMain.SuspendLayout();
        tabEquipment.SuspendLayout();
        tabRecipe.SuspendLayout();
        SuspendLayout();
        //
        // tabMain
        //
        tabMain.Controls.Add(tabEquipment);
        tabMain.Controls.Add(tabRecipe);
        tabMain.Dock = DockStyle.Fill;
        tabMain.Location = new Point(0, 0);
        tabMain.Name = "tabMain";
        tabMain.SelectedIndex = 0;
        tabMain.Size = new Size(1184, 721);
        tabMain.TabIndex = 0;
        tabMain.SelectedIndexChanged += tabMain_SelectedIndexChanged;
        //
        // tabEquipment
        //
        tabEquipment.Controls.Add(equipmentControl);
        tabEquipment.Location = new Point(4, 24);
        tabEquipment.Name = "tabEquipment";
        tabEquipment.Padding = new Padding(3);
        tabEquipment.Size = new Size(1176, 693);
        tabEquipment.TabIndex = 0;
        tabEquipment.Text = "설비 기준정보 관리";
        tabEquipment.UseVisualStyleBackColor = true;
        //
        // equipmentControl
        //
        equipmentControl.Dock = DockStyle.Fill;
        equipmentControl.Location = new Point(3, 3);
        equipmentControl.Name = "equipmentControl";
        equipmentControl.Size = new Size(1170, 687);
        equipmentControl.TabIndex = 0;
        //
        // tabRecipe
        //
        tabRecipe.Controls.Add(recipeControl);
        tabRecipe.Location = new Point(4, 24);
        tabRecipe.Name = "tabRecipe";
        tabRecipe.Padding = new Padding(3);
        tabRecipe.Size = new Size(1176, 693);
        tabRecipe.TabIndex = 1;
        tabRecipe.Text = "레시피 기준정보 관리";
        tabRecipe.UseVisualStyleBackColor = true;
        //
        // recipeControl
        //
        recipeControl.Dock = DockStyle.Fill;
        recipeControl.Location = new Point(3, 3);
        recipeControl.Name = "recipeControl";
        recipeControl.Size = new Size(1170, 687);
        recipeControl.TabIndex = 0;
        //
        // MainForm
        //
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1184, 721);
        Controls.Add(tabMain);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "RMS 기준정보 관리";
        tabMain.ResumeLayout(false);
        tabEquipment.ResumeLayout(false);
        tabRecipe.ResumeLayout(false);
        ResumeLayout(false);
    }

    #endregion

    private TabControl tabMain;
    private TabPage tabEquipment;
    private TabPage tabRecipe;
    private Controls.EquipmentControl equipmentControl;
    private Controls.RecipeControl recipeControl;
}
