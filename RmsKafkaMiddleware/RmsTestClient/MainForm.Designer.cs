namespace RmsTestClient;

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
        btnSend = new Button();
        chkNowTime = new CheckBox();
        lstSamples = new ListBox();
        lblSamples = new Label();
        lblConnection = new Label();
        splMain = new SplitContainer();
        txtRequest = new TextBox();
        lblRequest = new Label();
        splResponse = new SplitContainer();
        dgvResponses = new DataGridView();
        txtResponse = new TextBox();
        pnlLeft.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)splMain).BeginInit();
        splMain.Panel1.SuspendLayout();
        splMain.Panel2.SuspendLayout();
        splMain.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)splResponse).BeginInit();
        splResponse.Panel1.SuspendLayout();
        splResponse.Panel2.SuspendLayout();
        splResponse.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvResponses).BeginInit();
        SuspendLayout();
        //
        // pnlLeft
        //
        pnlLeft.Controls.Add(btnSend);
        pnlLeft.Controls.Add(chkNowTime);
        pnlLeft.Controls.Add(lstSamples);
        pnlLeft.Controls.Add(lblSamples);
        pnlLeft.Controls.Add(lblConnection);
        pnlLeft.Dock = DockStyle.Left;
        pnlLeft.Location = new Point(0, 0);
        pnlLeft.Name = "pnlLeft";
        pnlLeft.Size = new Size(260, 721);
        pnlLeft.TabIndex = 0;
        //
        // lblConnection
        //
        lblConnection.Location = new Point(10, 10);
        lblConnection.Name = "lblConnection";
        lblConnection.Size = new Size(240, 40);
        lblConnection.TabIndex = 0;
        lblConnection.Text = "Kafka 연결 중…";
        //
        // lblSamples
        //
        lblSamples.AutoSize = true;
        lblSamples.Location = new Point(10, 58);
        lblSamples.Name = "lblSamples";
        lblSamples.Size = new Size(100, 15);
        lblSamples.TabIndex = 1;
        lblSamples.Text = "요청 예제 (Samples)";
        //
        // lstSamples
        //
        lstSamples.IntegralHeight = false;
        lstSamples.Location = new Point(10, 78);
        lstSamples.Name = "lstSamples";
        lstSamples.Size = new Size(240, 200);
        lstSamples.TabIndex = 2;
        lstSamples.SelectedIndexChanged += lstSamples_SelectedIndexChanged;
        //
        // chkNowTime
        //
        chkNowTime.Checked = true;
        chkNowTime.CheckState = CheckState.Checked;
        chkNowTime.Location = new Point(10, 286);
        chkNowTime.Name = "chkNowTime";
        chkNowTime.Size = new Size(240, 40);
        chkNowTime.TabIndex = 3;
        chkNowTime.Text = "예제를 고를 때 EVENT_TIME을 현재 시각으로 (새 요청)";
        chkNowTime.UseVisualStyleBackColor = true;
        //
        // btnSend
        //
        btnSend.Location = new Point(10, 334);
        btnSend.Name = "btnSend";
        btnSend.Size = new Size(240, 40);
        btnSend.TabIndex = 4;
        btnSend.Text = "RMS.REQUEST로 송신";
        btnSend.UseVisualStyleBackColor = true;
        btnSend.Click += btnSend_Click;
        //
        // splMain
        //
        splMain.Dock = DockStyle.Fill;
        splMain.Location = new Point(260, 0);
        splMain.Name = "splMain";
        //
        // splMain.Panel1
        //
        splMain.Panel1.Controls.Add(txtRequest);
        splMain.Panel1.Controls.Add(lblRequest);
        //
        // splMain.Panel2
        //
        splMain.Panel2.Controls.Add(splResponse);
        splMain.Size = new Size(924, 721);
        splMain.SplitterDistance = 420;
        splMain.TabIndex = 1;
        //
        // lblRequest
        //
        lblRequest.Dock = DockStyle.Top;
        lblRequest.Location = new Point(0, 0);
        lblRequest.Name = "lblRequest";
        lblRequest.Size = new Size(420, 24);
        lblRequest.TabIndex = 0;
        lblRequest.Text = "요청 XML (편집 가능)";
        lblRequest.TextAlign = ContentAlignment.MiddleLeft;
        //
        // txtRequest
        //
        txtRequest.Dock = DockStyle.Fill;
        txtRequest.Font = new Font("Consolas", 9.5F);
        txtRequest.Location = new Point(0, 24);
        txtRequest.Multiline = true;
        txtRequest.Name = "txtRequest";
        txtRequest.ScrollBars = ScrollBars.Both;
        txtRequest.Size = new Size(420, 697);
        txtRequest.TabIndex = 1;
        txtRequest.WordWrap = false;
        //
        // splResponse
        //
        splResponse.Dock = DockStyle.Fill;
        splResponse.Location = new Point(0, 0);
        splResponse.Name = "splResponse";
        splResponse.Orientation = Orientation.Horizontal;
        //
        // splResponse.Panel1
        //
        splResponse.Panel1.Controls.Add(dgvResponses);
        //
        // splResponse.Panel2
        //
        splResponse.Panel2.Controls.Add(txtResponse);
        splResponse.Size = new Size(500, 721);
        splResponse.SplitterDistance = 300;
        splResponse.TabIndex = 0;
        //
        // dgvResponses
        //
        dgvResponses.AllowUserToAddRows = false;
        dgvResponses.AllowUserToDeleteRows = false;
        dgvResponses.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
        dgvResponses.Dock = DockStyle.Fill;
        dgvResponses.Location = new Point(0, 0);
        dgvResponses.MultiSelect = false;
        dgvResponses.Name = "dgvResponses";
        dgvResponses.ReadOnly = true;
        dgvResponses.RowHeadersVisible = false;
        dgvResponses.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvResponses.Size = new Size(500, 300);
        dgvResponses.TabIndex = 0;
        dgvResponses.SelectionChanged += dgvResponses_SelectionChanged;
        //
        // txtResponse
        //
        txtResponse.Dock = DockStyle.Fill;
        txtResponse.Font = new Font("Consolas", 9.5F);
        txtResponse.Location = new Point(0, 0);
        txtResponse.Multiline = true;
        txtResponse.Name = "txtResponse";
        txtResponse.ReadOnly = true;
        txtResponse.ScrollBars = ScrollBars.Both;
        txtResponse.Size = new Size(500, 417);
        txtResponse.TabIndex = 0;
        txtResponse.WordWrap = false;
        //
        // MainForm
        //
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1184, 721);
        Controls.Add(splMain);
        Controls.Add(pnlLeft);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "RMS_TEST 요청 송신 도구 — RMS.REQUEST / RMS.RESPONSE";
        FormClosing += MainForm_FormClosing;
        Load += MainForm_Load;
        pnlLeft.ResumeLayout(false);
        pnlLeft.PerformLayout();
        splMain.Panel1.ResumeLayout(false);
        splMain.Panel1.PerformLayout();
        splMain.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splMain).EndInit();
        splMain.ResumeLayout(false);
        splResponse.Panel1.ResumeLayout(false);
        splResponse.Panel2.ResumeLayout(false);
        splResponse.Panel2.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)splResponse).EndInit();
        splResponse.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)dgvResponses).EndInit();
        ResumeLayout(false);
    }

    #endregion

    private Panel pnlLeft;
    private Label lblConnection;
    private Label lblSamples;
    private ListBox lstSamples;
    private CheckBox chkNowTime;
    private Button btnSend;
    private SplitContainer splMain;
    private Label lblRequest;
    private TextBox txtRequest;
    private SplitContainer splResponse;
    private DataGridView dgvResponses;
    private TextBox txtResponse;
}
