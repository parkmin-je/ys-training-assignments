namespace RmsMasterData.Controls;

partial class EquipmentControl
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
        txtSearchEquipName = new TextBox();
        lblSearchEquipName = new Label();
        txtSearchEquipId = new TextBox();
        lblSearchEquipId = new Label();
        grpEdit = new GroupBox();
        lblMode = new Label();
        cboUseYn = new ComboBox();
        lblUseYn = new Label();
        txtEquipName = new TextBox();
        lblEquipName = new Label();
        txtEquipId = new TextBox();
        lblEquipId = new Label();
        dgvEquipment = new DataGridView();
        colEquipId = new DataGridViewTextBoxColumn();
        colEquipName = new DataGridViewTextBoxColumn();
        colUseYn = new DataGridViewTextBoxColumn();
        colUpdatedAt = new DataGridViewTextBoxColumn();
        lblStatus = new Label();
        pnlCondition.SuspendLayout();
        grpEdit.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvEquipment).BeginInit();
        SuspendLayout();
        //
        // pnlCondition
        //
        pnlCondition.Controls.Add(btnDelete);
        pnlCondition.Controls.Add(btnSave);
        pnlCondition.Controls.Add(btnNew);
        pnlCondition.Controls.Add(btnSearch);
        pnlCondition.Controls.Add(txtSearchEquipName);
        pnlCondition.Controls.Add(lblSearchEquipName);
        pnlCondition.Controls.Add(txtSearchEquipId);
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
        // txtSearchEquipId
        //
        txtSearchEquipId.Location = new Point(62, 13);
        txtSearchEquipId.Name = "txtSearchEquipId";
        txtSearchEquipId.Size = new Size(140, 23);
        txtSearchEquipId.TabIndex = 1;
        txtSearchEquipId.KeyDown += txtSearch_KeyDown;
        //
        // lblSearchEquipName
        //
        lblSearchEquipName.AutoSize = true;
        lblSearchEquipName.Location = new Point(218, 17);
        lblSearchEquipName.Name = "lblSearchEquipName";
        lblSearchEquipName.Size = new Size(43, 15);
        lblSearchEquipName.TabIndex = 2;
        lblSearchEquipName.Text = "설비명";
        //
        // txtSearchEquipName
        //
        txtSearchEquipName.Location = new Point(268, 13);
        txtSearchEquipName.Name = "txtSearchEquipName";
        txtSearchEquipName.Size = new Size(160, 23);
        txtSearchEquipName.TabIndex = 3;
        txtSearchEquipName.KeyDown += txtSearch_KeyDown;
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
        // grpEdit
        //
        grpEdit.Controls.Add(lblMode);
        grpEdit.Controls.Add(cboUseYn);
        grpEdit.Controls.Add(lblUseYn);
        grpEdit.Controls.Add(txtEquipName);
        grpEdit.Controls.Add(lblEquipName);
        grpEdit.Controls.Add(txtEquipId);
        grpEdit.Controls.Add(lblEquipId);
        grpEdit.Dock = DockStyle.Right;
        grpEdit.Location = new Point(800, 50);
        grpEdit.Name = "grpEdit";
        grpEdit.Size = new Size(300, 526);
        grpEdit.TabIndex = 2;
        grpEdit.TabStop = false;
        grpEdit.Text = "설비 정보";
        //
        // lblMode
        //
        lblMode.AutoSize = true;
        lblMode.Font = new Font("맑은 고딕", 9F, FontStyle.Bold);
        lblMode.Location = new Point(14, 28);
        lblMode.Name = "lblMode";
        lblMode.Size = new Size(55, 15);
        lblMode.TabIndex = 0;
        lblMode.Text = "[신규]";
        //
        // lblEquipId
        //
        lblEquipId.AutoSize = true;
        lblEquipId.Location = new Point(14, 60);
        lblEquipId.Name = "lblEquipId";
        lblEquipId.Size = new Size(43, 15);
        lblEquipId.TabIndex = 1;
        lblEquipId.Text = "설비 ID";
        //
        // txtEquipId
        //
        txtEquipId.Location = new Point(90, 56);
        txtEquipId.MaxLength = 50;
        txtEquipId.Name = "txtEquipId";
        txtEquipId.Size = new Size(190, 23);
        txtEquipId.TabIndex = 2;
        //
        // lblEquipName
        //
        lblEquipName.AutoSize = true;
        lblEquipName.Location = new Point(14, 94);
        lblEquipName.Name = "lblEquipName";
        lblEquipName.Size = new Size(43, 15);
        lblEquipName.TabIndex = 3;
        lblEquipName.Text = "설비명";
        //
        // txtEquipName
        //
        txtEquipName.Location = new Point(90, 90);
        txtEquipName.MaxLength = 100;
        txtEquipName.Name = "txtEquipName";
        txtEquipName.Size = new Size(190, 23);
        txtEquipName.TabIndex = 4;
        //
        // lblUseYn
        //
        lblUseYn.AutoSize = true;
        lblUseYn.Location = new Point(14, 128);
        lblUseYn.Name = "lblUseYn";
        lblUseYn.Size = new Size(55, 15);
        lblUseYn.TabIndex = 5;
        lblUseYn.Text = "사용 여부";
        //
        // cboUseYn
        //
        cboUseYn.DropDownStyle = ComboBoxStyle.DropDownList;
        cboUseYn.Items.AddRange(new object[] { "Y", "N" });
        cboUseYn.Location = new Point(90, 124);
        cboUseYn.Name = "cboUseYn";
        cboUseYn.Size = new Size(80, 23);
        cboUseYn.TabIndex = 6;
        //
        // dgvEquipment
        //
        dgvEquipment.AllowUserToAddRows = false;
        dgvEquipment.AllowUserToDeleteRows = false;
        dgvEquipment.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvEquipment.Columns.AddRange(new DataGridViewColumn[] { colEquipId, colEquipName, colUseYn, colUpdatedAt });
        dgvEquipment.Dock = DockStyle.Fill;
        dgvEquipment.Location = new Point(0, 50);
        dgvEquipment.MultiSelect = false;
        dgvEquipment.Name = "dgvEquipment";
        dgvEquipment.ReadOnly = true;
        dgvEquipment.RowHeadersVisible = false;
        dgvEquipment.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvEquipment.Size = new Size(800, 526);
        dgvEquipment.TabIndex = 1;
        dgvEquipment.SelectionChanged += dgvEquipment_SelectionChanged;
        //
        // colEquipId
        //
        colEquipId.DataPropertyName = "EquipId";
        colEquipId.HeaderText = "설비 ID";
        colEquipId.Name = "colEquipId";
        colEquipId.ReadOnly = true;
        //
        // colEquipName
        //
        colEquipName.DataPropertyName = "EquipName";
        colEquipName.HeaderText = "설비명";
        colEquipName.Name = "colEquipName";
        colEquipName.ReadOnly = true;
        //
        // colUseYn
        //
        colUseYn.DataPropertyName = "UseYn";
        colUseYn.HeaderText = "사용 여부";
        colUseYn.Name = "colUseYn";
        colUseYn.ReadOnly = true;
        //
        // colUpdatedAt
        //
        colUpdatedAt.DataPropertyName = "UpdatedAt";
        colUpdatedAt.DefaultCellStyle.Format = "yyyy-MM-dd HH:mm:ss";
        colUpdatedAt.HeaderText = "수정 일시";
        colUpdatedAt.Name = "colUpdatedAt";
        colUpdatedAt.ReadOnly = true;
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
        // EquipmentControl
        //
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        Controls.Add(dgvEquipment);
        Controls.Add(grpEdit);
        Controls.Add(pnlCondition);
        Controls.Add(lblStatus);
        Name = "EquipmentControl";
        Size = new Size(1100, 600);
        Load += EquipmentControl_Load;
        pnlCondition.ResumeLayout(false);
        pnlCondition.PerformLayout();
        grpEdit.ResumeLayout(false);
        grpEdit.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)dgvEquipment).EndInit();
        ResumeLayout(false);
    }

    #endregion

    private Panel pnlCondition;
    private Label lblSearchEquipId;
    private TextBox txtSearchEquipId;
    private Label lblSearchEquipName;
    private TextBox txtSearchEquipName;
    private Button btnSearch;
    private Button btnNew;
    private Button btnSave;
    private Button btnDelete;
    private GroupBox grpEdit;
    private Label lblMode;
    private Label lblEquipId;
    private TextBox txtEquipId;
    private Label lblEquipName;
    private TextBox txtEquipName;
    private Label lblUseYn;
    private ComboBox cboUseYn;
    private DataGridView dgvEquipment;
    private DataGridViewTextBoxColumn colEquipId;
    private DataGridViewTextBoxColumn colEquipName;
    private DataGridViewTextBoxColumn colUseYn;
    private DataGridViewTextBoxColumn colUpdatedAt;
    private Label lblStatus;
}
