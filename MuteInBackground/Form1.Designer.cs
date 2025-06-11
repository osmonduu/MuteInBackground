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
            this.chkEnableAutoMute = new System.Windows.Forms.CheckBox();
            this.lblStatus = new System.Windows.Forms.Label();
            this.lstApps = new System.Windows.Forms.ListBox();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnRemove = new System.Windows.Forms.Button();
            this.lblMonitoredApps = new System.Windows.Forms.Label();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.lblDebug = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // chkEnableAutoMute
            // 
            this.chkEnableAutoMute.AutoSize = true;
            this.chkEnableAutoMute.Location = new System.Drawing.Point(99, 12);
            this.chkEnableAutoMute.Name = "chkEnableAutoMute";
            this.chkEnableAutoMute.Size = new System.Drawing.Size(111, 17);
            this.chkEnableAutoMute.TabIndex = 0;
            this.chkEnableAutoMute.Text = "Enable Auto-Mute";
            this.chkEnableAutoMute.UseVisualStyleBackColor = true;
            this.chkEnableAutoMute.CheckedChanged += new System.EventHandler(this.chkEnableAutoMute_CheckedChanged);
            // 
            // lblStatus
            // 
            this.lblStatus.AutoSize = true;
            this.lblStatus.Location = new System.Drawing.Point(23, 45);
            this.lblStatus.Name = "lblStatus";
            this.lblStatus.Size = new System.Drawing.Size(60, 13);
            this.lblStatus.TabIndex = 1;
            this.lblStatus.Text = "Status: Idle";
            // 
            // lstApps
            // 
            this.lstApps.FormattingEnabled = true;
            this.lstApps.Location = new System.Drawing.Point(26, 98);
            this.lstApps.Name = "lstApps";
            this.lstApps.Size = new System.Drawing.Size(250, 147);
            this.lstApps.TabIndex = 2;
            // 
            // btnAdd
            // 
            this.btnAdd.Location = new System.Drawing.Point(49, 259);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(75, 23);
            this.btnAdd.TabIndex = 3;
            this.btnAdd.Text = "Add";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnRemove
            // 
            this.btnRemove.Location = new System.Drawing.Point(177, 259);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(75, 23);
            this.btnRemove.TabIndex = 4;
            this.btnRemove.Text = "Remove";
            this.btnRemove.UseVisualStyleBackColor = true;
            this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
            // 
            // lblMonitoredApps
            // 
            this.lblMonitoredApps.AutoSize = true;
            this.lblMonitoredApps.Location = new System.Drawing.Point(23, 82);
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
            // lblDebug
            // 
            this.lblDebug.AutoSize = true;
            this.lblDebug.Location = new System.Drawing.Point(23, 292);
            this.lblDebug.Name = "lblDebug";
            this.lblDebug.Size = new System.Drawing.Size(45, 13);
            this.lblDebug.TabIndex = 7;
            this.lblDebug.Text = "DEBUG";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(314, 314);
            this.Controls.Add(this.lblDebug);
            this.Controls.Add(this.lblMonitoredApps);
            this.Controls.Add(this.btnRemove);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.lstApps);
            this.Controls.Add(this.lblStatus);
            this.Controls.Add(this.chkEnableAutoMute);
            this.Name = "Form1";
            this.Text = "MuteInBackground";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.CheckBox chkEnableAutoMute;
        private System.Windows.Forms.Label lblStatus;
        private System.Windows.Forms.ListBox lstApps;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnRemove;
        private System.Windows.Forms.Label lblMonitoredApps;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.Label lblDebug;
    }
}

