namespace YsEdu.Simulator;

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
        pnlLeft = new Panel();
        grpManual = new GroupBox();
        btnResend = new Button();
        btnSend = new Button();
        btnBuild = new Button();
        txtParams = new TextBox();
        lblParams = new Label();
        cboEventType = new ComboBox();
        lblEventType = new Label();
        cboEqp = new ComboBox();
        lblEqp = new Label();
        grpScenario = new GroupBox();
        btnResetHistory = new Button();
        btnErrors = new Button();
        btnOrderDup = new Button();
        btnStructure = new Button();
        btnMismatch = new Button();
        btnMiddle = new Button();
        btnNormal = new Button();
        txtLotId = new TextBox();
        lblLotId = new Label();
        lblConnection = new Label();
        splMain = new SplitContainer();
        splTop = new SplitContainer();
        dgvTrace = new DataGridView();
        lstSteps = new ListBox();
        txtXml = new TextBox();
        pnlLeft.SuspendLayout();
        grpManual.SuspendLayout();
        grpScenario.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)splMain).BeginInit();
        splMain.Panel1.SuspendLayout();
        splMain.Panel2.SuspendLayout();
        splMain.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)splTop).BeginInit();
        splTop.Panel1.SuspendLayout();
        splTop.Panel2.SuspendLayout();
        splTop.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvTrace).BeginInit();
        SuspendLayout();
        //
        // pnlLeft
        //
        pnlLeft.Controls.Add(grpManual);
        pnlLeft.Controls.Add(grpScenario);
        pnlLeft.Controls.Add(lblConnection);
        pnlLeft.Dock = DockStyle.Left;
        pnlLeft.Location = new Point(0, 0);
        pnlLeft.Name = "pnlLeft";
        pnlLeft.Padding = new Padding(8);
        pnlLeft.Size = new Size(300, 781);
        pnlLeft.TabIndex = 0;
        //
        // lblConnection
        //
        lblConnection.Dock = DockStyle.Top;
        lblConnection.Location = new Point(8, 8);
        lblConnection.Name = "lblConnection";
        lblConnection.Size = new Size(284, 36);
        lblConnection.TabIndex = 0;
        lblConnection.Text = "Kafka 연결 중…";
        //
        // grpScenario
        //
        grpScenario.Controls.Add(btnResetHistory);
        grpScenario.Controls.Add(btnErrors);
        grpScenario.Controls.Add(btnOrderDup);
        grpScenario.Controls.Add(btnStructure);
        grpScenario.Controls.Add(btnMismatch);
        grpScenario.Controls.Add(btnMiddle);
        grpScenario.Controls.Add(btnNormal);
        grpScenario.Controls.Add(txtLotId);
        grpScenario.Controls.Add(lblLotId);
        grpScenario.Dock = DockStyle.Top;
        grpScenario.Location = new Point(8, 44);
        grpScenario.Name = "grpScenario";
        grpScenario.Size = new Size(284, 352);
        grpScenario.TabIndex = 1;
        grpScenario.TabStop = false;
        grpScenario.Text = "완료 확인 시나리오 (2차 p.12)";
        //
        // lblLotId
        //
        lblLotId.AutoSize = true;
        lblLotId.Location = new Point(10, 28);
        lblLotId.Name = "lblLotId";
        lblLotId.Size = new Size(41, 15);
        lblLotId.TabIndex = 0;
        lblLotId.Text = "LOTID";
        //
        // txtLotId
        //
        txtLotId.Location = new Point(70, 24);
        txtLotId.Name = "txtLotId";
        txtLotId.Size = new Size(200, 23);
        txtLotId.TabIndex = 1;
        txtLotId.Text = "LOT_001";
        //
        // btnNormal
        //
        btnNormal.Location = new Point(10, 58);
        btnNormal.Name = "btnNormal";
        btnNormal.Size = new Size(260, 34);
        btnNormal.TabIndex = 2;
        btnNormal.Text = "① 정상 인라인 (A → B → C)";
        btnNormal.UseVisualStyleBackColor = true;
        btnNormal.Click += btnNormal_Click;
        //
        // btnMiddle
        //
        btnMiddle.Location = new Point(10, 96);
        btnMiddle.Name = "btnMiddle";
        btnMiddle.Size = new Size(260, 34);
        btnMiddle.TabIndex = 3;
        btnMiddle.Text = "② 중간 설비 (B, LOT_START 없음)";
        btnMiddle.UseVisualStyleBackColor = true;
        btnMiddle.Click += btnMiddle_Click;
        //
        // btnMismatch
        //
        btnMismatch.Location = new Point(10, 134);
        btnMismatch.Name = "btnMismatch";
        btnMismatch.Size = new Size(260, 34);
        btnMismatch.TabIndex = 4;
        btnMismatch.Text = "③ RMS 불일치 (SPEED=500 → 450)";
        btnMismatch.UseVisualStyleBackColor = true;
        btnMismatch.Click += btnMismatch_Click;
        //
        // btnStructure
        //
        btnStructure.Location = new Point(10, 172);
        btnStructure.Name = "btnStructure";
        btnStructure.Size = new Size(260, 34);
        btnStructure.TabIndex = 5;
        btnStructure.Text = "④ 구성 오류 (누락 / 추가 / 중복)";
        btnStructure.UseVisualStyleBackColor = true;
        btnStructure.Click += btnStructure_Click;
        //
        // btnOrderDup
        //
        btnOrderDup.Location = new Point(10, 210);
        btnOrderDup.Name = "btnOrderDup";
        btnOrderDup.Size = new Size(260, 34);
        btnOrderDup.TabIndex = 6;
        btnOrderDup.Text = "⑤ 순서 · 중복 (START 없는 END / 재수신)";
        btnOrderDup.UseVisualStyleBackColor = true;
        btnOrderDup.Click += btnOrderDup_Click;
        //
        // btnErrors
        //
        btnErrors.Location = new Point(10, 248);
        btnErrors.Name = "btnErrors";
        btnErrors.Size = new Size(260, 34);
        btnErrors.TabIndex = 7;
        btnErrors.Text = "⑥ 처리 오류 (XML / 미등록 / DB 실패)";
        btnErrors.UseVisualStyleBackColor = true;
        btnErrors.Click += btnErrors_Click;
        //
        // btnResetHistory
        //
        btnResetHistory.ForeColor = Color.Firebrick;
        btnResetHistory.Location = new Point(10, 300);
        btnResetHistory.Name = "btnResetHistory";
        btnResetHistory.Size = new Size(260, 34);
        btnResetHistory.TabIndex = 8;
        btnResetHistory.Text = "이력 초기화 (기준정보 유지)";
        btnResetHistory.UseVisualStyleBackColor = true;
        btnResetHistory.Click += btnResetHistory_Click;
        //
        // grpManual
        //
        grpManual.Controls.Add(btnResend);
        grpManual.Controls.Add(btnSend);
        grpManual.Controls.Add(btnBuild);
        grpManual.Controls.Add(txtParams);
        grpManual.Controls.Add(lblParams);
        grpManual.Controls.Add(cboEventType);
        grpManual.Controls.Add(lblEventType);
        grpManual.Controls.Add(cboEqp);
        grpManual.Controls.Add(lblEqp);
        grpManual.Dock = DockStyle.Top;
        grpManual.Location = new Point(8, 396);
        grpManual.Name = "grpManual";
        grpManual.Size = new Size(284, 236);
        grpManual.TabIndex = 2;
        grpManual.TabStop = false;
        grpManual.Text = "직접 송신 (오른쪽 XML 편집 가능)";
        //
        // lblEqp
        //
        lblEqp.AutoSize = true;
        lblEqp.Location = new Point(10, 30);
        lblEqp.Name = "lblEqp";
        lblEqp.Size = new Size(39, 15);
        lblEqp.TabIndex = 0;
        lblEqp.Text = "EQPID";
        //
        // cboEqp
        //
        cboEqp.DropDownStyle = ComboBoxStyle.DropDownList;
        cboEqp.Location = new Point(90, 26);
        cboEqp.Name = "cboEqp";
        cboEqp.Size = new Size(180, 23);
        cboEqp.TabIndex = 1;
        //
        // lblEventType
        //
        lblEventType.AutoSize = true;
        lblEventType.Location = new Point(10, 62);
        lblEventType.Name = "lblEventType";
        lblEventType.Size = new Size(64, 15);
        lblEventType.TabIndex = 2;
        lblEventType.Text = "EventType";
        //
        // cboEventType
        //
        cboEventType.DropDownStyle = ComboBoxStyle.DropDownList;
        cboEventType.Location = new Point(90, 58);
        cboEventType.Name = "cboEventType";
        cboEventType.Size = new Size(180, 23);
        cboEventType.TabIndex = 3;
        //
        // lblParams
        //
        lblParams.AutoSize = true;
        lblParams.Location = new Point(10, 94);
        lblParams.Name = "lblParams";
        lblParams.Size = new Size(73, 15);
        lblParams.TabIndex = 4;
        lblParams.Text = "Parameters";
        //
        // txtParams
        //
        txtParams.Location = new Point(90, 90);
        txtParams.Name = "txtParams";
        txtParams.Size = new Size(180, 23);
        txtParams.TabIndex = 5;
        txtParams.Text = "TEMP=120;PRESSURE=30;SPEED=450";
        //
        // btnBuild
        //
        btnBuild.Location = new Point(10, 126);
        btnBuild.Name = "btnBuild";
        btnBuild.Size = new Size(260, 30);
        btnBuild.TabIndex = 6;
        btnBuild.Text = "XML 만들기 (새 MessageName)";
        btnBuild.UseVisualStyleBackColor = true;
        btnBuild.Click += btnBuild_Click;
        //
        // btnSend
        //
        btnSend.Location = new Point(10, 160);
        btnSend.Name = "btnSend";
        btnSend.Size = new Size(260, 30);
        btnSend.TabIndex = 7;
        btnSend.Text = "XML 송신";
        btnSend.UseVisualStyleBackColor = true;
        btnSend.Click += btnSend_Click;
        //
        // btnResend
        //
        btnResend.Location = new Point(10, 194);
        btnResend.Name = "btnResend";
        btnResend.Size = new Size(260, 30);
        btnResend.TabIndex = 8;
        btnResend.Text = "직전 XML 그대로 재송신 (중복 시험)";
        btnResend.UseVisualStyleBackColor = true;
        btnResend.Click += btnResend_Click;
        //
        // splMain
        //
        splMain.Dock = DockStyle.Fill;
        splMain.Location = new Point(300, 0);
        splMain.Name = "splMain";
        splMain.Orientation = Orientation.Horizontal;
        //
        // splMain.Panel1
        //
        splMain.Panel1.Controls.Add(splTop);
        //
        // splMain.Panel2
        //
        splMain.Panel2.Controls.Add(txtXml);
        splMain.Size = new Size(884, 781);
        splMain.SplitterDistance = 470;
        splMain.TabIndex = 1;
        //
        // splTop
        //
        splTop.Dock = DockStyle.Fill;
        splTop.Location = new Point(0, 0);
        splTop.Name = "splTop";
        splTop.Orientation = Orientation.Horizontal;
        //
        // splTop.Panel1
        //
        splTop.Panel1.Controls.Add(dgvTrace);
        //
        // splTop.Panel2
        //
        splTop.Panel2.Controls.Add(lstSteps);
        splTop.Size = new Size(884, 470);
        splTop.SplitterDistance = 290;
        splTop.TabIndex = 0;
        //
        // dgvTrace
        //
        dgvTrace.AllowUserToAddRows = false;
        dgvTrace.AllowUserToDeleteRows = false;
        dgvTrace.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
        dgvTrace.Dock = DockStyle.Fill;
        dgvTrace.Location = new Point(0, 0);
        dgvTrace.MultiSelect = false;
        dgvTrace.Name = "dgvTrace";
        dgvTrace.ReadOnly = true;
        dgvTrace.RowHeadersVisible = false;
        dgvTrace.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvTrace.Size = new Size(884, 290);
        dgvTrace.TabIndex = 0;
        dgvTrace.SelectionChanged += dgvTrace_SelectionChanged;
        //
        // lstSteps
        //
        lstSteps.Dock = DockStyle.Fill;
        lstSteps.Font = new Font("맑은 고딕", 9F);
        lstSteps.HorizontalScrollbar = true;
        lstSteps.IntegralHeight = false;
        lstSteps.Location = new Point(0, 0);
        lstSteps.Name = "lstSteps";
        lstSteps.Size = new Size(884, 176);
        lstSteps.TabIndex = 0;
        //
        // txtXml
        //
        txtXml.Dock = DockStyle.Fill;
        txtXml.Font = new Font("Consolas", 9.5F);
        txtXml.Location = new Point(0, 0);
        txtXml.Multiline = true;
        txtXml.Name = "txtXml";
        txtXml.ScrollBars = ScrollBars.Both;
        txtXml.Size = new Size(884, 307);
        txtXml.TabIndex = 0;
        txtXml.WordWrap = false;
        //
        // MainForm
        //
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1184, 781);
        Controls.Add(splMain);
        Controls.Add(pnlLeft);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "설비 / CIM 시뮬레이터 — A · B · C 인라인";
        FormClosing += MainForm_FormClosing;
        Load += MainForm_Load;
        pnlLeft.ResumeLayout(false);
        grpManual.ResumeLayout(false);
        grpManual.PerformLayout();
        grpScenario.ResumeLayout(false);
        grpScenario.PerformLayout();
        splMain.Panel1.ResumeLayout(false);
        splMain.Panel2.ResumeLayout(false);
        splMain.Panel2.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)splMain).EndInit();
        splMain.ResumeLayout(false);
        splTop.Panel1.ResumeLayout(false);
        splTop.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splTop).EndInit();
        splTop.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)dgvTrace).EndInit();
        ResumeLayout(false);
    }

    #endregion

    private Panel pnlLeft;
    private Label lblConnection;
    private GroupBox grpScenario;
    private Label lblLotId;
    private TextBox txtLotId;
    private Button btnNormal;
    private Button btnMiddle;
    private Button btnMismatch;
    private Button btnStructure;
    private Button btnOrderDup;
    private Button btnErrors;
    private Button btnResetHistory;
    private GroupBox grpManual;
    private Label lblEqp;
    private ComboBox cboEqp;
    private Label lblEventType;
    private ComboBox cboEventType;
    private Label lblParams;
    private TextBox txtParams;
    private Button btnBuild;
    private Button btnSend;
    private Button btnResend;
    private SplitContainer splMain;
    private SplitContainer splTop;
    private DataGridView dgvTrace;
    private ListBox lstSteps;
    private TextBox txtXml;
}
