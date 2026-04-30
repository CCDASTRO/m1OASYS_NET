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

            bool scopeSafe = false;
            bool.TryParse(
                p.GetValue(Id, "ScopeSafeEnabled", "", "False"),
                out scopeSafe);

            chkScopeSafe.Checked = scopeSafe;
        }

        private void cmdOK_Click(object sender, EventArgs e)
        {
            Profile p = new Profile();
            p.DeviceType = "Dome";

            p.WriteValue(Id, "IP", txtIP.Text);
            p.WriteValue(Id, "Port", txtPort.Text);
            p.WriteValue(Id, "ScopeSafeEnabled", chkScopeSafe.Checked.ToString());

            Close();
        }

        private void cmdCancel_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void InitializeComponent()
        {
            this.txtIP = new System.Windows.Forms.TextBox();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.chkScopeSafe = new System.Windows.Forms.CheckBox();
            this.btnOK = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // txtIP
            // 
            this.txtIP.Location = new System.Drawing.Point(52, 12);
            this.txtIP.Name = "txtIP";
            this.txtIP.Size = new System.Drawing.Size(105, 20);
            this.txtIP.TabIndex = 6;
            // 
            // txtPort
            // 
            this.txtPort.Location = new System.Drawing.Point(52, 38);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(105, 20);
            this.txtPort.TabIndex = 5;
            // 
            // chkScopeSafe
            // 
            this.chkScopeSafe.AutoSize = true;
            this.chkScopeSafe.Enabled = false;
            this.chkScopeSafe.Location = new System.Drawing.Point(52, 64);
            this.chkScopeSafe.Name = "chkScopeSafe";
            this.chkScopeSafe.Size = new System.Drawing.Size(82, 17);
            this.chkScopeSafe.TabIndex = 4;
            this.chkScopeSafe.Text = "Scope Safe";
            this.chkScopeSafe.Visible = false;
            // 
            // btnOK
            // 
            this.btnOK.Location = new System.Drawing.Point(12, 87);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "OK";
            this.btnOK.Click += new System.EventHandler(this.cmdOK_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(98, 87);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 23);
            this.btnCancel.TabIndex = 0;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.Click += new System.EventHandler(this.cmdCancel_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(17, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "IP";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 41);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(26, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Port";
            // 
            // SetupDialogForm
            // 
            this.ClientSize = new System.Drawing.Size(185, 117);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.chkScopeSafe);
            this.Controls.Add(this.txtPort);
            this.Controls.Add(this.txtIP);
            this.Name = "SetupDialogForm";
            this.Load += new System.EventHandler(this.SetupDialogForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private TextBox txtIP;
        private TextBox txtPort;
        private CheckBox chkScopeSafe;
        private Button btnOK;
        private Label label1;
        private Label label2;
        private Button btnCancel;
    }
}