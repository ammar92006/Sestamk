namespace Sestamk.Forms
{
    partial class frmUpdate
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            lblTitle = new Label();
            lblStatus = new Label();
            txtChangeLog = new TextBox();
            lblFileSize = new Label();
            progressBar1 = new ProgressBar();
            btnDownload = new Button();
            btnClose = new Button();
            SuspendLayout();
            // 
            // lblTitle
            // 
            lblTitle.Font = new Font("Segoe UI", 20F);
            lblTitle.Location = new Point(391, 9);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(151, 43);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "label1";
            // 
            // lblStatus
            // 
            lblStatus.Font = new Font("Segoe UI", 20F);
            lblStatus.Location = new Point(627, 77);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(151, 43);
            lblStatus.TabIndex = 1;
            lblStatus.Text = "label1";
            // 
            // txtChangeLog
            // 
            txtChangeLog.Location = new Point(595, 136);
            txtChangeLog.Multiline = true;
            txtChangeLog.Name = "txtChangeLog";
            txtChangeLog.Size = new Size(183, 93);
            txtChangeLog.TabIndex = 2;
            // 
            // lblFileSize
            // 
            lblFileSize.Font = new Font("Segoe UI", 20F);
            lblFileSize.Location = new Point(627, 253);
            lblFileSize.Name = "lblFileSize";
            lblFileSize.Size = new Size(151, 43);
            lblFileSize.TabIndex = 3;
            lblFileSize.Text = "label1";
            // 
            // progressBar1
            // 
            progressBar1.Location = new Point(189, 318);
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new Size(477, 23);
            progressBar1.TabIndex = 4;
            // 
            // btnDownload
            // 
            btnDownload.Location = new Point(534, 391);
            btnDownload.Name = "btnDownload";
            btnDownload.Size = new Size(75, 23);
            btnDownload.TabIndex = 5;
            btnDownload.Text = "button1";
            btnDownload.UseVisualStyleBackColor = true;
            btnDownload.Click += btnDownload_Click;
            // 
            // btnClose
            // 
            btnClose.Location = new Point(12, 12);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(75, 23);
            btnClose.TabIndex = 6;
            btnClose.Text = "button1";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;
            // 
            // frmUpdate
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(800, 450);
            Controls.Add(btnClose);
            Controls.Add(btnDownload);
            Controls.Add(progressBar1);
            Controls.Add(lblFileSize);
            Controls.Add(txtChangeLog);
            Controls.Add(lblStatus);
            Controls.Add(lblTitle);
            Name = "frmUpdate";
            Text = "frmUpdate";
            Load += frmUpdate_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Label lblTitle;
        private Label lblStatus;
        private TextBox txtChangeLog;
        private Label lblFileSize;
        private ProgressBar progressBar1;
        private Button btnDownload;
        private Button btnClose;
    }
}