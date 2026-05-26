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
        private bool calibrationShown = false;
        public StatusForm()
        {
            InitializeComponent();
        }

        private void label2_Click(object sender, EventArgs e)
        {

        }

        private void timer1_Tick(
     object sender,
     EventArgs e)
        {
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
                    ? RoofTelemetry.FaultMessage
                    : "None";

            progressRoof.Value =
                Math.Max(
                    0,
                    Math.Min(
                        100,
                        RoofTelemetry.PercentOpen));

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
            // Calibration complete popup
            // ---------------------------------

            if (!RoofTelemetry.CalibrationMode &&
                RoofTelemetry.LastCalibrationValue > 0 &&
                !calibrationShown)
            {
                calibrationShown = true;

                MessageBox.Show(
                    "Calibration Complete\n\n"
                    + "Open Pulse Count = "
                    + RoofTelemetry.LastCalibrationValue,
                    "Calibration");
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
                        "m1OASYS_NET.Dome");

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

        private void lblWatchdog_Click(object sender, EventArgs e)
        {

        }
    }
}
