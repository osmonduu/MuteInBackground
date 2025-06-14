namespace MuteInBackground
{
    partial class Form1
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
            this.components = new System.ComponentModel.Container();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.lblStatus = new System.Windows.Forms.Label();
            this.lblMonitoredApps = new System.Windows.Forms.Label();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel3 = new System.Windows.Forms.FlowLayoutPanel();
            this.chkEnableAutoMute = new System.Windows.Forms.CheckBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lstApps = new System.Windows.Forms.ListBox();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnRemove = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel3.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.panel1.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(0, 10);
            this.lblStatus.Margin = new System.Windows.Forms.Padding(0, 10, 0, 0);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(60, 13);
            this.lblStatus.TabIndex = 1;
            this.lblStatus.Text = "Status: Idle";
            // 
            // lblMonitoredApps
            // 
            this.lblMonitoredApps.AutoSize = true;
            this.lblMonitoredApps.Location = new System.Drawing.Point(0, 38);
            this.lblMonitoredApps.Margin = new System.Windows.Forms.Padding(0, 15, 0, 0);
            this.lblMonitoredApps.Name = "lblMonitoredApps";
            this.lblMonitoredApps.Size = new System.Drawing.Size(84, 13);
            this.lblMonitoredApps.TabIndex = 5;
            this.lblMonitoredApps.Text = "Monitored Apps:";
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(61, 4);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel3, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel2, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel3, 0, 3);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(10);
            this.tableLayoutPanel1.RowCount = 4;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(326, 348);
            this.tableLayoutPanel1.TabIndex = 6;
            this.tableLayoutPanel1.Paint += new System.Windows.Forms.PaintEventHandler(this.tableLayoutPanel1_Paint);
            // 
            // flowLayoutPanel3
            // 
            this.flowLayoutPanel3.AutoSize = true;
            this.flowLayoutPanel3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel3.Controls.Add(this.lblStatus);
            this.flowLayoutPanel3.Controls.Add(this.lblMonitoredApps);
            this.flowLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel3.FlowDirection = System.Windows.Forms.FlowDirection.TopDown;
            this.flowLayoutPanel3.Location = new System.Drawing.Point(13, 42);
            this.flowLayoutPanel3.Name = "flowLayoutPanel3";
            this.flowLayoutPanel3.Padding = new System.Windows.Forms.Padding(0, 0, 0, 2);
            this.flowLayoutPanel3.Size = new System.Drawing.Size(300, 53);
            this.flowLayoutPanel3.TabIndex = 7;
            // 
            // chkEnableAutoMute
            // 
            this.chkEnableAutoMute.AutoSize = true;
            this.chkEnableAutoMute.Location = new System.Drawing.Point(94, 3);
            this.chkEnableAutoMute.Name = "chkEnableAutoMute";
            this.chkEnableAutoMute.Size = new System.Drawing.Size(111, 17);
            this.chkEnableAutoMute.TabIndex = 0;
            this.chkEnableAutoMute.Text = "Enable Auto-Mute";
            this.chkEnableAutoMute.UseVisualStyleBackColor = true;
            this.chkEnableAutoMute.CheckedChanged += new System.EventHandler(this.chkEnableAutoMute_CheckedChanged);
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.AutoSize = true;
            this.tableLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel2.ColumnCount = 3;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel2.Controls.Add(this.chkEnableAutoMute, 1, 0);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(13, 13);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 1;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Size = new System.Drawing.Size(300, 23);
            this.tableLayoutPanel2.TabIndex = 8;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.lstApps);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(13, 101);
            this.panel1.Name = "panel1";
            this.panel1.Padding = new System.Windows.Forms.Padding(10, 0, 10, 0);
            this.panel1.Size = new System.Drawing.Size(300, 205);
            this.panel1.TabIndex = 9;
            // 
            // lstApps
            // 
            this.lstApps.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstApps.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F);
            this.lstApps.FormattingEnabled = true;
            this.lstApps.ItemHeight = 20;
            this.lstApps.Location = new System.Drawing.Point(10, 0);
            this.lstApps.Name = "lstApps";
            this.lstApps.Size = new System.Drawing.Size(280, 205);
            this.lstApps.TabIndex = 3;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.AutoSize = true;
            this.tableLayoutPanel3.ColumnCount = 3;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Controls.Add(this.flowLayoutPanel2, 1, 0);
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.Location = new System.Drawing.Point(10, 309);
            this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(306, 29);
            this.tableLayoutPanel3.TabIndex = 10;
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.AutoSize = true;
            this.flowLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel2.Controls.Add(this.btnAdd);
            this.flowLayoutPanel2.Controls.Add(this.btnRemove);
            this.flowLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel2.Location = new System.Drawing.Point(58, 3);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(190, 23);
            this.flowLayoutPanel2.TabIndex = 11;
            this.flowLayoutPanel2.WrapContents = false;
            // 
            // btnAdd
            // 
            this.btnAdd.Location = new System.Drawing.Point(0, 0);
            this.btnAdd.Margin = new System.Windows.Forms.Padding(0, 0, 20, 0);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(75, 23);
            this.btnAdd.TabIndex = 3;
            this.btnAdd.Text = "Add";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnRemove
            // 
            this.btnRemove.Location = new System.Drawing.Point(115, 0);
            this.btnRemove.Margin = new System.Windows.Forms.Padding(20, 0, 0, 0);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(75, 23);
            this.btnRemove.TabIndex = 4;
            this.btnRemove.Text = "Remove";
            this.btnRemove.UseVisualStyleBackColor = true;
            this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(326, 348);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "Form1";
            this.Text = "MuteInBackground";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.flowLayoutPanel3.ResumeLayout(false);
            this.flowLayoutPanel3.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.flowLayoutPanel2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.Label lblMonitoredApps;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel3;
        private System.Windows.Forms.CheckBox chkEnableAutoMute;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ListBox lstApps;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnRemove;
    }
}

