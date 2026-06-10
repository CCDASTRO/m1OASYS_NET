using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace m1OASYS_NET
{
    public partial class StatusForm : Form
    {

        private bool allowClose = false;
        private bool calibrationShown = false;
        public StatusForm()
        {
            InitializeComponent();
        }
        public void ForceClose()
        {
            allowClose = true;
            Close();
        }
        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(
    object sender,
    EventArgs e)
        {
            if (IsDisposed ||
                !Visible)
            {
                return;
            }

            lblState.Text =
                "State: " +
                RoofTelemetry.ShutterState;

            lblPercent.Text =
                "Open: " +
                RoofTelemetry.PercentOpen +
                "%";

            lblPulses.Text =
                "Pulses: " +
                RoofTelemetry.CurrentPulseCount;

            lblFault.Text =
    RoofTelemetry.Faulted
        ? "Fault: " +
          RoofTelemetry.FaultMessage
        : "Fault: None";

            // ---------------------------------
            // Progress bar safety
            // ---------------------------------

            int percent =
                Math.Max(
                    0,
                    Math.Min(
                        100,
                        RoofTelemetry.PercentOpen));

            if (progressRoof.Value != percent)
            {
                progressRoof.Value = percent;
            }

            // ---------------------------------
            // Reconnect status
            // ---------------------------------

            lblReconnect.Text =
                "Reconnect: " +
                (
                    RoofTelemetry.LastReconnectTime ==
                    DateTime.MinValue
                        ? "Never"
                        : RoofTelemetry
                            .LastReconnectTime
                            .ToString("HH:mm:ss")
                );

            // ---------------------------------
            // Last fault
            // ---------------------------------

            lblLastFault.Text =
                "Last Fault: " +
                (
                    RoofTelemetry.LastFaultTime ==
                    DateTime.MinValue
                        ? "None"
                        : RoofTelemetry
                            .LastFaultTime
                            .ToString("HH:mm:ss")
                );

            // ---------------------------------
            // Watchdog status
            // ---------------------------------

            lblWatchdog.Text =
                "Watchdog: " +
                (
                    string.IsNullOrWhiteSpace(
                        RoofTelemetry.LastWatchdogEvent)
                        ? "None"
                        : RoofTelemetry.LastWatchdogEvent
                );

            // ---------------------------------
            // Mount safety status
            // ---------------------------------
            if (!RoofTelemetry.ScopeSafetyEnabled)
            {
                lblMountSafe.Text =
                    "Mount Safe: N/A";
            }
            else
            {
                switch (RoofTelemetry.ScopeSafety)
                {
                    case ScopeSafetyState.Safe:
                        lblMountSafe.Text =
                            "Mount Safe: YES";
                        break;

                    case ScopeSafetyState.NotSafe:
                        lblMountSafe.Text =
                            "Mount Safe: NO";
                        break;

                    default:
                        lblMountSafe.Text =
                            "Mount Safe: UNKNOWN";
                        break;
                }
            }

            // ---------------------------------
            // Calibration complete popup
            // ---------------------------------

            if (!RoofTelemetry.CalibrationMode &&
                RoofTelemetry.LastCalibrationValue > 0 &&
                !calibrationShown)
            {
                calibrationShown = true;

                MessageBox.Show(
                    "Calibration Complete\n\n" +
                    "Open Pulse Count = " +
                    RoofTelemetry.LastCalibrationValue,
                    "Calibration",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }
        private void btnCalibrate_Click(
    object sender,
    EventArgs e)
        {
            try
            {
                MessageBox.Show(
                    "Calibration will begin.\n\n"
                    + "Ensure roof is fully closed.");
                calibrationShown = false;

                RoofTelemetry.LastCalibrationValue = 0;
                RoofTelemetry.CalibrationMode = true;
                RoofTelemetry.CurrentPulseCount = 0;

                ASCOM.DriverAccess.Dome dome =
                    new ASCOM.DriverAccess.Dome(
                        "ASCOM.m1OASYS_NET.Dome");

                dome.OpenShutter();

                MessageBox.Show(
                    "Allow roof to fully open.\n\n"
                    + "Calibration will complete automatically.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        protected override void OnFormClosing(
    FormClosingEventArgs e)
        {
            if (!allowClose &&
                e.CloseReason ==
                CloseReason.UserClosing)
            {
                e.Cancel = true;

                this.WindowState =
                    FormWindowState.Minimized;

                return;
            }

            timer1.Enabled = false;

            base.OnFormClosing(e);
        }
        private void lblWatchdog_Click(object sender, EventArgs e)
        {

        }

        private void StatusForm_Load(object sender, EventArgs e)
        {

        }

        private void lblFault_Click(object sender, EventArgs e)
        {

        }

        private void btnMinimize_Click(object sender, EventArgs e)
        {
            this.WindowState =
                FormWindowState.Minimized;
        }
    }
}
