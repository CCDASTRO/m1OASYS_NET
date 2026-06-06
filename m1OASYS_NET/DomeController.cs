using ASCOM;
using ASCOM.DeviceInterface;
using ASCOM.Utilities;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace m1OASYS_NET
{
    public class DomeController
    {
        private TcpClient client;
        private NetworkStream stream;
        private bool usePulseTelemetry;
        private Thread rxThread;
        private Thread verifyThread;
        private Thread telemetryThread;
        private volatile bool running;
        private bool useScopeSafety;
        private readonly object ioLock = new object();
        private readonly object stateLock = new object();

        private bool connected;
        

        private ShutterState shutterState = ShutterState.shutterClosed;

        private DateTime lastRealTelemetry = DateTime.MinValue;

        // ---------------- VERIFY MODE ----------------
        private volatile bool verifyMode = false;
        private DateTime verifyStart;
        private const int VERIFY_TIMEOUT_MS = 90000;

        private string lastFrame = "";

        private TraceLogger log;

        public DomeController()
        {
        //===================================================
        // Tracelogging
        //===================================================
            bool enableLogging = false;

            try
            {
                Profile p = new Profile();
                p.DeviceType = "Dome";

                bool.TryParse(
                    p.GetValue("ASCOM.m1OASYS_NET.Dome", "EnableLogging", "", "False"),
                    out enableLogging);
            }
            catch
            {
                enableLogging = false; // fail safe OFF
            }

            log = new TraceLogger("", "DomeController")
            {
                Enabled = enableLogging
            };
        }

        // =====================================================
        // CONNECT
        // =====================================================

        public void Connect(string ip, int port, bool enablePulseTelemetry, bool enableScopeSafety)
        {
            

            client = new TcpClient
            {
                ReceiveTimeout = 3000,
                SendTimeout = 3000
            };

            client.Connect(ip, port);
            stream = client.GetStream();
            usePulseTelemetry = enablePulseTelemetry;
            useScopeSafety = enableScopeSafety;
            RoofTelemetry.ScopeSafetyEnabled = enableScopeSafety;
            running = true;

            rxThread = new Thread(RxLoop) { IsBackground = true };
            rxThread.Start();

            verifyThread = new Thread(VerifyLoop) { IsBackground = true };
            verifyThread.Start();
            telemetryThread = new Thread(TelemetryLoop)
    {
        IsBackground = true
    };

            telemetryThread.Start();
            connected = true;
            RoofTelemetry.LastReconnectTime = DateTime.Now;
            log.LogMessage("Connect", "Connected successfully.");

        // =====================================================
        // FORCE INITIAL STATE QUERY
        // =====================================================
            Thread.Sleep(300);
            SendRaw("xx00100");
        }

        // =====================================================
        // DISCONNECT
        // =====================================================

        public void Disconnect()
        {
            running = false;

            connected = false;

            // ---------------------------------
            // Force socket shutdown FIRST
            // ---------------------------------

            try
            {
                stream?.Close();
            }
            catch
            {
            }

            try
            {
                client?.Close();
            }
            catch
            {
            }

            // ---------------------------------
            // Stop worker threads
            // ---------------------------------

            try
            {
                if (rxThread != null &&
                    rxThread.IsAlive)
                {
                    rxThread.Join(1000);
                }
            }
            catch
            {
            }

            try
            {
                if (verifyThread != null &&
                    verifyThread.IsAlive)
                {
                    verifyThread.Join(1000);
                }
            }
            catch
            {
            }

            try
            {
                if (telemetryThread != null &&
                    telemetryThread.IsAlive)
                {
                    telemetryThread.Join(1000);
                }
            }
            catch
            {
            }

            RoofTelemetry.Moving =
                false;
        }

        // =====================================================
        // RX LOOP
        // =====================================================

        private void RxLoop()
        {
            byte[] buffer =
                new byte[1024];

            var sb =
                new StringBuilder();

            while (running)
            {
                try
                {
                    // -----------------------------
                    // Validate connection
                    // -----------------------------

                    if (client == null ||
                        !client.Connected ||
                        stream == null)
                    {
                        break;
                    }

                    // -----------------------------
                    // Read incoming data
                    // -----------------------------

                    if (stream.DataAvailable)
                    {
                        int len;

                        lock (ioLock)
                        {
                            len =
                                stream.Read(
                                    buffer,
                                    0,
                                    buffer.Length);
                        }

                        // Remote disconnect
                        if (len <= 0)
                        {
                            break;
                        }

                        sb.Append(
                            Encoding.ASCII.GetString(
                                buffer,
                                0,
                                len));

                        Process(sb.ToString());

                        sb.Clear();
                    }
                    else
                    {
                        Thread.Sleep(20);
                    }
                }
                catch (ObjectDisposedException)
                {
                    // Expected during disconnect
                    break;
                }
                catch (IOException)
                {
                    // Socket closed
                    break;
                }
                catch (Exception ex)
                {
                    log.LogMessage(
                        "RX",
                        ex.Message);

                    break;
                }
            }

            log.LogMessage(
                "RX",
                "RX thread exited");
        }

        // =====================================================
        // VERIFY LOOP (COMMAND CONFIRMATION ONLY)
        // =====================================================

        private void VerifyLoop()
        {
            while (running)
            {
                Thread.Sleep(500);

                if (!verifyMode)
                    continue;

                // ---------------------------------
                // Movement timeout protection
                // ---------------------------------

                if ((DateTime.Now - verifyStart)
                    .TotalMilliseconds >
                    VERIFY_TIMEOUT_MS)
                {
                    lock (stateLock)
                    {
                        shutterState =
                            ShutterState.shutterError;

                        RoofTelemetry.ShutterState =
                            "Error";

                        RoofTelemetry.Faulted =
                            true;

                        RoofTelemetry.FaultMessage =
                            "Movement timeout";
                        RoofTelemetry.LastFaultTime = DateTime.Now;

                        RoofTelemetry.LastWatchdogEvent =
                            "Movement timeout";
                        try
                        {
                            SendRaw("tn00300");
                        }
                        catch
                        {
                        }
                        RoofTelemetry.Moving =
                            false;
                    }

                    verifyMode = false;

                    continue;
                }

                // ---------------------------------
                // Request live roof status
                // ---------------------------------

                SendRaw("xx00100");
            }
        }
        private void TelemetryLoop()
        {
            while (running)
            {
                try
                {
                    // ---------------------------------
                    // Request roof state
                    // ---------------------------------

                    SendRaw("xx00100");

                    // ---------------------------------
                    // Pulse telemetry
                    // ---------------------------------

                    if (usePulseTelemetry)
                    {
                        Thread.Sleep(150);

                        SendRaw("cv007");

                        Thread.Sleep(150);

                        // -----------------------------
                        // Pulse watchdog protection
                        // -----------------------------

                        if (RoofTelemetry.Moving)
                        {
                            if (
                                RoofTelemetry.CurrentPulseCount ==
                                RoofTelemetry.LastPulseCount)
                            {
                                double elapsed =
                                    (
                                        DateTime.Now -
                                        RoofTelemetry.LastPulseTime
                                    ).TotalSeconds;

                                if (elapsed > 3)
                                {
                                    lock (stateLock)
                                    {
                                        shutterState =
                                            ShutterState.shutterError;

                                        RoofTelemetry.ShutterState =
                                            "Error";

                                        RoofTelemetry.Faulted =
                                            true;

                                        RoofTelemetry.FaultMessage =
                                            "No pulse movement";
                                        RoofTelemetry.LastFaultTime = DateTime.Now;

                                        RoofTelemetry.LastWatchdogEvent =
                                            "No pulse movement";
                                        try
                                        {
                                            SendRaw("tn00300");
                                        }
                                        catch
                                        {
                                        }
                                        RoofTelemetry.Moving =
                                            false;
                                    }

                                    verifyMode = false;
                                }
                            }

                            RoofTelemetry.LastPulseCount =
                                RoofTelemetry.CurrentPulseCount;
                        }
                    }

                    Thread.Sleep(1000);
                }
                catch (Exception ex)
                {
                    log.LogMessage(
                        "TelemetryLoop",
                        ex.Message);
                }
            }
        }
        // =====================================================
        // PARSER
        // =====================================================

        private void Process(string data)
        {
            if (string.IsNullOrWhiteSpace(data))
                return;

            data = data.Replace("[0D]", "\n");

            var parts = data.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var p in parts)
            {
                Handle(p.Trim());
            }
        }

        // =====================================================
        // STATE ENGINE
        // =====================================================

        private void Handle(string msg)
        {
            if (string.IsNullOrWhiteSpace(msg))
                return;

            msg = msg.Trim();
            if (msg.Contains("Secure0081"))
            {
                RoofTelemetry.MountSafe = true;

                log.LogMessage(
                    "ScopeSafe",
                    "SAFE");

                return;
            }

            if (msg.Contains("NotSecure"))
            {
                RoofTelemetry.MountSafe = false;

                log.LogMessage(
                    "ScopeSafe",
                    "UNSAFE");

                return;
            }
            if (usePulseTelemetry &&
                msg.StartsWith("CV007"))
            {
                try
                {
                    string value =
                        msg.Substring(5).Trim();

                    if (int.TryParse(value, out int count))
                    {
                        RoofTelemetry.CurrentPulseCount =
                            count;

                        RoofTelemetry.LastPulseTime =
                            DateTime.Now;

                        if (RoofTelemetry.OpenPulseCount > 0)
                        {
                            int percent =
                                (int)(
                                    (double)count /
                                    RoofTelemetry.OpenPulseCount
                                    * 100.0);

                            percent =
                                Math.Max(
                                    0,
                                    Math.Min(100, percent));

                            if (shutterState ==
    ShutterState.shutterClosing)
                            {
                                RoofTelemetry.PercentOpen =
                                    100 - percent;
                            }
                            else
                            {
                                RoofTelemetry.PercentOpen =
                                    percent;
                            }
                        }

                        log.LogMessage(
                            "PULSE_COUNT",
                            count.ToString());
                    }
                }
                catch (Exception ex)
                {
                    log.LogMessage(
                        "CounterParse",
                        ex.Message);
                }

                return;
            }

            log.LogMessage("RX", msg);

            if (msg == lastFrame)
                return;

            lastFrame = msg;

            if (msg.StartsWith("0ATC"))
                return;

            if (!msg.Contains("XX001"))
                return;

            lock (stateLock)
            {
                lastRealTelemetry = DateTime.Now;
                RoofTelemetry.Faulted = false;

                RoofTelemetry.FaultMessage = "";

                // If currently opening, ignore temporary CLOSED
                if (verifyMode &&
                    shutterState == ShutterState.shutterOpening &&
                    msg.Contains("closed"))
                {
                    log.LogMessage("VERIFY", "Ignoring temporary CLOSED during OPEN verify");
                    return;
                }

                // If currently closing, ignore temporary OPEN
                if (verifyMode &&
                    shutterState == ShutterState.shutterClosing &&
                    msg.Contains("open") &&
                    !msg.Contains("closed"))
                {
                    log.LogMessage("VERIFY", "Ignoring temporary OPEN during CLOSE verify");
                    return;
                }

                if (msg.Contains("closed"))
                {
                    shutterState =
                        ShutterState.shutterClosed;

                    RoofTelemetry.ShutterState =
                        "Closed";

                    RoofTelemetry.CurrentPulseCount = 0;
                    RoofTelemetry.PercentOpen = 0;

                    RoofTelemetry.ClosedLimitActive =
                        true;

                    RoofTelemetry.OpenLimitActive =
                        false;

                    RoofTelemetry.Moving =
                        false;

                    verifyMode = false;

                    return;
                }

                if (msg.Contains("open") &&
                    !msg.Contains("closed"))
                {
                    shutterState =
                        ShutterState.shutterOpen;

                    RoofTelemetry.ShutterState =
                        "Open";
                    
                    // Restore telemetry position
                    RoofTelemetry.CurrentPulseCount =
                        RoofTelemetry.OpenPulseCount;

                    RoofTelemetry.PercentOpen =
                        100;

                    RoofTelemetry.OpenLimitActive =
                        true;

                    RoofTelemetry.ClosedLimitActive =
                        false;

                    RoofTelemetry.Moving =
                        false;

                    // -----------------------------
                    // Auto-save calibration
                    // -----------------------------

                    if (usePulseTelemetry && RoofTelemetry.CalibrationMode)
                    {
                        int learned =
                            (int)(
                                RoofTelemetry.CurrentPulseCount
                                * 1.02);

                        RoofTelemetry.OpenPulseCount = learned;
                        RoofTelemetry.LastCalibrationValue = learned;
                        RoofTelemetry.CalibrationMode = false;

                        log.LogMessage("Calibration", $"Learned OpenPulseCount={learned}");

                        try
                        {
                            Profile p =
                                new Profile();

                            p.DeviceType = "Dome";

                            p.WriteValue(
                                "ASCOM.m1OASYS_NET.Dome",
                                "OpenPulseCount",
                                learned.ToString());
                        }
                        catch
                        {
                        }
                    }

                    verifyMode = false;

                    return;
                }
            }
        }

        // =====================================================
        // COMMAND ENGINE
        // =====================================================

        private void SendRaw(string cmd)
        {
            try
            {
                byte[] data = Encoding.ASCII.GetBytes(Crc32.CalculateCRC(cmd));

                lock (ioLock)
                {
                    stream.Write(data, 0, data.Length);
                    stream.Flush();
                }
            }
            catch (Exception ex)
            {
                log.LogMessage("TX", ex.Message);
            }
        }

        // =====================================================
        // COMMANDS (ENTER VERIFY MODE)
        // =====================================================

        public void OpenShutter() => ExecuteCommand("tn00100");
        public void CloseShutter() => ExecuteCommand("tn00200");
        public void Abort() => ExecuteCommand("tn00300");

        private void ExecuteCommand(string cmd)
        {
            // ---------------------------------
            // Optional scope safety enforcement
            // ---------------------------------

            if (useScopeSafety &&
                (
                    cmd == "tn00100" ||
                    cmd == "tn00200"
                ))
            {
                SendRaw("xx005sensoron00");

                Thread.Sleep(1000);

                SendRaw("xx00200");

                Thread.Sleep(1000);

                if (!RoofTelemetry.MountSafe)
                {
                    RoofTelemetry.Faulted = true;

                    RoofTelemetry.FaultMessage =
                        "Scope not safe";

                    RoofTelemetry.LastFaultTime =
                        DateTime.Now;

                    throw new ASCOM.InvalidOperationException(
                        "Scope not safe for roof movement");
                }
            }

            lock (stateLock)
            {
                if (cmd == "tn00100")
                {
                    shutterState =
                        ShutterState.shutterOpening;

                    RoofTelemetry.ShutterState =
                        "Opening";

                    RoofTelemetry.Moving = true;

                    RoofTelemetry.MotionStartTime =
                        DateTime.Now;
                }

                if (cmd == "tn00200")
                {
                    shutterState =
                        ShutterState.shutterClosing;

                    RoofTelemetry.ShutterState =
                        "Closing";

                    RoofTelemetry.Moving = true;

                    RoofTelemetry.MotionStartTime =
                        DateTime.Now;
                }

                if (cmd == "tn00300")
                {
                    RoofTelemetry.ShutterState =
                        "Stopped";

                    RoofTelemetry.Moving = false;
                }

                verifyMode = true;

                verifyStart = DateTime.Now;
            }

            SendRaw(cmd);
        }

        // =====================================================
        // PROPERTIES
        // =====================================================

        public bool IsConnected => connected;

        public bool Slewing
        {
            get
            {
                lock (stateLock)
                {
                    return shutterState == ShutterState.shutterOpening ||
                           shutterState == ShutterState.shutterClosing ||
                           verifyMode;
                }
            }
        }

        public ShutterState ShutterStatus
        {
            get
            {
                lock (stateLock)
                    return shutterState;
            }
        }
    }
}