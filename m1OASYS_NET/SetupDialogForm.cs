using System;
using System.Windows.Forms;
using ASCOM.Utilities;

namespace m1OASYS_NET
{
    public partial class SetupDialogForm : Form
    {
        private const string Id = "ASCOM.m1OASYS_NET.Dome";

        public SetupDialogForm()
        {
            InitializeComponent();
        }

        private void SetupDialogForm_Load(object sender, EventArgs e)
        {
            Profile p = new Profile();
            p.DeviceType = "Dome";

            txtIP.Text = p.GetValue(Id, "IP", "", "");
            txtPort.Text = p.GetValue(Id, "Port", "", "");

            
            // -----------------------------
            // Logging option
            // -----------------------------
            bool enableLogging = false;
            bool.TryParse(
                p.GetValue(Id, "EnableLogging", "", "False"),
                out enableLogging);

            chkLogging.Checked = enableLogging;
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            Profile p = new Profile();
            p.DeviceType = "Dome";

            p.WriteValue(Id, "IP", txtIP.Text);
            p.WriteValue(Id, "Port", txtPort.Text);

            p.WriteValue(Id, "EnableLogging", chkLogging.Checked.ToString());

            Close();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        // -----------------------------
        // UI
        // -----------------------------
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SetupDialogForm));
            this.txtIP = new System.Windows.Forms.TextBox();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.chkLogging = new System.Windows.Forms.CheckBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // txtIP
            // 
            this.txtIP.Location = new System.Drawing.Point(61, 11);
            this.txtIP.Name = "txtIP";
            this.txtIP.Size = new System.Drawing.Size(110, 20);
            this.txtIP.TabIndex = 2;
            // 
            // txtPort
            // 
            this.txtPort.Location = new System.Drawing.Point(61, 37);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(110, 20);
            this.txtPort.TabIndex = 3;
            // 
            // chkLogging
            // 
            this.chkLogging.AutoSize = true;
            this.chkLogging.Location = new System.Drawing.Point(61, 63);
            this.chkLogging.Name = "chkLogging";
            this.chkLogging.Size = new System.Drawing.Size(100, 17);
            this.chkLogging.TabIndex = 5;
            this.chkLogging.Text = "Enable Logging";
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(185, 8);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(65, 23);
            this.btnOK.TabIndex = 6;
            this.btnOK.Text = "OK";
            this.btnOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(186, 37);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(64, 23);
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(24, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(17, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "IP";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(24, 40);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(26, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Port";
            // 
            // SetupDialogForm
            // 
            this.ClientSize = new System.Drawing.Size(261, 85);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtIP);
            this.Controls.Add(this.txtPort);
            this.Controls.Add(this.chkLogging);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "SetupDialogForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "m1 Setup";
            this.Load += new System.EventHandler(this.SetupDialogForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        // -----------------------------
        // Controls
        // -----------------------------
        private TextBox txtIP;
        private TextBox txtPort;
        private CheckBox chkLogging;
        private Button btnOK;
        private Button btnCancel;
        private Label label1;
        private Label label2;

        
    }
}