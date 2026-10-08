namespace RmsMasterData.Controls;

partial class RecipeControl
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

    #region 구성 요소 디자이너에서 생성한 코드

    private void InitializeComponent()
    {
        pnlCondition = new Panel();
        btnDelete = new Button();
        btnSave = new Button();
        btnNew = new Button();
        btnSearch = new Button();
        txtSearchRecipeId = new TextBox();
        lblSearchRecipeId = new Label();
        cboSearchEquipId = new ComboBox();
        lblSearchEquipId = new Label();
        dgvRecipe = new DataGridView();
        colRecipeId = new DataGridViewTextBoxColumn();
        colEquipId = new DataGridViewTextBoxColumn();
        colFactorId = new DataGridViewTextBoxColumn();
        colFactorValue = new DataGridViewTextBoxColumn();
        colUseYn = new DataGridViewTextBoxColumn();
        pnlDetail = new Panel();
        grpParameter = new GroupBox();
        dgvParameter = new DataGridView();
        colParameterId = new DataGridViewTextBoxColumn();
        colParameterValue = new DataGridViewTextBoxColumn();
        pnlParamButtons = new Panel();
        btnRemoveParam = new Button();
        btnAddParam = new Button();
        grpRecipe = new GroupBox();
        lblMode = new Label();
        cboUseYn = new ComboBox();
        lblUseYn = new Label();
        txtFactorValue = new TextBox();
        lblFactorValue = new Label();
        txtFactorId = new TextBox();
        lblFactorId = new Label();
        cboEquipId = new ComboBox();
        lblEquipId = new Label();
        txtRecipeId = new TextBox();
        lblRecipeId = new Label();
        lblStatus = new Label();
        pnlCondition.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvRecipe).BeginInit();
        pnlDetail.SuspendLayout();
        grpParameter.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvParameter).BeginInit();
        pnlParamButtons.SuspendLayout();
        grpRecipe.SuspendLayout();
        SuspendLayout();
        //
        // pnlCondition
        //
        pnlCondition.Controls.Add(btnDelete);
        pnlCondition.Controls.Add(btnSave);
        pnlCondition.Controls.Add(btnNew);
        pnlCondition.Controls.Add(btnSearch);
        pnlCondition.Controls.Add(txtSearchRecipeId);
        pnlCondition.Controls.Add(lblSearchRecipeId);
        pnlCondition.Controls.Add(cboSearchEquipId);
        pnlCondition.Controls.Add(lblSearchEquipId);
        pnlCondition.Dock = DockStyle.Top;
        pnlCondition.Location = new Point(0, 0);
        pnlCondition.Name = "pnlCondition";
        pnlCondition.Size = new Size(1100, 50);
        pnlCondition.TabIndex = 0;
        //
        // lblSearchEquipId
        //
        lblSearchEquipId.AutoSize = true;
        lblSearchEquipId.Location = new Point(10, 17);
        lblSearchEquipId.Name = "lblSearchEquipId";
        lblSearchEquipId.Size = new Size(43, 15);
        lblSearchEquipId.TabIndex = 0;
        lblSearchEquipId.Text = "설비 ID";
        //
        // cboSearchEquipId
        //
        cboSearchEquipId.DropDownStyle = ComboBoxStyle.DropDownList;
        cboSearchEquipId.Location = new Point(62, 13);
        cboSearchEquipId.Name = "cboSearchEquipId";
        cboSearchEquipId.Size = new Size(140, 23);
        cboSearchEquipId.TabIndex = 1;
        //
        // lblSearchRecipeId
        //
        lblSearchRecipeId.AutoSize = true;
        lblSearchRecipeId.Location = new Point(218, 17);
        lblSearchRecipeId.Name = "lblSearchRecipeId";
        lblSearchRecipeId.Size = new Size(55, 15);
        lblSearchRecipeId.TabIndex = 2;
        lblSearchRecipeId.Text = "레시피 ID";
        //
        // txtSearchRecipeId
        //
        txtSearchRecipeId.Location = new Point(280, 13);
        txtSearchRecipeId.Name = "txtSearchRecipeId";
        txtSearchRecipeId.Size = new Size(150, 23);
        txtSearchRecipeId.TabIndex = 3;
        txtSearchRecipeId.KeyDown += txtSearchRecipeId_KeyDown;
        //
        // btnSearch
        //
        btnSearch.Location = new Point(446, 10);
        btnSearch.Name = "btnSearch";
        btnSearch.Size = new Size(80, 30);
        btnSearch.TabIndex = 4;
        btnSearch.Text = "조회";
        btnSearch.UseVisualStyleBackColor = true;
        btnSearch.Click += btnSearch_Click;
        //
        // btnNew
        //
        btnNew.Location = new Point(532, 10);
        btnNew.Name = "btnNew";
        btnNew.Size = new Size(80, 30);
        btnNew.TabIndex = 5;
        btnNew.Text = "신규";
        btnNew.UseVisualStyleBackColor = true;
        btnNew.Click += btnNew_Click;
        //
        // btnSave
        //
        btnSave.Location = new Point(618, 10);
        btnSave.Name = "btnSave";
        btnSave.Size = new Size(80, 30);
        btnSave.TabIndex = 6;
        btnSave.Text = "저장";
        btnSave.UseVisualStyleBackColor = true;
        btnSave.Click += btnSave_Click;
        //
        // btnDelete
        //
        btnDelete.Location = new Point(704, 10);
        btnDelete.Name = "btnDelete";
        btnDelete.Size = new Size(80, 30);
        btnDelete.TabIndex = 7;
        btnDelete.Text = "삭제";
        btnDelete.UseVisualStyleBackColor = true;
        btnDelete.Click += btnDelete_Click;
        //
        // dgvRecipe
        //
        dgvRecipe.AllowUserToAddRows = false;
        dgvRecipe.AllowUserToDeleteRows = false;
        dgvRecipe.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvRecipe.Columns.AddRange(new DataGridViewColumn[] { colRecipeId, colEquipId, colFactorId, colFactorValue, colUseYn });
        dgvRecipe.Dock = DockStyle.Fill;
        dgvRecipe.Location = new Point(0, 50);
        dgvRecipe.MultiSelect = false;
        dgvRecipe.Name = "dgvRecipe";
        dgvRecipe.ReadOnly = true;
        dgvRecipe.RowHeadersVisible = false;
        dgvRecipe.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvRecipe.Size = new Size(1100, 266);
        dgvRecipe.TabIndex = 1;
        dgvRecipe.SelectionChanged += dgvRecipe_SelectionChanged;
        //
        // colRecipeId
        //
        colRecipeId.DataPropertyName = "RecipeId";
        colRecipeId.HeaderText = "레시피 ID";
        colRecipeId.Name = "colRecipeId";
        colRecipeId.ReadOnly = true;
        //
        // colEquipId
        //
        colEquipId.DataPropertyName = "EquipId";
        colEquipId.HeaderText = "설비 ID";
        colEquipId.Name = "colEquipId";
        colEquipId.ReadOnly = true;
        //
        // colFactorId
        //
        colFactorId.DataPropertyName = "FactorId";
        colFactorId.HeaderText = "팩터 ID";
        colFactorId.Name = "colFactorId";
        colFactorId.ReadOnly = true;
        //
        // colFactorValue
        //
        colFactorValue.DataPropertyName = "FactorValue";
        colFactorValue.HeaderText = "팩터 값";
        colFactorValue.Name = "colFactorValue";
        colFactorValue.ReadOnly = true;
        //
        // colUseYn
        //
        colUseYn.DataPropertyName = "UseYn";
        colUseYn.HeaderText = "사용 여부";
        colUseYn.Name = "colUseYn";
        colUseYn.ReadOnly = true;
        //
        // pnlDetail
        //
        pnlDetail.Controls.Add(grpParameter);
        pnlDetail.Controls.Add(grpRecipe);
        pnlDetail.Dock = DockStyle.Bottom;
        pnlDetail.Location = new Point(0, 316);
        pnlDetail.Name = "pnlDetail";
        pnlDetail.Size = new Size(1100, 260);
        pnlDetail.TabIndex = 2;
        //
        // grpRecipe
        //
        grpRecipe.Controls.Add(lblMode);
        grpRecipe.Controls.Add(cboUseYn);
        grpRecipe.Controls.Add(lblUseYn);
        grpRecipe.Controls.Add(txtFactorValue);
        grpRecipe.Controls.Add(lblFactorValue);
        grpRecipe.Controls.Add(txtFactorId);
        grpRecipe.Controls.Add(lblFactorId);
        grpRecipe.Controls.Add(cboEquipId);
        grpRecipe.Controls.Add(lblEquipId);
        grpRecipe.Controls.Add(txtRecipeId);
        grpRecipe.Controls.Add(lblRecipeId);
        grpRecipe.Dock = DockStyle.Left;
        grpRecipe.Location = new Point(0, 0);
        grpRecipe.Name = "grpRecipe";
        grpRecipe.Size = new Size(330, 260);
        grpRecipe.TabIndex = 0;
        grpRecipe.TabStop = false;
        grpRecipe.Text = "레시피 (Master)";
        //
        // lblMode
        //
        lblMode.AutoSize = true;
        lblMode.Font = new Font("맑은 고딕", 9F, FontStyle.Bold);
        lblMode.Location = new Point(14, 26);
        lblMode.Name = "lblMode";
        lblMode.Size = new Size(43, 15);
        lblMode.TabIndex = 0;
        lblMode.Text = "[신규]";
        //
        // lblRecipeId
        //
        lblRecipeId.AutoSize = true;
        lblRecipeId.Location = new Point(14, 56);
        lblRecipeId.Name = "lblRecipeId";
        lblRecipeId.Size = new Size(55, 15);
        lblRecipeId.TabIndex = 1;
        lblRecipeId.Text = "레시피 ID";
        //
        // txtRecipeId
        //
        txtRecipeId.Location = new Point(100, 52);
        txtRecipeId.MaxLength = 50;
        txtRecipeId.Name = "txtRecipeId";
        txtRecipeId.Size = new Size(210, 23);
        txtRecipeId.TabIndex = 2;
        //
        // lblEquipId
        //
        lblEquipId.AutoSize = true;
        lblEquipId.Location = new Point(14, 90);
        lblEquipId.Name = "lblEquipId";
        lblEquipId.Size = new Size(43, 15);
        lblEquipId.TabIndex = 3;
        lblEquipId.Text = "설비 ID";
        //
        // cboEquipId
        //
        cboEquipId.DropDownStyle = ComboBoxStyle.DropDownList;
        cboEquipId.Location = new Point(100, 86);
        cboEquipId.Name = "cboEquipId";
        cboEquipId.Size = new Size(210, 23);
        cboEquipId.TabIndex = 4;
        //
        // lblFactorId
        //
        lblFactorId.AutoSize = true;
        lblFactorId.Location = new Point(14, 124);
        lblFactorId.Name = "lblFactorId";
        lblFactorId.Size = new Size(43, 15);
        lblFactorId.TabIndex = 5;
        lblFactorId.Text = "팩터 ID";
        //
        // txtFactorId
        //
        txtFactorId.Location = new Point(100, 120);
        txtFactorId.MaxLength = 50;
        txtFactorId.Name = "txtFactorId";
        txtFactorId.Size = new Size(210, 23);
        txtFactorId.TabIndex = 6;
        //
        // lblFactorValue
        //
        lblFactorValue.AutoSize = true;
        lblFactorValue.Location = new Point(14, 158);
        lblFactorValue.Name = "lblFactorValue";
        lblFactorValue.Size = new Size(43, 15);
        lblFactorValue.TabIndex = 7;
        lblFactorValue.Text = "팩터 값";
        //
        // txtFactorValue
        //
        txtFactorValue.Location = new Point(100, 154);
        txtFactorValue.MaxLength = 200;
        txtFactorValue.Name = "txtFactorValue";
        txtFactorValue.Size = new Size(210, 23);
        txtFactorValue.TabIndex = 8;
        //
        // lblUseYn
        //
        lblUseYn.AutoSize = true;
        lblUseYn.Location = new Point(14, 192);
        lblUseYn.Name = "lblUseYn";
        lblUseYn.Size = new Size(55, 15);
        lblUseYn.TabIndex = 9;
        lblUseYn.Text = "사용 여부";
        //
        // cboUseYn
        //
        cboUseYn.DropDownStyle = ComboBoxStyle.DropDownList;
        cboUseYn.Items.AddRange(new object[] { "Y", "N" });
        cboUseYn.Location = new Point(100, 188);
        cboUseYn.Name = "cboUseYn";
        cboUseYn.Size = new Size(80, 23);
        cboUseYn.TabIndex = 10;
        //
        // grpParameter
        //
        grpParameter.Controls.Add(dgvParameter);
        grpParameter.Controls.Add(pnlParamButtons);
        grpParameter.Dock = DockStyle.Fill;
        grpParameter.Location = new Point(330, 0);
        grpParameter.Name = "grpParameter";
        grpParameter.Size = new Size(770, 260);
        grpParameter.TabIndex = 1;
        grpParameter.TabStop = false;
        grpParameter.Text = "선택한 레시피의 파라미터 (Detail)";
        //
        // dgvParameter
        //
        dgvParameter.AllowUserToAddRows = false;
        dgvParameter.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvParameter.Columns.AddRange(new DataGridViewColumn[] { colParameterId, colParameterValue });
        dgvParameter.Dock = DockStyle.Fill;
        dgvParameter.Location = new Point(3, 19);
        dgvParameter.Name = "dgvParameter";
        dgvParameter.RowHeadersWidth = 24;
        dgvParameter.Size = new Size(664, 238);
        dgvParameter.TabIndex = 0;
        //
        // colParameterId
        //
        colParameterId.DataPropertyName = "ParameterId";
        colParameterId.HeaderText = "파라미터 ID";
        colParameterId.MaxInputLength = 50;
        colParameterId.Name = "colParameterId";
        //
        // colParameterValue
        //
        colParameterValue.DataPropertyName = "ParameterValue";
        colParameterValue.HeaderText = "파라미터 값";
        colParameterValue.MaxInputLength = 500;
        colParameterValue.Name = "colParameterValue";
        //
        // pnlParamButtons
        //
        pnlParamButtons.Controls.Add(btnRemoveParam);
        pnlParamButtons.Controls.Add(btnAddParam);
        pnlParamButtons.Dock = DockStyle.Right;
        pnlParamButtons.Location = new Point(667, 19);
        pnlParamButtons.Name = "pnlParamButtons";
        pnlParamButtons.Size = new Size(100, 238);
        pnlParamButtons.TabIndex = 1;
        //
        // btnAddParam
        //
        btnAddParam.Location = new Point(8, 6);
        btnAddParam.Name = "btnAddParam";
        btnAddParam.Size = new Size(86, 30);
        btnAddParam.TabIndex = 0;
        btnAddParam.Text = "행 추가";
        btnAddParam.UseVisualStyleBackColor = true;
        btnAddParam.Click += btnAddParam_Click;
        //
        // btnRemoveParam
        //
        btnRemoveParam.Location = new Point(8, 42);
        btnRemoveParam.Name = "btnRemoveParam";
        btnRemoveParam.Size = new Size(86, 30);
        btnRemoveParam.TabIndex = 1;
        btnRemoveParam.Text = "행 삭제";
        btnRemoveParam.UseVisualStyleBackColor = true;
        btnRemoveParam.Click += btnRemoveParam_Click;
        //
        // lblStatus
        //
        lblStatus.Dock = DockStyle.Bottom;
        lblStatus.Location = new Point(0, 576);
        lblStatus.Name = "lblStatus";
        lblStatus.Padding = new Padding(6, 0, 0, 0);
        lblStatus.Size = new Size(1100, 24);
        lblStatus.TabIndex = 3;
        lblStatus.TextAlign = ContentAlignment.MiddleLeft;
        //
        // RecipeControl
        //
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        Controls.Add(dgvRecipe);
        Controls.Add(pnlDetail);
        Controls.Add(pnlCondition);
        Controls.Add(lblStatus);
        Name = "RecipeControl";
        Size = new Size(1100, 600);
        Load += RecipeControl_Load;
        pnlCondition.ResumeLayout(false);
        pnlCondition.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)dgvRecipe).EndInit();
        pnlDetail.ResumeLayout(false);
        grpParameter.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)dgvParameter).EndInit();
        pnlParamButtons.ResumeLayout(false);
        grpRecipe.ResumeLayout(false);
        grpRecipe.PerformLayout();
        ResumeLayout(false);
    }

    #endregion

    private Panel pnlCondition;
    private Label lblSearchEquipId;
    private ComboBox cboSearchEquipId;
    private Label lblSearchRecipeId;
    private TextBox txtSearchRecipeId;
    private Button btnSearch;
    private Button btnNew;
    private Button btnSave;
    private Button btnDelete;
    private DataGridView dgvRecipe;
    private DataGridViewTextBoxColumn colRecipeId;
    private DataGridViewTextBoxColumn colEquipId;
    private DataGridViewTextBoxColumn colFactorId;
    private DataGridViewTextBoxColumn colFactorValue;
    private DataGridViewTextBoxColumn colUseYn;
    private Panel pnlDetail;
    private GroupBox grpRecipe;
    private Label lblMode;
    private Label lblRecipeId;
    private TextBox txtRecipeId;
    private Label lblEquipId;
    private ComboBox cboEquipId;
    private Label lblFactorId;
    private TextBox txtFactorId;
    private Label lblFactorValue;
    private TextBox txtFactorValue;
    private Label lblUseYn;
    private ComboBox cboUseYn;
    private GroupBox grpParameter;
    private DataGridView dgvParameter;
    private DataGridViewTextBoxColumn colParameterId;
    private DataGridViewTextBoxColumn colParameterValue;
    private Panel pnlParamButtons;
    private Button btnAddParam;
    private Button btnRemoveParam;
    private Label lblStatus;
}
