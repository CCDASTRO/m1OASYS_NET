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

            chkEnablePushover.Checked =
                Convert.ToBoolean(
                    p.GetValue(
                        Id,
                        "EnablePushover",
                        "",
                        "False"));

            txtPushoverToken.Text =
                p.GetValue(
                    Id,
                    "PushoverToken",
                    "",
                    "");

            txtPushoverUserKey.Text =
                p.GetValue(
                    Id,
                    "PushoverUserKey",
                    "",
                    "");


            chkNotifyRoofOpened.Checked =
                Convert.ToBoolean(
                    p.GetValue(
                        Id,
                        "NotifyRoofOpened",
                        "",
                        "True"));

            chkNotifyRoofClosed.Checked =
                Convert.ToBoolean(
                    p.GetValue(
                        Id,
                        "NotifyRoofClosed",
                        "",
                        "True"));

            chkNotifyRoofFault.Checked =
                Convert.ToBoolean(
                    p.GetValue(
                        Id,
                        "NotifyRoofFault",
                        "",
                        "True"));

            chkNotifyConnectionLost.Checked =
                Convert.ToBoolean(
                    p.GetValue(
                        Id,
                        "NotifyConnectionLost",
                        "",
                        "True"));

            chkNotifyConnectionRestored.Checked =
                Convert.ToBoolean(
                    p.GetValue(
                        Id,
                        "NotifyConnectionRestored",
                        "",
                        "True"));

            chkNotifyScopeBlocked.Checked =
                Convert.ToBoolean(
                    p.GetValue(
                        Id,
                        "NotifyScopeBlocked",
                        "",
                        "True"));

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

            p.WriteValue(
                Id,
                "EnablePushover",
                chkEnablePushover.Checked.ToString());

            p.WriteValue(
                Id,
                "PushoverToken",
                txtPushoverToken.Text.Trim());

            p.WriteValue(
                Id,
                "PushoverUserKey",
                txtPushoverUserKey.Text.Trim());
                p.WriteValue(
                Id,
                "NotifyRoofOpened",
                chkNotifyRoofOpened.Checked.ToString());

            p.WriteValue(
                Id,
                "NotifyRoofClosed",
                chkNotifyRoofClosed.Checked.ToString());

            p.WriteValue(
                Id,
                "NotifyRoofFault",
                chkNotifyRoofFault.Checked.ToString());

            p.WriteValue(
                Id,
                "NotifyConnectionLost",
                chkNotifyConnectionLost.Checked.ToString());

            p.WriteValue(
                Id,
                "NotifyConnectionRestored",
                chkNotifyConnectionRestored.Checked.ToString());

            p.WriteValue(
                Id,
                "NotifyScopeBlocked",
                chkNotifyScopeBlocked.Checked.ToString());
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
            this.chkEnablePushover = new System.Windows.Forms.CheckBox();
            this.txtPushoverToken = new System.Windows.Forms.TextBox();
            this.txtPushoverUserKey = new System.Windows.Forms.TextBox();
            this.btnTestPushover = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.chkNotifyRoofOpened = new System.Windows.Forms.CheckBox();
            this.chkNotifyRoofClosed = new System.Windows.Forms.CheckBox();
            this.chkNotifyRoofFault = new System.Windows.Forms.CheckBox();
            this.chkNotifyConnectionLost = new System.Windows.Forms.CheckBox();
            this.chkNotifyConnectionRestored = new System.Windows.Forms.CheckBox();
            this.chkNotifyScopeBlocked = new System.Windows.Forms.CheckBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label4 = new System.Windows.Forms.Label();
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
            this.chkPulseTelemetry.Location = new System.Drawing.Point(126, 116);
            this.chkPulseTelemetry.Name = "chkPulseTelemetry";
            this.chkPulseTelemetry.Size = new System.Drawing.Size(130, 17);
            this.chkPulseTelemetry.TabIndex = 8;
            this.chkPulseTelemetry.Text = "Enable Motion Sensor";
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
            // chkEnablePushover
            // 
            this.chkEnablePushover.AutoSize = true;
            this.chkEnablePushover.Location = new System.Drawing.Point(15, 202);
            this.chkEnablePushover.Name = "chkEnablePushover";
            this.chkEnablePushover.Size = new System.Drawing.Size(116, 17);
            this.chkEnablePushover.TabIndex = 14;
            this.chkEnablePushover.Text = "Enable Pusherover";
            this.chkEnablePushover.UseVisualStyleBackColor = true;
            // 
            // txtPushoverToken
            // 
            this.txtPushoverToken.Location = new System.Drawing.Point(64, 321);
            this.txtPushoverToken.Name = "txtPushoverToken";
            this.txtPushoverToken.Size = new System.Drawing.Size(183, 20);
            this.txtPushoverToken.TabIndex = 15;
            // 
            // txtPushoverUserKey
            // 
            this.txtPushoverUserKey.Location = new System.Drawing.Point(63, 347);
            this.txtPushoverUserKey.Name = "txtPushoverUserKey";
            this.txtPushoverUserKey.Size = new System.Drawing.Size(184, 20);
            this.txtPushoverUserKey.TabIndex = 16;
            // 
            // btnTestPushover
            // 
            this.btnTestPushover.Location = new System.Drawing.Point(66, 384);
            this.btnTestPushover.Name = "btnTestPushover";
            this.btnTestPushover.Size = new System.Drawing.Size(119, 23);
            this.btnTestPushover.TabIndex = 17;
            this.btnTestPushover.Text = "Test Pushover";
            this.btnTestPushover.UseVisualStyleBackColor = true;
            this.btnTestPushover.Click += new System.EventHandler(this.btnTestPushover_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(17, 324);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 13);
            this.label1.TabIndex = 18;
            this.label1.Text = "Token";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(19, 350);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(25, 13);
            this.label2.TabIndex = 19;
            this.label2.Text = "Key";
            // 
            // chkNotifyRoofOpened
            // 
            this.chkNotifyRoofOpened.AutoSize = true;
            this.chkNotifyRoofOpened.Location = new System.Drawing.Point(16, 225);
            this.chkNotifyRoofOpened.Name = "chkNotifyRoofOpened";
            this.chkNotifyRoofOpened.Size = new System.Drawing.Size(120, 17);
            this.chkNotifyRoofOpened.TabIndex = 20;
            this.chkNotifyRoofOpened.Text = "Notify Roof Opened";
            this.chkNotifyRoofOpened.UseVisualStyleBackColor = true;
            // 
            // chkNotifyRoofClosed
            // 
            this.chkNotifyRoofClosed.AutoSize = true;
            this.chkNotifyRoofClosed.Location = new System.Drawing.Point(139, 225);
            this.chkNotifyRoofClosed.Name = "chkNotifyRoofClosed";
            this.chkNotifyRoofClosed.Size = new System.Drawing.Size(114, 17);
            this.chkNotifyRoofClosed.TabIndex = 21;
            this.chkNotifyRoofClosed.Text = "Notify Roof Closed";
            this.chkNotifyRoofClosed.UseVisualStyleBackColor = true;
            // 
            // chkNotifyRoofFault
            // 
            this.chkNotifyRoofFault.AutoSize = true;
            this.chkNotifyRoofFault.Location = new System.Drawing.Point(16, 248);
            this.chkNotifyRoofFault.Name = "chkNotifyRoofFault";
            this.chkNotifyRoofFault.Size = new System.Drawing.Size(105, 17);
            this.chkNotifyRoofFault.TabIndex = 22;
            this.chkNotifyRoofFault.Text = "Notify Roof Fault";
            this.chkNotifyRoofFault.UseVisualStyleBackColor = true;
            // 
            // chkNotifyConnectionLost
            // 
            this.chkNotifyConnectionLost.AutoSize = true;
            this.chkNotifyConnectionLost.Location = new System.Drawing.Point(16, 271);
            this.chkNotifyConnectionLost.Name = "chkNotifyConnectionLost";
            this.chkNotifyConnectionLost.Size = new System.Drawing.Size(151, 17);
            this.chkNotifyConnectionLost.TabIndex = 23;
            this.chkNotifyConnectionLost.Text = "Notify Communication Lost";
            this.chkNotifyConnectionLost.UseVisualStyleBackColor = true;
            // 
            // chkNotifyConnectionRestored
            // 
            this.chkNotifyConnectionRestored.AutoSize = true;
            this.chkNotifyConnectionRestored.Location = new System.Drawing.Point(16, 294);
            this.chkNotifyConnectionRestored.Name = "chkNotifyConnectionRestored";
            this.chkNotifyConnectionRestored.Size = new System.Drawing.Size(174, 17);
            this.chkNotifyConnectionRestored.TabIndex = 24;
            this.chkNotifyConnectionRestored.Text = "Notify Communication Restored";
            this.chkNotifyConnectionRestored.UseVisualStyleBackColor = true;
            // 
            // chkNotifyScopeBlocked
            // 
            this.chkNotifyScopeBlocked.AutoSize = true;
            this.chkNotifyScopeBlocked.Location = new System.Drawing.Point(139, 248);
            this.chkNotifyScopeBlocked.Name = "chkNotifyScopeBlocked";
            this.chkNotifyScopeBlocked.Size = new System.Drawing.Size(124, 17);
            this.chkNotifyScopeBlocked.TabIndex = 25;
            this.chkNotifyScopeBlocked.Text = "Notify Scope Unsafe";
            this.chkNotifyScopeBlocked.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.ScrollBar;
            this.panel1.Location = new System.Drawing.Point(4, 186);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(259, 2);
            this.panel1.TabIndex = 26;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(77, 170);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(113, 13);
            this.label4.TabIndex = 27;
            this.label4.Text = "Pushover Notifications";
            // 
            // SetupDialogForm
            // 
            this.ClientSize = new System.Drawing.Size(268, 420);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.chkNotifyScopeBlocked);
            this.Controls.Add(this.chkNotifyConnectionRestored);
            this.Controls.Add(this.chkNotifyConnectionLost);
            this.Controls.Add(this.chkNotifyRoofFault);
            this.Controls.Add(this.chkNotifyRoofClosed);
            this.Controls.Add(this.chkNotifyRoofOpened);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnTestPushover);
            this.Controls.Add(this.txtPushoverUserKey);
            this.Controls.Add(this.txtPushoverToken);
            this.Controls.Add(this.chkEnablePushover);
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
        private CheckBox chkEnablePushover;
        private TextBox txtPushoverToken;
        private TextBox txtPushoverUserKey;
        private Button btnTestPushover;
        private Label label1;
        private Label label2;
        private CheckBox chkNotifyRoofOpened;
        private CheckBox chkNotifyRoofClosed;
        private CheckBox chkNotifyRoofFault;
        private CheckBox chkNotifyConnectionLost;
        private CheckBox chkNotifyConnectionRestored;
        private CheckBox chkNotifyScopeBlocked;
        private Panel panel1;
        private Label label4;
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

        private async void btnTestPushover_Click(
    object sender,
    EventArgs e)
        {
            
            bool ok =
                await PushoverNotifier.SendAsync(
                    txtPushoverToken.Text.Trim(),
                    txtPushoverUserKey.Text.Trim(),
                    "m1OASYS test notification");

            MessageBox.Show(
                ok
                    ? "Notification sent."
                    : "Notification failed.");
        }
    }
}