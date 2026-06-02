namespace m1OASYS_NET
{
    partial class StatusForm
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
            this.lblState = new System.Windows.Forms.Label();
            this.lblPercent = new System.Windows.Forms.Label();
            this.lblPulses = new System.Windows.Forms.Label();
            this.lblFault = new System.Windows.Forms.Label();
            this.progressRoof = new System.Windows.Forms.ProgressBar();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.btnCalibrate = new System.Windows.Forms.Button();
            this.lblLastFault = new System.Windows.Forms.Label();
            this.lblReconnect = new System.Windows.Forms.Label();
            this.lblWatchdog = new System.Windows.Forms.Label();
            this.lblMountSafe = new System.Windows.Forms.Label();
            this.btnMinimize = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lblState
            // 
            this.lblState.AutoSize = true;
            this.lblState.Location = new System.Drawing.Point(9, 7);
            this.lblState.Name = "lblState";
            this.lblState.Size = new System.Drawing.Size(32, 13);
            this.lblState.TabIndex = 0;
            this.lblState.Text = "State";
            // 
            // lblPercent
            // 
            this.lblPercent.AutoSize = true;
            this.lblPercent.Location = new System.Drawing.Point(203, 7);
            this.lblPercent.Name = "lblPercent";
            this.lblPercent.Size = new System.Drawing.Size(15, 13);
            this.lblPercent.TabIndex = 1;
            this.lblPercent.Text = "%";
            this.lblPercent.Click += new System.EventHandler(this.label2_Click);
            // 
            // lblPulses
            // 
            this.lblPulses.AutoSize = true;
            this.lblPulses.Location = new System.Drawing.Point(9, 52);
            this.lblPulses.Name = "lblPulses";
            this.lblPulses.Size = new System.Drawing.Size(38, 13);
            this.lblPulses.TabIndex = 2;
            this.lblPulses.Text = "Pulses";
            // 
            // lblFault
            // 
            this.lblFault.AutoSize = true;
            this.lblFault.Location = new System.Drawing.Point(124, 29);
            this.lblFault.Name = "lblFault";
            this.lblFault.Size = new System.Drawing.Size(30, 13);
            this.lblFault.TabIndex = 3;
            this.lblFault.Text = "Fault";
            this.lblFault.Click += new System.EventHandler(this.lblFault_Click);
            // 
            // progressRoof
            // 
            this.progressRoof.Location = new System.Drawing.Point(95, 7);
            this.progressRoof.Name = "progressRoof";
            this.progressRoof.Size = new System.Drawing.Size(102, 13);
            this.progressRoof.TabIndex = 4;
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Interval = 250;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // btnCalibrate
            // 
            this.btnCalibrate.Location = new System.Drawing.Point(42, 105);
            this.btnCalibrate.Name = "btnCalibrate";
            this.btnCalibrate.Size = new System.Drawing.Size(75, 23);
            this.btnCalibrate.TabIndex = 5;
            this.btnCalibrate.Text = "Calibrate";
            this.btnCalibrate.UseVisualStyleBackColor = true;
            this.btnCalibrate.Click += new System.EventHandler(this.btnCalibrate_Click);
            // 
            // lblLastFault
            // 
            this.lblLastFault.AutoSize = true;
            this.lblLastFault.Location = new System.Drawing.Point(124, 52);
            this.lblLastFault.Name = "lblLastFault";
            this.lblLastFault.Size = new System.Drawing.Size(53, 13);
            this.lblLastFault.TabIndex = 6;
            this.lblLastFault.Text = "Last Fault";
            // 
            // lblReconnect
            // 
            this.lblReconnect.AutoSize = true;
            this.lblReconnect.Location = new System.Drawing.Point(124, 75);
            this.lblReconnect.Name = "lblReconnect";
            this.lblReconnect.Size = new System.Drawing.Size(60, 13);
            this.lblReconnect.TabIndex = 7;
            this.lblReconnect.Text = "Reconnect";
            // 
            // lblWatchdog
            // 
            this.lblWatchdog.AutoSize = true;
            this.lblWatchdog.Location = new System.Drawing.Point(9, 75);
            this.lblWatchdog.Name = "lblWatchdog";
            this.lblWatchdog.Size = new System.Drawing.Size(57, 13);
            this.lblWatchdog.TabIndex = 8;
            this.lblWatchdog.Text = "Watchdog";
            this.lblWatchdog.Click += new System.EventHandler(this.lblWatchdog_Click);
            // 
            // lblMountSafe
            // 
            this.lblMountSafe.AutoSize = true;
            this.lblMountSafe.Location = new System.Drawing.Point(9, 29);
            this.lblMountSafe.Name = "lblMountSafe";
            this.lblMountSafe.Size = new System.Drawing.Size(37, 13);
            this.lblMountSafe.TabIndex = 9;
            this.lblMountSafe.Text = "Mount";
            // 
            // btnMinimize
            // 
            this.btnMinimize.Location = new System.Drawing.Point(159, 105);
            this.btnMinimize.Name = "btnMinimize";
            this.btnMinimize.Size = new System.Drawing.Size(75, 23);
            this.btnMinimize.TabIndex = 10;
            this.btnMinimize.Text = "Hide";
            this.btnMinimize.UseVisualStyleBackColor = true;
            this.btnMinimize.Click += new System.EventHandler(this.btnMinimize_Click);
            // 
            // StatusForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(280, 140);
            this.Controls.Add(this.btnMinimize);
            this.Controls.Add(this.lblMountSafe);
            this.Controls.Add(this.lblWatchdog);
            this.Controls.Add(this.lblReconnect);
            this.Controls.Add(this.lblLastFault);
            this.Controls.Add(this.btnCalibrate);
            this.Controls.Add(this.progressRoof);
            this.Controls.Add(this.lblFault);
            this.Controls.Add(this.lblPulses);
            this.Controls.Add(this.lblPercent);
            this.Controls.Add(this.lblState);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;
            this.Name = "StatusForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "m1 Roof Status";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.StatusForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label lblState;
        private System.Windows.Forms.Label lblPercent;
        private System.Windows.Forms.Label lblPulses;
        private System.Windows.Forms.Label lblFault;
        private System.Windows.Forms.ProgressBar progressRoof;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.Button btnCalibrate;
        private System.Windows.Forms.Label lblLastFault;
        private System.Windows.Forms.Label lblReconnect;
        private System.Windows.Forms.Label lblWatchdog;
        private System.Windows.Forms.Label lblMountSafe;
        private System.Windows.Forms.Button btnMinimize;
    }
}