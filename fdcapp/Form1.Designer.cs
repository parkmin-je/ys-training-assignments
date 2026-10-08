namespace fdcapp
{
    partial class Form1
    {
        /// <summary>
        ///  디자이너 변수 (필수)
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  사용 중인 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리 리소스를 해제할지 여부 (true면 해제)</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너 생성 코드

        /// <summary>
        ///  디자이너 지원용 필수 메서드 - 코드 편집기로 수정하지 마세요.
        ///  이 메서드의 내용은 디자이너가 자동 생성합니다.
        /// </summary>
        private void InitializeComponent()
        {
            lblEquipId = new Label();
            txtEquipId = new TextBox();
            lblEquipName = new Label();
            txtEquipName = new TextBox();
            lblLine = new Label();
            cboLine = new ComboBox();
            btnAdd = new Button();
            lblParam = new Label();
            cboParam = new ComboBox();
            lblFdcValue = new Label();
            txtFdcValue = new TextBox();
            btnFdcCheck = new Button();
            btnSimStart = new Button();
            btnSimStop = new Button();
            lblSimStatus = new Label();
            lblEquipmentList = new Label();
            dgvEquipment = new DataGridView();
            cboStatus = new ComboBox();
            btnChangeStatus = new Button();
            lblLogList = new Label();
            dgvLogs = new DataGridView();
            ((System.ComponentModel.ISupportInitialize)dgvEquipment).BeginInit();
            ((System.ComponentModel.ISupportInitialize)dgvLogs).BeginInit();
            SuspendLayout();
            // 
            // lblEquipId
            // 
            lblEquipId.AutoSize = true;
            lblEquipId.Location = new Point(12, 15);
            lblEquipId.Name = "lblEquipId";
            lblEquipId.Size = new Size(43, 15);
            lblEquipId.TabIndex = 0;
            lblEquipId.Text = "설비ID";
            // 
            // txtEquipId
            // 
            txtEquipId.Location = new Point(70, 12);
            txtEquipId.Name = "txtEquipId";
            txtEquipId.Size = new Size(120, 23);
            txtEquipId.TabIndex = 1;
            // 
            // lblEquipName
            // 
            lblEquipName.AutoSize = true;
            lblEquipName.Location = new Point(202, 15);
            lblEquipName.Name = "lblEquipName";
            lblEquipName.Size = new Size(43, 15);
            lblEquipName.TabIndex = 2;
            lblEquipName.Text = "설비명";
            // 
            // txtEquipName
            // 
            txtEquipName.Location = new Point(260, 12);
            txtEquipName.Name = "txtEquipName";
            txtEquipName.Size = new Size(150, 23);
            txtEquipName.TabIndex = 3;
            // 
            // lblLine
            // 
            lblLine.AutoSize = true;
            lblLine.Location = new Point(418, 15);
            lblLine.Name = "lblLine";
            lblLine.Size = new Size(31, 15);
            lblLine.TabIndex = 4;
            lblLine.Text = "라인";
            // 
            // cboLine
            // 
            cboLine.DropDownStyle = ComboBoxStyle.DropDownList;
            cboLine.Location = new Point(458, 12);
            cboLine.Name = "cboLine";
            cboLine.Size = new Size(100, 23);
            cboLine.TabIndex = 5;
            // 
            // btnAdd
            // 
            btnAdd.Location = new Point(570, 12);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(90, 23);
            btnAdd.TabIndex = 6;
            btnAdd.Text = "등록";
            btnAdd.UseVisualStyleBackColor = true;
            btnAdd.Click += btnAdd_Click;
            // 
            // lblParam
            // 
            lblParam.AutoSize = true;
            lblParam.Location = new Point(12, 43);
            lblParam.Name = "lblParam";
            lblParam.Size = new Size(55, 15);
            lblParam.TabIndex = 7;
            lblParam.Text = "파라미터";
            // 
            // cboParam
            // 
            cboParam.DropDownStyle = ComboBoxStyle.DropDownList;
            cboParam.Location = new Point(70, 40);
            cboParam.Name = "cboParam";
            cboParam.Size = new Size(120, 23);
            cboParam.TabIndex = 8;
            // 
            // lblFdcValue
            // 
            lblFdcValue.AutoSize = true;
            lblFdcValue.Location = new Point(202, 43);
            lblFdcValue.Name = "lblFdcValue";
            lblFdcValue.Size = new Size(43, 15);
            lblFdcValue.TabIndex = 9;
            lblFdcValue.Text = "수집값";
            // 
            // txtFdcValue
            // 
            txtFdcValue.Location = new Point(260, 40);
            txtFdcValue.Name = "txtFdcValue";
            txtFdcValue.Size = new Size(150, 23);
            txtFdcValue.TabIndex = 10;
            // 
            // btnFdcCheck
            // 
            btnFdcCheck.Location = new Point(418, 40);
            btnFdcCheck.Name = "btnFdcCheck";
            btnFdcCheck.Size = new Size(90, 23);
            btnFdcCheck.TabIndex = 11;
            btnFdcCheck.Text = "판정";
            btnFdcCheck.UseVisualStyleBackColor = true;
            btnFdcCheck.Click += btnFdcCheck_Click;
            // 
            // btnSimStart
            // 
            btnSimStart.Location = new Point(520, 40);
            btnSimStart.Name = "btnSimStart";
            btnSimStart.Size = new Size(110, 23);
            btnSimStart.TabIndex = 12;
            btnSimStart.Text = "시뮬레이터 시작";
            btnSimStart.UseVisualStyleBackColor = true;
            btnSimStart.Click += btnSimStart_Click;
            // 
            // btnSimStop
            // 
            btnSimStop.Enabled = false;
            btnSimStop.Location = new Point(640, 40);
            btnSimStop.Name = "btnSimStop";
            btnSimStop.Size = new Size(110, 23);
            btnSimStop.TabIndex = 13;
            btnSimStop.Text = "시뮬레이터 정지";
            btnSimStop.UseVisualStyleBackColor = true;
            btnSimStop.Click += btnSimStop_Click;
            // 
            // lblSimStatus
            // 
            lblSimStatus.AutoSize = true;
            lblSimStatus.Location = new Point(760, 43);
            lblSimStatus.Name = "lblSimStatus";
            lblSimStatus.Size = new Size(47, 15);
            lblSimStatus.TabIndex = 14;
            lblSimStatus.Text = "대기 중";
            // 
            // lblEquipmentList
            // 
            lblEquipmentList.AutoSize = true;
            lblEquipmentList.Location = new Point(12, 68);
            lblEquipmentList.Name = "lblEquipmentList";
            lblEquipmentList.Size = new Size(59, 15);
            lblEquipmentList.TabIndex = 15;
            lblEquipmentList.Text = "설비 목록";
            // 
            // dgvEquipment
            // 
            dgvEquipment.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvEquipment.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvEquipment.Location = new Point(12, 88);
            dgvEquipment.Name = "dgvEquipment";
            dgvEquipment.RowHeadersWidth = 51;
            dgvEquipment.Size = new Size(480, 360);
            dgvEquipment.TabIndex = 13;
            dgvEquipment.SelectionChanged += dgvEquipment_SelectionChanged;
            // 
            // cboStatus
            // 
            cboStatus.DropDownStyle = ComboBoxStyle.DropDownList;
            cboStatus.Location = new Point(12, 456);
            cboStatus.Name = "cboStatus";
            cboStatus.Size = new Size(100, 23);
            cboStatus.TabIndex = 14;
            // 
            // btnChangeStatus
            // 
            btnChangeStatus.Location = new Point(120, 456);
            btnChangeStatus.Name = "btnChangeStatus";
            btnChangeStatus.Size = new Size(100, 23);
            btnChangeStatus.TabIndex = 15;
            btnChangeStatus.Text = "상태변경";
            btnChangeStatus.UseVisualStyleBackColor = true;
            btnChangeStatus.Click += btnChangeStatus_Click;
            // 
            // lblLogList
            // 
            lblLogList.AutoSize = true;
            lblLogList.Location = new Point(504, 68);
            lblLogList.Name = "lblLogList";
            lblLogList.Size = new Size(59, 15);
            lblLogList.TabIndex = 16;
            lblLogList.Text = "설비 로그";
            // 
            // dgvLogs
            // 
            dgvLogs.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvLogs.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvLogs.Location = new Point(504, 88);
            dgvLogs.Name = "dgvLogs";
            dgvLogs.RowHeadersWidth = 51;
            dgvLogs.Size = new Size(484, 360);
            dgvLogs.TabIndex = 17;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1000, 500);
            Controls.Add(dgvLogs);
            Controls.Add(lblLogList);
            Controls.Add(btnChangeStatus);
            Controls.Add(cboStatus);
            Controls.Add(dgvEquipment);
            Controls.Add(lblEquipmentList);
            Controls.Add(lblSimStatus);
            Controls.Add(btnSimStop);
            Controls.Add(btnSimStart);
            Controls.Add(btnFdcCheck);
            Controls.Add(txtFdcValue);
            Controls.Add(lblFdcValue);
            Controls.Add(cboParam);
            Controls.Add(lblParam);
            Controls.Add(btnAdd);
            Controls.Add(cboLine);
            Controls.Add(lblLine);
            Controls.Add(txtEquipName);
            Controls.Add(lblEquipName);
            Controls.Add(txtEquipId);
            Controls.Add(lblEquipId);
            Name = "Form1";
            Text = "설비 관리";
            ((System.ComponentModel.ISupportInitialize)dgvEquipment).EndInit();
            ((System.ComponentModel.ISupportInitialize)dgvLogs).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblEquipId;
        private TextBox txtEquipId;
        private Label lblEquipName;
        private TextBox txtEquipName;
        private Label lblLine;
        private ComboBox cboLine;
        private Button btnAdd;
        private Label lblParam;
        private ComboBox cboParam;
        private Label lblFdcValue;
        private TextBox txtFdcValue;
        private Button btnFdcCheck;
        private Button btnSimStart;
        private Button btnSimStop;
        private Label lblSimStatus;
        private Label lblEquipmentList;
        private DataGridView dgvEquipment;
        private ComboBox cboStatus;
        private Button btnChangeStatus;
        private Label lblLogList;
        private DataGridView dgvLogs;
    }
}
