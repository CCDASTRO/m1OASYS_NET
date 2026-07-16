using ASCOM;
using ASCOM.DeviceInterface;
using ASCOM.Utilities;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO.Ports;

namespace m1OASYS_NET
{

    public enum ScopeSafetyState
    {
        Unknown,
        Safe,
        NotSafe
    }

    public enum NotificationType
    {
        RoofOpened,
        RoofClosed,
        RoofFault,
        ConnectionLost,
        ConnectionRestored,
        ScopeBlocked
    }

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
        private readonly ManualResetEventSlim scopeSafetyResponse =
            new ManualResetEventSlim(false);
        private SerialPort serial;

        private bool useSerial;
        private bool connected;
        private volatile bool m1Responded;
        private readonly StringBuilder rxBuffer = new StringBuilder();
        private ShutterState shutterState = ShutterState.shutterError;
        private ScopeSafetyState scopeSafety = ScopeSafetyState.Unknown;
        private DateTime lastRealTelemetry = DateTime.MinValue;
        private bool openNotificationSent = false;
        private bool closedNotificationSent = false;
        private bool connectionLostNotified = false;
        private string lastFaultNotification = "";

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

                RoofTelemetry.EnablePushover =
                    Convert.ToBoolean(
                    p.GetValue(
                    "ASCOM.m1OASYS_NET.Dome",
                    "EnablePushover",
                    "",
                    "False"));

                RoofTelemetry.NotifyRoofOpened =
                    Convert.ToBoolean(
                        p.GetValue(
                            "ASCOM.m1OASYS_NET.Dome",
                            "NotifyRoofOpened",
                            "",
                            "True"));

                RoofTelemetry.NotifyRoofClosed =
                    Convert.ToBoolean(
                        p.GetValue(
                            "ASCOM.m1OASYS_NET.Dome",
                            "NotifyRoofClosed",
                            "",
                            "True"));

                RoofTelemetry.NotifyRoofFault =
                    Convert.ToBoolean(
                        p.GetValue(
                            "ASCOM.m1OASYS_NET.Dome",
                            "NotifyRoofFault",
                            "",
                            "True"));

                RoofTelemetry.NotifyConnectionLost =
                    Convert.ToBoolean(
                        p.GetValue(
                            "ASCOM.m1OASYS_NET.Dome",
                            "NotifyConnectionLost",
                            "",
                            "True"));

                RoofTelemetry.NotifyConnectionRestored =
                    Convert.ToBoolean(
                        p.GetValue(
                            "ASCOM.m1OASYS_NET.Dome",
                            "NotifyConnectionRestored",
                            "",
                            "True"));

                RoofTelemetry.NotifyScopeBlocked =
                    Convert.ToBoolean(
                        p.GetValue(
                            "ASCOM.m1OASYS_NET.Dome",
                            "NotifyScopeBlocked",
                            "",
                            "True"));

                RoofTelemetry.PushoverToken =
                    p.GetValue(
                        "ASCOM.m1OASYS_NET.Dome",
                        "PushoverToken",
                        "",
                        "");

                RoofTelemetry.PushoverUserKey =
                    p.GetValue(
                        "ASCOM.m1OASYS_NET.Dome",
                        "PushoverUserKey",
                        "",
                        "");

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

