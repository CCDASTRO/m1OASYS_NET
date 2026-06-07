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
            cboConnectionMethod.Text = p.GetValue(Id, "ConnectionMethod", "", "TCP/IP");

            cboComPort.Items.Clear();

            cboComPort.Items.AddRange(
                System.IO.Ports.SerialPort.GetPortNames());

            string savedPort =
                p.GetValue(
                    Id,
                    "ComPort",
                    "",
                    "COM1");

            if (cboComPort.Items.Contains(savedPort))
            {
                cboComPort.SelectedItem =
                    savedPort;
            }
            else if (cboComPort.Items.Count > 0)
            {
                cboComPort.SelectedIndex = 0;
            }
            UpdateConnectionControls();

            // -----------------------------
            // Hall Pulse Telemetry option
            // -----------------------------
            bool pulseEnabled;

            bool.TryParse(
                p.GetValue(Id, "UsePulseTelemetry", "", "False"), out pulseEnabled);

            chkPulseTelemetry.Checked = pulseEnabled;

            bool scopeSafety;

            bool.TryParse(p.GetValue(Id, "UseScopeSafety", "", "False"), out scopeSafety);

            chkScopeSafety.Checked =
                scopeSafety;

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

            p.WriteValue(Id, "ConnectionMethod", cboConnectionMethod.Text);

            p.WriteValue(Id, "ComPort", cboComPort.Text);

            p.WriteValue(Id, "IP", txtIP.Text);
            p.WriteValue(Id, "Port", txtPort.Text);

            p.WriteValue(Id, "EnableLogging", chkLogging.Checked.ToString());
            p.WriteValue(Id, "UsePulseTelemetry", chkPulseTelemetry.Checked.ToString());
            p.WriteValue(Id, "UseScopeSafety", chkScopeSafety.Checked.ToString());
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
            this.lblIP = new System.Windows.Forms.Label();
            this.lblPort = new System.Windows.Forms.Label();
            this.chkPulseTelemetry = new System.Windows.Forms.CheckBox();
            this.chkScopeSafety = new System.Windows.Forms.CheckBox();
            this.cboConnectionMethod = new System.Windows.Forms.ComboBox();
            this.cboComPort = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.lblComPort = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // txtIP
            // 
            this.txtIP.Location = new System.Drawing.Point(66, 64);
            this.txtIP.Name = "txtIP";
            this.txtIP.Size = new System.Drawing.Size(110, 20);
            this.txtIP.TabIndex = 2;
            // 
            // txtPort
            // 
            this.txtPort.Location = new System.Drawing.Point(66, 90);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(110, 20);
            this.txtPort.TabIndex = 3;
            // 
            // chkLogging
            // 
            this.chkLogging.AutoSize = true;
            this.chkLogging.Location = new System.Drawing.Point(15, 116);
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
            // lblIP
            // 
            this.lblIP.AutoSize = true;
            this.lblIP.Location = new System.Drawing.Point(12, 67);
            this.lblIP.Name = "lblIP";
            this.lblIP.Size = new System.Drawing.Size(17, 13);
            this.lblIP.TabIndex = 0;
            this.lblIP.Text = "IP";
            // 
            // lblPort
            // 
            this.lblPort.AutoSize = true;
            this.lblPort.Location = new System.Drawing.Point(12, 93);
            this.lblPort.Name = "lblPort";
            this.lblPort.Size = new System.Drawing.Size(39, 13);
            this.lblPort.TabIndex = 1;
            this.lblPort.Text = "IP Port";
            // 
            // chkPulseTelemetry
            // 
            this.chkPulseTelemetry.AutoSize = true;
            this.chkPulseTelemetry.Location = new System.Drawing.Point(139, 119);
            this.chkPulseTelemetry.Name = "chkPulseTelemetry";
            this.chkPulseTelemetry.Size = new System.Drawing.Size(93, 17);
            this.chkPulseTelemetry.TabIndex = 8;
            this.chkPulseTelemetry.Text = "Hall Telemetry";
            this.chkPulseTelemetry.UseVisualStyleBackColor = true;
            // 
            // chkScopeSafety
            // 
            this.chkScopeSafety.AutoSize = true;
            this.chkScopeSafety.Location = new System.Drawing.Point(15, 139);
            this.chkScopeSafety.Name = "chkScopeSafety";
            this.chkScopeSafety.Size = new System.Drawing.Size(161, 17);
            this.chkScopeSafety.TabIndex = 9;
            this.chkScopeSafety.Text = "Enable Mount Safety Sensor";
            this.chkScopeSafety.UseVisualStyleBackColor = true;
            // 
            // cboConnectionMethod
            // 
            this.cboConnectionMethod.FormattingEnabled = true;
            this.cboConnectionMethod.Items.AddRange(new object[] {
            "TCP/IP",
            "Serial"});
            this.cboConnectionMethod.Location = new System.Drawing.Point(68, 12);
            this.cboConnectionMethod.Name = "cboConnectionMethod";
            this.cboConnectionMethod.Size = new System.Drawing.Size(110, 21);
            this.cboConnectionMethod.TabIndex = 10;
            this.cboConnectionMethod.Text = "TCP/IP";
            this.cboConnectionMethod.SelectedIndexChanged += new System.EventHandler(this.cboConnectionMethod_SelectedIndexChanged);
            // 
            // cboComPort
            // 
            this.cboComPort.FormattingEnabled = true;
            this.cboComPort.Location = new System.Drawing.Point(68, 37);
            this.cboComPort.Name = "cboComPort";
            this.cboComPort.Size = new System.Drawing.Size(110, 21);
            this.cboComPort.TabIndex = 11;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 15);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(43, 13);
            this.label3.TabIndex = 12;
            this.label3.Text = "Method";
            // 
            // lblComPort
            // 
            this.lblComPort.AutoSize = true;
            this.lblComPort.Location = new System.Drawing.Point(12, 42);
            this.lblComPort.Name = "lblComPort";
            this.lblComPort.Size = new System.Drawing.Size(50, 13);
            this.lblComPort.TabIndex = 13;
            this.lblComPort.Text = "Com Port";
            this.lblComPort.Click += new System.EventHandler(this.label4_Click);
            // 
            // SetupDialogForm
            // 
            this.ClientSize = new System.Drawing.Size(261, 165);
            this.Controls.Add(this.lblComPort);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.cboComPort);
            this.Controls.Add(this.cboConnectionMethod);
            this.Controls.Add(this.chkScopeSafety);
            this.Controls.Add(this.chkPulseTelemetry);
            this.Controls.Add(this.lblIP);
            this.Controls.Add(this.lblPort);
            this.Controls.Add(this.txtIP);
            this.Controls.Add(this.txtPort);
            this.Controls.Add(this.chkLogging);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
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
        private Label lblIP;
        private CheckBox chkPulseTelemetry;
        private CheckBox chkScopeSafety;
        private ComboBox cboConnectionMethod;
        private ComboBox cboComPort;
        private Label label3;
        private Label lblComPort;
        private Label lblPort;

        private void UpdateConnectionControls()
        {
            bool useSerial =
               cboConnectionMethod.Text.Trim()
               .StartsWith(
               "Serial",
               StringComparison.OrdinalIgnoreCase);

            lblComPort.Enabled =
                useSerial;

            cboComPort.Enabled =
                useSerial;

            lblIP.Enabled =
                !useSerial;

            txtIP.Enabled =
                !useSerial;

            lblPort.Enabled =
                !useSerial;

            txtPort.Enabled =
                !useSerial;
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void cboConnectionMethod_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateConnectionControls();
        }
    }
}