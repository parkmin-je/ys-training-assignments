namespace RmsResultViewer;

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
        pnlCondition = new Panel();
        btnReset = new Button();
        btnSearch = new Button();
        txtRecipeId = new TextBox();
        lblRecipeId = new Label();
        txtEquipId = new TextBox();
        lblEquipId = new Label();
        dtpTo = new DateTimePicker();
        lblTilde = new Label();
        dtpFrom = new DateTimePicker();
        lblPeriod = new Label();
        splMain = new SplitContainer();
        grpMaster = new GroupBox();
        dgvMaster = new DataGridView();
        colResultId = new DataGridViewTextBoxColumn();
        colEventTime = new DataGridViewTextBoxColumn();
        colEquipId = new DataGridViewTextBoxColumn();
        colRecipeId = new DataGridViewTextBoxColumn();
        grpDetail = new GroupBox();
        dgvDetail = new DataGridView();
        colParameterId = new DataGridViewTextBoxColumn();
        colParameterValue = new DataGridViewTextBoxColumn();
        lblStatus = new Label();
        pnlCondition.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)splMain).BeginInit();
        splMain.Panel1.SuspendLayout();
        splMain.Panel2.SuspendLayout();
        splMain.SuspendLayout();
        grpMaster.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvMaster).BeginInit();
        grpDetail.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvDetail).BeginInit();
        SuspendLayout();
        //
        // pnlCondition
        //
        pnlCondition.Controls.Add(btnReset);
        pnlCondition.Controls.Add(btnSearch);
        pnlCondition.Controls.Add(txtRecipeId);
        pnlCondition.Controls.Add(lblRecipeId);
        pnlCondition.Controls.Add(txtEquipId);
        pnlCondition.Controls.Add(lblEquipId);
        pnlCondition.Controls.Add(dtpTo);
        pnlCondition.Controls.Add(lblTilde);
        pnlCondition.Controls.Add(dtpFrom);
        pnlCondition.Controls.Add(lblPeriod);
        pnlCondition.Dock = DockStyle.Top;
        pnlCondition.Location = new Point(0, 0);
        pnlCondition.Name = "pnlCondition";
        pnlCondition.Size = new Size(1184, 52);
        pnlCondition.TabIndex = 0;
        //
        // lblPeriod
        //
        lblPeriod.AutoSize = true;
        lblPeriod.Location = new Point(10, 18);
        lblPeriod.Name = "lblPeriod";
        lblPeriod.Size = new Size(55, 15);
        lblPeriod.TabIndex = 0;
        lblPeriod.Text = "이벤트 기간";
        //
        // dtpFrom
        //
        dtpFrom.CustomFormat = "yyyy-MM-dd HH:mm:ss";
        dtpFrom.Format = DateTimePickerFormat.Custom;
        dtpFrom.Location = new Point(80, 14);
        dtpFrom.Name = "dtpFrom";
        dtpFrom.Size = new Size(160, 23);
        dtpFrom.TabIndex = 1;
        //
        // lblTilde
        //
        lblTilde.AutoSize = true;
        lblTilde.Location = new Point(246, 18);
        lblTilde.Name = "lblTilde";
        lblTilde.Size = new Size(15, 15);
        lblTilde.TabIndex = 2;
        lblTilde.Text = "~";
        //
        // dtpTo
        //
        dtpTo.CustomFormat = "yyyy-MM-dd HH:mm:ss";
        dtpTo.Format = DateTimePickerFormat.Custom;
        dtpTo.Location = new Point(266, 14);
        dtpTo.Name = "dtpTo";
        dtpTo.Size = new Size(160, 23);
        dtpTo.TabIndex = 3;
        //
        // lblEquipId
        //
        lblEquipId.AutoSize = true;
        lblEquipId.Location = new Point(444, 18);
        lblEquipId.Name = "lblEquipId";
        lblEquipId.Size = new Size(43, 15);
        lblEquipId.TabIndex = 4;
        lblEquipId.Text = "설비 ID";
        //
        // txtEquipId
        //
        txtEquipId.Location = new Point(494, 14);
        txtEquipId.Name = "txtEquipId";
        txtEquipId.Size = new Size(120, 23);
        txtEquipId.TabIndex = 5;
        //
        // lblRecipeId
        //
        lblRecipeId.AutoSize = true;
        lblRecipeId.Location = new Point(628, 18);
        lblRecipeId.Name = "lblRecipeId";
        lblRecipeId.Size = new Size(55, 15);
        lblRecipeId.TabIndex = 6;
        lblRecipeId.Text = "레시피 ID";
        //
        // txtRecipeId
        //
        txtRecipeId.Location = new Point(690, 14);
        txtRecipeId.Name = "txtRecipeId";
        txtRecipeId.Size = new Size(120, 23);
        txtRecipeId.TabIndex = 7;
        //
        // btnSearch
        //
        btnSearch.Location = new Point(828, 11);
        btnSearch.Name = "btnSearch";
        btnSearch.Size = new Size(90, 30);
        btnSearch.TabIndex = 8;
        btnSearch.Text = "조회";
        btnSearch.UseVisualStyleBackColor = true;
        btnSearch.Click += btnSearch_Click;
        //
        // btnReset
        //
        btnReset.Location = new Point(924, 11);
        btnReset.Name = "btnReset";
        btnReset.Size = new Size(90, 30);
        btnReset.TabIndex = 9;
        btnReset.Text = "초기화";
        btnReset.UseVisualStyleBackColor = true;
        btnReset.Click += btnReset_Click;
        //
        // splMain
        //
        splMain.Dock = DockStyle.Fill;
        splMain.Location = new Point(0, 52);
        splMain.Name = "splMain";
        splMain.Orientation = Orientation.Horizontal;
        //
        // splMain.Panel1
        //
        splMain.Panel1.Controls.Add(grpMaster);
        //
        // splMain.Panel2
        //
        splMain.Panel2.Controls.Add(grpDetail);
        splMain.Size = new Size(1184, 585);
        splMain.SplitterDistance = 330;
        splMain.TabIndex = 1;
        //
        // grpMaster
        //
        grpMaster.Controls.Add(dgvMaster);
        grpMaster.Dock = DockStyle.Fill;
        grpMaster.Location = new Point(0, 0);
        grpMaster.Name = "grpMaster";
        grpMaster.Size = new Size(1184, 330);
        grpMaster.TabIndex = 0;
        grpMaster.TabStop = false;
        grpMaster.Text = "RMS 결과 (TB_RMS_RESULT)";
        //
        // dgvMaster
        //
        dgvMaster.AllowUserToAddRows = false;
        dgvMaster.AllowUserToDeleteRows = false;
        dgvMaster.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvMaster.Columns.AddRange(new DataGridViewColumn[] { colResultId, colEventTime, colEquipId, colRecipeId });
        dgvMaster.Dock = DockStyle.Fill;
        dgvMaster.Location = new Point(3, 19);
        dgvMaster.MultiSelect = false;
        dgvMaster.Name = "dgvMaster";
        dgvMaster.ReadOnly = true;
        dgvMaster.RowHeadersVisible = false;
        dgvMaster.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvMaster.Size = new Size(1178, 308);
        dgvMaster.TabIndex = 0;
        dgvMaster.SelectionChanged += dgvMaster_SelectionChanged;
        //
        // colResultId
        //
        colResultId.DataPropertyName = "ResultId";
        colResultId.HeaderText = "결과 ID";
        colResultId.Name = "colResultId";
        colResultId.ReadOnly = true;
        //
        // colEventTime
        //
        colEventTime.DataPropertyName = "EventTime";
        colEventTime.DefaultCellStyle.Format = "yyyy-MM-dd HH:mm:ss.fff";
        colEventTime.HeaderText = "이벤트 시간";
        colEventTime.Name = "colEventTime";
        colEventTime.ReadOnly = true;
        //
        // colEquipId
        //
        colEquipId.DataPropertyName = "EquipId";
        colEquipId.HeaderText = "설비 ID";
        colEquipId.Name = "colEquipId";
        colEquipId.ReadOnly = true;
        //
        // colRecipeId
        //
        colRecipeId.DataPropertyName = "RecipeId";
        colRecipeId.HeaderText = "레시피 ID";
        colRecipeId.Name = "colRecipeId";
        colRecipeId.ReadOnly = true;
        //
        // grpDetail
        //
        grpDetail.Controls.Add(dgvDetail);
        grpDetail.Dock = DockStyle.Fill;
        grpDetail.Location = new Point(0, 0);
        grpDetail.Name = "grpDetail";
        grpDetail.Size = new Size(1184, 251);
        grpDetail.TabIndex = 0;
        grpDetail.TabStop = false;
        grpDetail.Text = "선택 결과의 파라미터";
        //
        // dgvDetail
        //
        dgvDetail.AllowUserToAddRows = false;
        dgvDetail.AllowUserToDeleteRows = false;
        dgvDetail.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgvDetail.Columns.AddRange(new DataGridViewColumn[] { colParameterId, colParameterValue });
        dgvDetail.Dock = DockStyle.Fill;
        dgvDetail.Location = new Point(3, 19);
        dgvDetail.Name = "dgvDetail";
        dgvDetail.ReadOnly = true;
        dgvDetail.RowHeadersVisible = false;
        dgvDetail.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvDetail.Size = new Size(1178, 229);
        dgvDetail.TabIndex = 0;
        //
        // colParameterId
        //
        colParameterId.DataPropertyName = "ParameterId";
        colParameterId.HeaderText = "파라미터 ID";
        colParameterId.Name = "colParameterId";
        colParameterId.ReadOnly = true;
        //
        // colParameterValue
        //
        colParameterValue.DataPropertyName = "ParameterValue";
        colParameterValue.HeaderText = "파라미터 값";
        colParameterValue.Name = "colParameterValue";
        colParameterValue.ReadOnly = true;
        //
        // lblStatus
        //
        lblStatus.Dock = DockStyle.Bottom;
        lblStatus.Location = new Point(0, 637);
        lblStatus.Name = "lblStatus";
        lblStatus.Padding = new Padding(8, 0, 0, 0);
        lblStatus.Size = new Size(1184, 24);
        lblStatus.TabIndex = 2;
        lblStatus.Text = "이벤트 기간을 정하고 [조회]를 누르세요.";
        lblStatus.TextAlign = ContentAlignment.MiddleLeft;
        //
        // MainForm
        //
        AcceptButton = btnSearch;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1184, 661);
        Controls.Add(splMain);
        Controls.Add(pnlCondition);
        Controls.Add(lblStatus);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "RMS 결과 조회";
        pnlCondition.ResumeLayout(false);
        pnlCondition.PerformLayout();
        splMain.Panel1.ResumeLayout(false);
        splMain.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splMain).EndInit();
        splMain.ResumeLayout(false);
        grpMaster.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)dgvMaster).EndInit();
        grpDetail.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)dgvDetail).EndInit();
        ResumeLayout(false);
    }

    #endregion

    private Panel pnlCondition;
    private Label lblPeriod;
    private DateTimePicker dtpFrom;
    private Label lblTilde;
    private DateTimePicker dtpTo;
    private Label lblEquipId;
    private TextBox txtEquipId;
    private Label lblRecipeId;
    private TextBox txtRecipeId;
    private Button btnSearch;
    private Button btnReset;
    private SplitContainer splMain;
    private GroupBox grpMaster;
    private DataGridView dgvMaster;
    private DataGridViewTextBoxColumn colResultId;
    private DataGridViewTextBoxColumn colEventTime;
    private DataGridViewTextBoxColumn colEquipId;
    private DataGridViewTextBoxColumn colRecipeId;
    private GroupBox grpDetail;
    private DataGridView dgvDetail;
    private DataGridViewTextBoxColumn colParameterId;
    private DataGridViewTextBoxColumn colParameterValue;
    private Label lblStatus;
}
