namespace YsEdu.Viewer;

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
        pnlSearch = new Panel();
        lblResult = new Label();
        btnClear = new Button();
        btnSearch = new Button();
        dtpTo = new DateTimePicker();
        lblTo = new Label();
        dtpFrom = new DateTimePicker();
        lblFrom = new Label();
        cboEqp = new ComboBox();
        lblEqp = new Label();
        txtLotId = new TextBox();
        lblLotId = new Label();
        tabMain = new TabControl();
        tabLot = new TabPage();
        splLot = new SplitContainer();
        grpLots = new GroupBox();
        dgvLots = new DataGridView();
        grpEquipments = new GroupBox();
        dgvEquipments = new DataGridView();
        tabEvent = new TabPage();
        dgvEvents = new DataGridView();
        tabRun = new TabPage();
        dgvRuns = new DataGridView();
        tabRms = new TabPage();
        splRms = new SplitContainer();
        grpChecks = new GroupBox();
        dgvChecks = new DataGridView();
        grpCheckDetails = new GroupBox();
        dgvCheckDetails = new DataGridView();
        tabMsg = new TabPage();
        splMsg = new SplitContainer();
        dgvMsgLog = new DataGridView();
        txtRawXml = new TextBox();
        pnlSearch.SuspendLayout();
        tabMain.SuspendLayout();
        tabLot.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)splLot).BeginInit();
        splLot.Panel1.SuspendLayout();
        splLot.Panel2.SuspendLayout();
        splLot.SuspendLayout();
        grpLots.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvLots).BeginInit();
        grpEquipments.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvEquipments).BeginInit();
        tabEvent.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvEvents).BeginInit();
        tabRun.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvRuns).BeginInit();
        tabRms.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)splRms).BeginInit();
        splRms.Panel1.SuspendLayout();
        splRms.Panel2.SuspendLayout();
        splRms.SuspendLayout();
        grpChecks.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvChecks).BeginInit();
        grpCheckDetails.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvCheckDetails).BeginInit();
        tabMsg.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)splMsg).BeginInit();
        splMsg.Panel1.SuspendLayout();
        splMsg.Panel2.SuspendLayout();
        splMsg.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvMsgLog).BeginInit();
        SuspendLayout();
        //
        // pnlSearch
        //
        pnlSearch.Controls.Add(lblResult);
        pnlSearch.Controls.Add(btnClear);
        pnlSearch.Controls.Add(btnSearch);
        pnlSearch.Controls.Add(dtpTo);
        pnlSearch.Controls.Add(lblTo);
        pnlSearch.Controls.Add(dtpFrom);
        pnlSearch.Controls.Add(lblFrom);
        pnlSearch.Controls.Add(cboEqp);
        pnlSearch.Controls.Add(lblEqp);
        pnlSearch.Controls.Add(txtLotId);
        pnlSearch.Controls.Add(lblLotId);
        pnlSearch.Dock = DockStyle.Top;
        pnlSearch.Location = new Point(0, 0);
        pnlSearch.Name = "pnlSearch";
        pnlSearch.Size = new Size(1264, 52);
        pnlSearch.TabIndex = 0;
        //
        // lblLotId
        //
        lblLotId.AutoSize = true;
        lblLotId.Location = new Point(12, 18);
        lblLotId.Name = "lblLotId";
        lblLotId.Size = new Size(41, 15);
        lblLotId.TabIndex = 0;
        lblLotId.Text = "LOTID";
        //
        // txtLotId
        //
        txtLotId.Location = new Point(58, 14);
        txtLotId.Name = "txtLotId";
        txtLotId.PlaceholderText = "비우면 전체";
        txtLotId.Size = new Size(130, 23);
        txtLotId.TabIndex = 1;
        txtLotId.KeyDown += txtLotId_KeyDown;
        //
        // lblEqp
        //
        lblEqp.AutoSize = true;
        lblEqp.Location = new Point(204, 18);
        lblEqp.Name = "lblEqp";
        lblEqp.Size = new Size(31, 15);
        lblEqp.TabIndex = 2;
        lblEqp.Text = "설비";
        //
        // cboEqp
        //
        cboEqp.DropDownStyle = ComboBoxStyle.DropDownList;
        cboEqp.Location = new Point(240, 14);
        cboEqp.Name = "cboEqp";
        cboEqp.Size = new Size(110, 23);
        cboEqp.TabIndex = 3;
        //
        // lblFrom
        //
        lblFrom.AutoSize = true;
        lblFrom.Location = new Point(366, 18);
        lblFrom.Name = "lblFrom";
        lblFrom.Size = new Size(31, 15);
        lblFrom.TabIndex = 4;
        lblFrom.Text = "기간";
        //
        // dtpFrom
        //
        dtpFrom.CustomFormat = "yyyy-MM-dd HH:mm";
        dtpFrom.Format = DateTimePickerFormat.Custom;
        dtpFrom.Location = new Point(402, 14);
        dtpFrom.Name = "dtpFrom";
        dtpFrom.Size = new Size(140, 23);
        dtpFrom.TabIndex = 5;
        //
        // lblTo
        //
        lblTo.AutoSize = true;
        lblTo.Location = new Point(548, 18);
        lblTo.Name = "lblTo";
        lblTo.Size = new Size(15, 15);
        lblTo.TabIndex = 6;
        lblTo.Text = "~";
        //
        // dtpTo
        //
        dtpTo.CustomFormat = "yyyy-MM-dd HH:mm";
        dtpTo.Format = DateTimePickerFormat.Custom;
        dtpTo.Location = new Point(568, 14);
        dtpTo.Name = "dtpTo";
        dtpTo.Size = new Size(140, 23);
        dtpTo.TabIndex = 7;
        //
        // btnSearch
        //
        btnSearch.Location = new Point(724, 11);
        btnSearch.Name = "btnSearch";
        btnSearch.Size = new Size(90, 30);
        btnSearch.TabIndex = 8;
        btnSearch.Text = "조회";
        btnSearch.UseVisualStyleBackColor = true;
        btnSearch.Click += btnSearch_Click;
        //
        // btnClear
        //
        btnClear.Location = new Point(820, 11);
        btnClear.Name = "btnClear";
        btnClear.Size = new Size(90, 30);
        btnClear.TabIndex = 9;
        btnClear.Text = "조건 초기화";
        btnClear.UseVisualStyleBackColor = true;
        btnClear.Click += btnClear_Click;
        //
        // lblResult
        //
        lblResult.AutoSize = true;
        lblResult.Location = new Point(926, 18);
        lblResult.Name = "lblResult";
        lblResult.Size = new Size(0, 15);
        lblResult.TabIndex = 10;
        //
        // tabMain
        //
        tabMain.Controls.Add(tabLot);
        tabMain.Controls.Add(tabEvent);
        tabMain.Controls.Add(tabRun);
        tabMain.Controls.Add(tabRms);
        tabMain.Controls.Add(tabMsg);
        tabMain.Dock = DockStyle.Fill;
        tabMain.Location = new Point(0, 52);
        tabMain.Name = "tabMain";
        tabMain.SelectedIndex = 0;
        tabMain.Size = new Size(1264, 709);
        tabMain.TabIndex = 1;
        //
        // tabLot
        //
        tabLot.Controls.Add(splLot);
        tabLot.Location = new Point(4, 24);
        tabLot.Name = "tabLot";
        tabLot.Padding = new Padding(3);
        tabLot.Size = new Size(1256, 681);
        tabLot.TabIndex = 0;
        tabLot.Text = "LOT 작업 · 설비 구동 상태";
        tabLot.UseVisualStyleBackColor = true;
        //
        // splLot
        //
        splLot.Dock = DockStyle.Fill;
        splLot.Location = new Point(3, 3);
        splLot.Name = "splLot";
        splLot.Orientation = Orientation.Horizontal;
        //
        // splLot.Panel1
        //
        splLot.Panel1.Controls.Add(grpLots);
        //
        // splLot.Panel2
        //
        splLot.Panel2.Controls.Add(grpEquipments);
        splLot.Size = new Size(1250, 675);
        splLot.SplitterDistance = 400;
        splLot.TabIndex = 0;
        //
        // grpLots
        //
        grpLots.Controls.Add(dgvLots);
        grpLots.Dock = DockStyle.Fill;
        grpLots.Location = new Point(0, 0);
        grpLots.Name = "grpLots";
        grpLots.Size = new Size(1250, 400);
        grpLots.TabIndex = 0;
        grpLots.TabStop = false;
        grpLots.Text = "LOT 작업 상태 (TB_LOT)";
        //
        // dgvLots
        //
        dgvLots.AllowUserToAddRows = false;
        dgvLots.AllowUserToDeleteRows = false;
        dgvLots.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
        dgvLots.Dock = DockStyle.Fill;
        dgvLots.Location = new Point(3, 19);
        dgvLots.Name = "dgvLots";
        dgvLots.ReadOnly = true;
        dgvLots.RowHeadersVisible = false;
        dgvLots.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvLots.Size = new Size(1244, 378);
        dgvLots.TabIndex = 0;
        //
        // grpEquipments
        //
        grpEquipments.Controls.Add(dgvEquipments);
        grpEquipments.Dock = DockStyle.Fill;
        grpEquipments.Location = new Point(0, 0);
        grpEquipments.Name = "grpEquipments";
        grpEquipments.Size = new Size(1250, 271);
        grpEquipments.TabIndex = 0;
        grpEquipments.TabStop = false;
        grpEquipments.Text = "설비 구동 상태 (TB_EQUIPMENT)";
        //
        // dgvEquipments
        //
        dgvEquipments.AllowUserToAddRows = false;
        dgvEquipments.AllowUserToDeleteRows = false;
        dgvEquipments.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
        dgvEquipments.Dock = DockStyle.Fill;
        dgvEquipments.Location = new Point(3, 19);
        dgvEquipments.Name = "dgvEquipments";
        dgvEquipments.ReadOnly = true;
        dgvEquipments.RowHeadersVisible = false;
        dgvEquipments.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvEquipments.Size = new Size(1244, 249);
        dgvEquipments.TabIndex = 0;
        //
        // tabEvent
        //
        tabEvent.Controls.Add(dgvEvents);
        tabEvent.Location = new Point(4, 24);
        tabEvent.Name = "tabEvent";
        tabEvent.Padding = new Padding(3);
        tabEvent.Size = new Size(1256, 681);
        tabEvent.TabIndex = 1;
        tabEvent.Text = "이벤트 이력";
        tabEvent.UseVisualStyleBackColor = true;
        //
        // dgvEvents
        //
        dgvEvents.AllowUserToAddRows = false;
        dgvEvents.AllowUserToDeleteRows = false;
        dgvEvents.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
        dgvEvents.Dock = DockStyle.Fill;
        dgvEvents.Location = new Point(3, 3);
        dgvEvents.Name = "dgvEvents";
        dgvEvents.ReadOnly = true;
        dgvEvents.RowHeadersVisible = false;
        dgvEvents.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvEvents.Size = new Size(1250, 675);
        dgvEvents.TabIndex = 0;
        //
        // tabRun
        //
        tabRun.Controls.Add(dgvRuns);
        tabRun.Location = new Point(4, 24);
        tabRun.Name = "tabRun";
        tabRun.Padding = new Padding(3);
        tabRun.Size = new Size(1256, 681);
        tabRun.TabIndex = 2;
        tabRun.Text = "설비별 구동 이력";
        tabRun.UseVisualStyleBackColor = true;
        //
        // dgvRuns
        //
        dgvRuns.AllowUserToAddRows = false;
        dgvRuns.AllowUserToDeleteRows = false;
        dgvRuns.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
        dgvRuns.Dock = DockStyle.Fill;
        dgvRuns.Location = new Point(3, 3);
        dgvRuns.Name = "dgvRuns";
        dgvRuns.ReadOnly = true;
        dgvRuns.RowHeadersVisible = false;
        dgvRuns.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvRuns.Size = new Size(1250, 675);
        dgvRuns.TabIndex = 0;
        //
        // tabRms
        //
        tabRms.Controls.Add(splRms);
        tabRms.Location = new Point(4, 24);
        tabRms.Name = "tabRms";
        tabRms.Padding = new Padding(3);
        tabRms.Size = new Size(1256, 681);
        tabRms.TabIndex = 3;
        tabRms.Text = "RMS 검증 이력";
        tabRms.UseVisualStyleBackColor = true;
        //
        // splRms
        //
        splRms.Dock = DockStyle.Fill;
        splRms.Location = new Point(3, 3);
        splRms.Name = "splRms";
        splRms.Orientation = Orientation.Horizontal;
        //
        // splRms.Panel1
        //
        splRms.Panel1.Controls.Add(grpChecks);
        //
        // splRms.Panel2
        //
        splRms.Panel2.Controls.Add(grpCheckDetails);
        splRms.Size = new Size(1250, 675);
        splRms.SplitterDistance = 360;
        splRms.TabIndex = 0;
        //
        // grpChecks
        //
        grpChecks.Controls.Add(dgvChecks);
        grpChecks.Dock = DockStyle.Fill;
        grpChecks.Location = new Point(0, 0);
        grpChecks.Name = "grpChecks";
        grpChecks.Size = new Size(1250, 360);
        grpChecks.TabIndex = 0;
        grpChecks.TabStop = false;
        grpChecks.Text = "검증 이력 — 종합 판정 (행을 선택하면 아래에 상세)";
        //
        // dgvChecks
        //
        dgvChecks.AllowUserToAddRows = false;
        dgvChecks.AllowUserToDeleteRows = false;
        dgvChecks.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
        dgvChecks.Dock = DockStyle.Fill;
        dgvChecks.Location = new Point(3, 19);
        dgvChecks.MultiSelect = false;
        dgvChecks.Name = "dgvChecks";
        dgvChecks.ReadOnly = true;
        dgvChecks.RowHeadersVisible = false;
        dgvChecks.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvChecks.Size = new Size(1244, 338);
        dgvChecks.TabIndex = 0;
        dgvChecks.SelectionChanged += dgvChecks_SelectionChanged;
        //
        // grpCheckDetails
        //
        grpCheckDetails.Controls.Add(dgvCheckDetails);
        grpCheckDetails.Dock = DockStyle.Fill;
        grpCheckDetails.Location = new Point(0, 0);
        grpCheckDetails.Name = "grpCheckDetails";
        grpCheckDetails.Size = new Size(1250, 311);
        grpCheckDetails.TabIndex = 0;
        grpCheckDetails.TabStop = false;
        grpCheckDetails.Text = "Parameter별 기준값 · 수신값 · 판정 · 사유";
        //
        // dgvCheckDetails
        //
        dgvCheckDetails.AllowUserToAddRows = false;
        dgvCheckDetails.AllowUserToDeleteRows = false;
        dgvCheckDetails.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
        dgvCheckDetails.Dock = DockStyle.Fill;
        dgvCheckDetails.Location = new Point(3, 19);
        dgvCheckDetails.Name = "dgvCheckDetails";
        dgvCheckDetails.ReadOnly = true;
        dgvCheckDetails.RowHeadersVisible = false;
        dgvCheckDetails.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvCheckDetails.Size = new Size(1244, 289);
        dgvCheckDetails.TabIndex = 0;
        //
        // tabMsg
        //
        tabMsg.Controls.Add(splMsg);
        tabMsg.Location = new Point(4, 24);
        tabMsg.Name = "tabMsg";
        tabMsg.Padding = new Padding(3);
        tabMsg.Size = new Size(1256, 681);
        tabMsg.TabIndex = 4;
        tabMsg.Text = "수신 메시지 · 실패 사유";
        tabMsg.UseVisualStyleBackColor = true;
        //
        // splMsg
        //
        splMsg.Dock = DockStyle.Fill;
        splMsg.Location = new Point(3, 3);
        splMsg.Name = "splMsg";
        splMsg.Orientation = Orientation.Horizontal;
        //
        // splMsg.Panel1
        //
        splMsg.Panel1.Controls.Add(dgvMsgLog);
        //
        // splMsg.Panel2
        //
        splMsg.Panel2.Controls.Add(txtRawXml);
        splMsg.Size = new Size(1250, 675);
        splMsg.SplitterDistance = 420;
        splMsg.TabIndex = 0;
        //
        // dgvMsgLog
        //
        dgvMsgLog.AllowUserToAddRows = false;
        dgvMsgLog.AllowUserToDeleteRows = false;
        dgvMsgLog.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
        dgvMsgLog.Dock = DockStyle.Fill;
        dgvMsgLog.Location = new Point(0, 0);
        dgvMsgLog.MultiSelect = false;
        dgvMsgLog.Name = "dgvMsgLog";
        dgvMsgLog.ReadOnly = true;
        dgvMsgLog.RowHeadersVisible = false;
        dgvMsgLog.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvMsgLog.Size = new Size(1250, 420);
        dgvMsgLog.TabIndex = 0;
        dgvMsgLog.SelectionChanged += dgvMsgLog_SelectionChanged;
        //
        // txtRawXml
        //
        txtRawXml.Dock = DockStyle.Fill;
        txtRawXml.Font = new Font("Consolas", 9.5F);
        txtRawXml.Location = new Point(0, 0);
        txtRawXml.Multiline = true;
        txtRawXml.Name = "txtRawXml";
        txtRawXml.ReadOnly = true;
        txtRawXml.ScrollBars = ScrollBars.Both;
        txtRawXml.Size = new Size(1250, 251);
        txtRawXml.TabIndex = 0;
        txtRawXml.WordWrap = false;
        //
        // MainForm
        //
        AcceptButton = btnSearch;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1264, 761);
        Controls.Add(tabMain);
        Controls.Add(pnlSearch);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "생산 이력 · RMS 검증 조회";
        Load += MainForm_Load;
        pnlSearch.ResumeLayout(false);
        pnlSearch.PerformLayout();
        tabMain.ResumeLayout(false);
        tabLot.ResumeLayout(false);
        splLot.Panel1.ResumeLayout(false);
        splLot.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splLot).EndInit();
        splLot.ResumeLayout(false);
        grpLots.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)dgvLots).EndInit();
        grpEquipments.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)dgvEquipments).EndInit();
        tabEvent.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)dgvEvents).EndInit();
        tabRun.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)dgvRuns).EndInit();
        tabRms.ResumeLayout(false);
        splRms.Panel1.ResumeLayout(false);
        splRms.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splRms).EndInit();
        splRms.ResumeLayout(false);
        grpChecks.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)dgvChecks).EndInit();
        grpCheckDetails.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)dgvCheckDetails).EndInit();
        tabMsg.ResumeLayout(false);
        splMsg.Panel1.ResumeLayout(false);
        splMsg.Panel2.ResumeLayout(false);
        splMsg.Panel2.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)splMsg).EndInit();
        splMsg.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)dgvMsgLog).EndInit();
        ResumeLayout(false);
    }

    #endregion

    private Panel pnlSearch;
    private Label lblLotId;
    private TextBox txtLotId;
    private Label lblEqp;
    private ComboBox cboEqp;
    private Label lblFrom;
    private DateTimePicker dtpFrom;
    private Label lblTo;
    private DateTimePicker dtpTo;
    private Button btnSearch;
    private Button btnClear;
    private Label lblResult;
    private TabControl tabMain;
    private TabPage tabLot;
    private SplitContainer splLot;
    private GroupBox grpLots;
    private DataGridView dgvLots;
    private GroupBox grpEquipments;
    private DataGridView dgvEquipments;
    private TabPage tabEvent;
    private DataGridView dgvEvents;
    private TabPage tabRun;
    private DataGridView dgvRuns;
    private TabPage tabRms;
    private SplitContainer splRms;
    private GroupBox grpChecks;
    private DataGridView dgvChecks;
    private GroupBox grpCheckDetails;
    private DataGridView dgvCheckDetails;
    private TabPage tabMsg;
    private SplitContainer splMsg;
    private DataGridView dgvMsgLog;
    private TextBox txtRawXml;
}
