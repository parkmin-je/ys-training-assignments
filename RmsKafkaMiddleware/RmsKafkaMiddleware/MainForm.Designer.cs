namespace RmsKafkaMiddleware;

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
        pnlTop = new Panel();
        lblCount = new Label();
        lblStatus = new Label();
        btnStop = new Button();
        btnStart = new Button();
        splMain = new SplitContainer();
        dgvMessages = new DataGridView();
        splBottom = new SplitContainer();
        txtXml = new TextBox();
        lstLog = new ListBox();
        pnlTop.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)splMain).BeginInit();
        splMain.Panel1.SuspendLayout();
        splMain.Panel2.SuspendLayout();
        splMain.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)dgvMessages).BeginInit();
        ((System.ComponentModel.ISupportInitialize)splBottom).BeginInit();
        splBottom.Panel1.SuspendLayout();
        splBottom.Panel2.SuspendLayout();
        splBottom.SuspendLayout();
        SuspendLayout();
        //
        // pnlTop
        //
        pnlTop.Controls.Add(lblCount);
        pnlTop.Controls.Add(lblStatus);
        pnlTop.Controls.Add(btnStop);
        pnlTop.Controls.Add(btnStart);
        pnlTop.Dock = DockStyle.Top;
        pnlTop.Location = new Point(0, 0);
        pnlTop.Name = "pnlTop";
        pnlTop.Size = new Size(1184, 52);
        pnlTop.TabIndex = 0;
        //
        // btnStart
        //
        btnStart.Location = new Point(10, 10);
        btnStart.Name = "btnStart";
        btnStart.Size = new Size(110, 32);
        btnStart.TabIndex = 0;
        btnStart.Text = "수신 시작";
        btnStart.UseVisualStyleBackColor = true;
        btnStart.Click += btnStart_Click;
        //
        // btnStop
        //
        btnStop.Enabled = false;
        btnStop.Location = new Point(126, 10);
        btnStop.Name = "btnStop";
        btnStop.Size = new Size(110, 32);
        btnStop.TabIndex = 1;
        btnStop.Text = "수신 중지";
        btnStop.UseVisualStyleBackColor = true;
        btnStop.Click += btnStop_Click;
        //
        // lblStatus
        //
        lblStatus.AutoSize = true;
        lblStatus.Font = new Font("맑은 고딕", 10F, FontStyle.Bold);
        lblStatus.Location = new Point(252, 16);
        lblStatus.Name = "lblStatus";
        lblStatus.Size = new Size(52, 19);
        lblStatus.TabIndex = 2;
        lblStatus.Text = "■ 중지";
        //
        // lblCount
        //
        lblCount.AutoSize = true;
        lblCount.Location = new Point(380, 18);
        lblCount.Name = "lblCount";
        lblCount.Size = new Size(160, 15);
        lblCount.TabIndex = 3;
        lblCount.Text = "처리 0 · 성공 0 · 중복 0 · 오류 0";
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
        splMain.Panel1.Controls.Add(dgvMessages);
        //
        // splMain.Panel2
        //
        splMain.Panel2.Controls.Add(splBottom);
        splMain.Size = new Size(1184, 709);
        splMain.SplitterDistance = 330;
        splMain.TabIndex = 1;
        //
        // dgvMessages
        //
        dgvMessages.AllowUserToAddRows = false;
        dgvMessages.AllowUserToDeleteRows = false;
        dgvMessages.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
        dgvMessages.Dock = DockStyle.Fill;
        dgvMessages.Location = new Point(0, 0);
        dgvMessages.MultiSelect = false;
        dgvMessages.Name = "dgvMessages";
        dgvMessages.ReadOnly = true;
        dgvMessages.RowHeadersVisible = false;
        dgvMessages.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgvMessages.Size = new Size(1184, 330);
        dgvMessages.TabIndex = 0;
        dgvMessages.SelectionChanged += dgvMessages_SelectionChanged;
        //
        // splBottom
        //
        splBottom.Dock = DockStyle.Fill;
        splBottom.Location = new Point(0, 0);
        splBottom.Name = "splBottom";
        //
        // splBottom.Panel1
        //
        splBottom.Panel1.Controls.Add(txtXml);
        //
        // splBottom.Panel2
        //
        splBottom.Panel2.Controls.Add(lstLog);
        splBottom.Size = new Size(1184, 375);
        splBottom.SplitterDistance = 600;
        splBottom.TabIndex = 0;
        //
        // txtXml
        //
        txtXml.Dock = DockStyle.Fill;
        txtXml.Font = new Font("Consolas", 9F);
        txtXml.Location = new Point(0, 0);
        txtXml.Multiline = true;
        txtXml.Name = "txtXml";
        txtXml.ReadOnly = true;
        txtXml.ScrollBars = ScrollBars.Both;
        txtXml.Size = new Size(600, 375);
        txtXml.TabIndex = 0;
        txtXml.WordWrap = false;
        //
        // lstLog
        //
        lstLog.Dock = DockStyle.Fill;
        lstLog.Font = new Font("Consolas", 9F);
        lstLog.HorizontalScrollbar = true;
        lstLog.IntegralHeight = false;
        lstLog.Location = new Point(0, 0);
        lstLog.Name = "lstLog";
        lstLog.Size = new Size(580, 375);
        lstLog.TabIndex = 0;
        //
        // MainForm
        //
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1184, 761);
        Controls.Add(splMain);
        Controls.Add(pnlTop);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Kafka 기반 RMS Middleware — RMS.REQUEST → RMS.RESPONSE";
        FormClosing += MainForm_FormClosing;
        Load += MainForm_Load;
        pnlTop.ResumeLayout(false);
        pnlTop.PerformLayout();
        splMain.Panel1.ResumeLayout(false);
        splMain.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splMain).EndInit();
        splMain.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)dgvMessages).EndInit();
        splBottom.Panel1.ResumeLayout(false);
        splBottom.Panel1.PerformLayout();
        splBottom.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splBottom).EndInit();
        splBottom.ResumeLayout(false);
        ResumeLayout(false);
    }

    #endregion

    private Panel pnlTop;
    private Button btnStart;
    private Button btnStop;
    private Label lblStatus;
    private Label lblCount;
    private SplitContainer splMain;
    private DataGridView dgvMessages;
    private SplitContainer splBottom;
    private TextBox txtXml;
    private ListBox lstLog;
}