        public void Connect(
            string connectionMethod,
            string ip,
            int port,
            string comPort,
    
            bool enablePulseTelemetry,
    
            bool enableScopeSafety)
        {
            Disconnect();

            m1Responded = false;
            scopeSafety = ScopeSafetyState.Unknown;
            RoofTelemetry.ScopeSafety = ScopeSafetyState.Unknown;
            RoofTelemetry.MountSafe = false;

            usePulseTelemetry = enablePulseTelemetry;
            useScopeSafety = enableScopeSafety;
            RoofTelemetry.ScopeSafetyEnabled = enableScopeSafety;

            if (connectionMethod.StartsWith(
                "Serial",
                StringComparison.OrdinalIgnoreCase))
            {
                useSerial = true;

                serial = new SerialPort(
                    comPort,
                    115200,
                    Parity.None,
                    8,
                    StopBits.One);

                log.LogMessage(
                    "Connect",
                    $"Opening Serial Port {comPort}");

                serial.Open();

                log.LogMessage(
                    "Connect",
                    $"Serial Port {comPort} Open");
            }
            else
            {
                useSerial = false;

                client = new TcpClient
                {
                    ReceiveTimeout = 3000,
                    SendTimeout = 3000
                };

                client.Connect(ip, port);

                stream = client.GetStream();

                log.LogMessage(
                    "Connect",
                    $"Connected to {ip}:{port}");
            }

            running = true;

            rxThread = new Thread(RxLoop)
            {
                IsBackground = true
            };
            rxThread.Start();

            verifyThread = new Thread(VerifyLoop)
            {
                IsBackground = true
            };
            verifyThread.Start();

            telemetryThread = new Thread(TelemetryLoop)
            {
                IsBackground = true
            };
            telemetryThread.Start();

            // ============================================
            // Wait for actual M1 traffic before connecting
            // ============================================

            log.LogMessage(
                "Connect",
                "Waiting for M1 response...");

            Thread.Sleep(300);

            SendRaw("vn"); 

            DateTime start = DateTime.Now;

            while ((DateTime.Now - start).TotalSeconds < 5)
            {
                if (m1Responded)
                {
                    connected = true;

                    RoofTelemetry.LastReconnectTime =
                        DateTime.Now;

                    lastRealTelemetry =
                        DateTime.Now;

                    connectionLostNotified =
                        false;


                    log.LogMessage(
                        "Connect",
                        "Connected successfully.");
                    if (useScopeSafety)
                    {
                        SendRaw("xx005sensoron00");

                        Thread.Sleep(500);

                        SendRaw("xx00200");

                        log.LogMessage(
                            "ScopeSafe",
                            "Initial safety query sent");
                    }
                    return;
                }

                Thread.Sleep(100);
            }

            Disconnect();

            throw new Exception(
                "No response from M1.");
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
                serial?.Close();
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
            stream = null;
            client = null;
            serial = null;

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
                    // Serial Connection
                    // -----------------------------

                    if (useSerial)
                    {
                        if (serial == null)
                        {
                            log.LogMessage(
                                "RX",
                                "serial == null");

                            break;
                        }

                        if (!serial.IsOpen)
                        {
                            log.LogMessage(
                                "RX",
                                "serial.IsOpen == false");

                            break;
                        }

                        if (serial.BytesToRead > 0)
                        {
                            string incoming =
                                serial.ReadExisting();
                            log.LogMessage(
                                "RXRAW",
                                incoming);

                            rxBuffer.Append(incoming);

                            string bufferText =
                                rxBuffer.ToString();

                            int pos;

                            while ((pos = bufferText.IndexOf('\r')) >= 0)
                            {
                                string message =
                                    bufferText.Substring(0, pos);

                                if (!string.IsNullOrWhiteSpace(message))
                                {

                                    log.LogMessage(
                                       "RXMSG",
                                       message);

                                    Process(message);
                                }

                                bufferText =
                                    bufferText.Substring(pos + 1);
                            }

                            rxBuffer.Clear();
                            rxBuffer.Append(bufferText);
                        }
                        else
                        {
                            Thread.Sleep(20);
                        }

                        continue;
                    }

                    // -----------------------------
                    // TCP Connection
                    // -----------------------------

                    if (client == null ||
                        !client.Connected ||
                        stream == null)
                    {
                        break;
                    }

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

                        if (len <= 0)
                        {
                            break;
                        }

                        sb.Append(
                            Encoding.ASCII.GetString(
                                buffer,
                                0,
                                len));

                        string bufferText =
                            sb.ToString();

                        int pos;

                        while ((pos = bufferText.IndexOf('\r')) >= 0)
                        {
                            string message =
                                bufferText.Substring(0, pos);

                            if (!string.IsNullOrWhiteSpace(message))
                            {
                                Process(message);
                            }

                            bufferText =
                                bufferText.Substring(pos + 1);
                        }

                        sb.Clear();
                        sb.Append(bufferText);
                    }
                    else
                    {
                        Thread.Sleep(20);
                    }
                }
                catch (ObjectDisposedException ex)
                {
                    log.LogMessage(
                        "RX",
                        ex.ToString());

                    break;
                }
                catch (IOException ex)
                {
                    log.LogMessage(
                        "RX",
                        ex.ToString());

                    break;
                }
                catch (Exception ex)
                {
                    log.LogMessage(
                        "RX",
                        ex.ToString());

                    break;
                }
            }

            if (running && !connectionLostNotified)
            {
                // The transport has ended, so do not report a stale ASCOM
                // Connected state while the caller still holds this instance.
                connected = false;
                connectionLostNotified = true;

                SendNotification(
                    NotificationType.ConnectionLost,
                    "⚠ ELK communication lost");
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

                        SendFaultNotification(
                            "Movement timeout");

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

                                if (elapsed > 10)
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

                                        SendNotification(
                                            NotificationType.RoofFault,
                                            "⚠ Roof fault: No pulse movement");

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


                    // ---------------------------------
                    // Communication watchdog
                    // ---------------------------------

                    if (connected)
                    {
                        DateTime telemetryTime;

                        lock (stateLock)
                        {
                            telemetryTime = lastRealTelemetry;
                        }

                        double secondsSinceTelemetry =
                            (DateTime.Now - telemetryTime)
                            .TotalSeconds;

                        log.LogMessage(
                            "COMMDEBUG",
                            $"Last={telemetryTime:HH:mm:ss.fff} Age={secondsSinceTelemetry:F1}");

                        if (secondsSinceTelemetry > 20)
                        {
                            if (!connectionLostNotified)
                            {
                                connectionLostNotified = true;

                                SendNotification(
                                    NotificationType.ConnectionLost,
                                    "⚠ ELK communication lost");

                                log.LogMessage(
                                    "COMM",
                                    "Communication lost");
                            }
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

            string upper = data.ToUpperInvariant();

            if (upper.Contains("VN") ||
            upper.Contains("XK") ||
            upper.Contains("KC") ||
            upper.Contains("XX") ||
            upper.Contains("D6Z"))
            {
                m1Responded = true;

                lock (stateLock)
                {
                    lastRealTelemetry = DateTime.Now;
                }

                log.LogMessage(
                    "TELEMETRY",
                    $"Updated {lastRealTelemetry:HH:mm:ss.fff}");
            }

            data = data.Replace("[0D]", "\n");

            var parts = data.Split(
                new[] { '\r', '\n' },
                StringSplitOptions.RemoveEmptyEntries);

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

            msg = msg.Trim().ToUpperInvariant();
            if (msg.Contains("SECURE0081"))
            {
                scopeSafety =
                    ScopeSafetyState.Safe;

                RoofTelemetry.MountSafe = true;
                RoofTelemetry.ScopeSafety =
                     ScopeSafetyState.Safe;

                scopeSafetyResponse.Set();

                log.LogMessage(
                    "ScopeSafe",
                    "SAFE");

                return;
            }

            if (msg.Contains("NOTSECURE"))
            {
                scopeSafety =
                    ScopeSafetyState.NotSafe;

                RoofTelemetry.MountSafe = false;
                RoofTelemetry.ScopeSafety =
                     ScopeSafetyState.NotSafe;

                scopeSafetyResponse.Set();

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


                        log.LogMessage(
                            "MOTION",
                            $"Pulse detected ({count})");
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
                

                if (connectionLostNotified)
                {
                    connectionLostNotified = false;

                    SendNotification(
                        NotificationType.ConnectionRestored,
                        "✅ ELK communication restored");

                    log.LogMessage(
                        "COMM",
                        "Communication restored");
                }

                RoofTelemetry.Faulted = false;
                lastFaultNotification = "";
                RoofTelemetry.FaultMessage = "";

                // If currently opening, ignore temporary CLOSED
                if (verifyMode &&
                    shutterState == ShutterState.shutterOpening &&
                    msg.Contains("CLOSED"))
                {
                    log.LogMessage("VERIFY", "Ignoring temporary CLOSED during OPEN verify");
                    return;
                }

                // If currently closing, ignore temporary OPEN
                if (verifyMode &&
                    shutterState == ShutterState.shutterClosing &&
                    msg.Contains("OPEN") &&
                    !msg.Contains("CLOSED"))
                {
                    log.LogMessage("VERIFY", "Ignoring temporary OPEN during CLOSE verify");
                    return;
                }

                if (msg.Contains("CLOSED"))
                {
                    shutterState =
                        ShutterState.shutterClosed;

                    RoofTelemetry.ShutterState =
                        "Closed";

                    if (!closedNotificationSent)
                    {
                        SendNotification(
                            NotificationType.RoofClosed,
                            "🏠 Observatory roof closed");

                        closedNotificationSent = true;
                        openNotificationSent = false;
                    }

                    
                    RoofTelemetry.ClosedLimitActive =
                        true;

                    RoofTelemetry.OpenLimitActive =
                        false;

                    RoofTelemetry.Moving =
                        false;

                    verifyMode = false;

                    return;
                }

                if (msg.Contains("OPEN") &&
                    !msg.Contains("CLOSED"))
                {
                    shutterState =
                        ShutterState.shutterOpen;

                    RoofTelemetry.ShutterState =
                        "Open";

                    if (!openNotificationSent)
                    {
                        SendNotification(
                            NotificationType.RoofOpened,
                            "🏠 Observatory roof opened");

                        openNotificationSent = true;
                        closedNotificationSent = false;
                    }

                    // Restore telemetry position
                    

                    RoofTelemetry.OpenLimitActive =
                        true;

                    RoofTelemetry.ClosedLimitActive =
                        false;

                    RoofTelemetry.Moving =
                        false;

                    // -----------------------------
                    // Auto-save calibration
                    // -----------------------------

                    
                    verifyMode = false;

                    return;
                }
            }
        }

        private void SendFaultNotification(string faultMessage)
        {
            if (lastFaultNotification == faultMessage)
                return;

            lastFaultNotification = faultMessage;

            SendNotification(
                NotificationType.RoofFault,
                $"⚠ Roof fault: {faultMessage}");
        }

        private async void SendNotification(
    NotificationType type,
    string message)
        {
            log.LogMessage(
    "Pushover",
    $"Enable={RoofTelemetry.EnablePushover} " +
    $"Opened={RoofTelemetry.NotifyRoofOpened} " +
    $"Closed={RoofTelemetry.NotifyRoofClosed}");
            if (!RoofTelemetry.EnablePushover)
            {
                return;
            }

            bool enabled = false;

            switch (type)
            {
                case NotificationType.RoofOpened:
                    enabled =
                        RoofTelemetry.NotifyRoofOpened;
                    break;

                case NotificationType.RoofClosed:
                    enabled =
                        RoofTelemetry.NotifyRoofClosed;
                    break;

                case NotificationType.RoofFault:
                    enabled =
                        RoofTelemetry.NotifyRoofFault;
                    break;

                case NotificationType.ConnectionLost:
                    enabled =
                        RoofTelemetry.NotifyConnectionLost;
                    break;

                case NotificationType.ConnectionRestored:
                    enabled =
                        RoofTelemetry.NotifyConnectionRestored;
                    break;

                case NotificationType.ScopeBlocked:
                    enabled =
                        RoofTelemetry.NotifyScopeBlocked;
                    break;
            }

            log.LogMessage( "Pushover", $"Type={type} Enabled={enabled}");

            if (!enabled)
            {
                return;
            }

            try
            {
                await PushoverNotifier.SendAsync(
                    RoofTelemetry.PushoverToken,
                    RoofTelemetry.PushoverUserKey,
                    message);

                log.LogMessage(
                    "Pushover",
                    message);
            }
            catch (Exception ex)
            {
                log.LogMessage(
                    "Pushover",
                    ex.Message);
            }
        }
        // =====================================================
        // COMMAND ENGINE
        // =====================================================

        private bool SendRaw(string cmd)
        {
            try
            {
                byte[] data =
                    Encoding.ASCII.GetBytes(
                        Crc32.CalculateCRC(cmd));

                lock (ioLock)
                {
                    if (useSerial)
                    {
                        serial.Write(
                            Encoding.ASCII.GetString(
                                data));
                    }
                    else
                    {
                        stream.Write(
                            data,
                            0,
                            data.Length);

                        stream.Flush();
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                connected = false;
                log.LogMessage(
                    "TX",
                    ex.Message);

                return false;
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
            if (!connected)
            {
                throw new ASCOM.NotConnectedException(
                    "The dome controller is not connected.");
            }

            // ---------------------------------
            // Optional scope safety enforcement
            // ---------------------------------

            if (useScopeSafety &&
                (
                    cmd == "tn00100" ||
                    cmd == "tn00200"
                ))
            {
                scopeSafetyResponse.Reset();
                scopeSafety = ScopeSafetyState.Unknown;
                RoofTelemetry.ScopeSafety = ScopeSafetyState.Unknown;
                RoofTelemetry.MountSafe = false;

                SendRaw("xx005sensoron00");

                Thread.Sleep(1000);

                SendRaw("xx00200");

                bool receivedSafetyState =
                    scopeSafetyResponse.Wait(2000);

                if (!receivedSafetyState ||
                    scopeSafety != ScopeSafetyState.Safe)
                {
                    RoofTelemetry.Faulted = true;

                    RoofTelemetry.FaultMessage =
                        receivedSafetyState
                            ? "Scope not safe"
                            : "Scope safety status unavailable";

                    RoofTelemetry.LastFaultTime =
                        DateTime.Now;

                    SendNotification(
                        NotificationType.ScopeBlocked,
                        "⚠ Roof movement blocked - telescope not safe");

                    throw new ASCOM.InvalidOperationException(
                        RoofTelemetry.FaultMessage +
                        "; roof movement blocked");
                }
            }

            if (cmd == "tn00300")
            {
                if (!SendRaw(cmd))
                    throw new ASCOM.NotConnectedException(
                        "The dome controller could not send the abort command.");

                lock (stateLock)
                {
                    shutterState = ShutterState.shutterError;
                    RoofTelemetry.ShutterState = "Stopped";
                    RoofTelemetry.Moving = false;
                    verifyMode = false;
                }

                return;
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

                verifyMode = true;

                verifyStart = DateTime.Now;
            }

            if (!SendRaw(cmd))
            {
                lock (stateLock)
                {
                    shutterState = ShutterState.shutterError;
                    RoofTelemetry.ShutterState = "Communication error";
                    RoofTelemetry.Moving = false;
                    verifyMode = false;
                }

                throw new ASCOM.NotConnectedException(
                    "The dome controller could not send the roof command.");
            }
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
        public ScopeSafetyState ScopeSafety
        {
            get
            {
                return scopeSafety;
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
