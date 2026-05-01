using ASCOM;
using ASCOM.DeviceInterface;
using ASCOM.Utilities;
using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace m1OASYS_NET
{
    public class DomeController
    {
        private TcpClient client;
        private NetworkStream stream;

        private Thread rxThread;
        private Thread verifyThread;

        private volatile bool running;

        private readonly object ioLock = new object();
        private readonly object stateLock = new object();

        private bool connected;
        

        private ShutterState shutterState = ShutterState.shutterError;

        private DateTime lastRealTelemetry = DateTime.MinValue;

        // ---------------- VERIFY MODE ----------------
        private volatile bool verifyMode = false;
        private DateTime verifyStart;
        private const int VERIFY_TIMEOUT_MS = 15000;

        private string lastFrame = "";

        private TraceLogger log;

        public DomeController()
        {
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

        public void Connect(string ip, int port)
        {
            

            client = new TcpClient
            {
                ReceiveTimeout = 3000,
                SendTimeout = 3000
            };

            client.Connect(ip, port);
            stream = client.GetStream();

            running = true;

            rxThread = new Thread(RxLoop) { IsBackground = true };
            rxThread.Start();

            verifyThread = new Thread(VerifyLoop) { IsBackground = true };
            verifyThread.Start();

            connected = true;

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

            try { rxThread?.Join(1000); } catch { }
            try { verifyThread?.Join(1000); } catch { }

            try { stream?.Close(); } catch { }
            try { client?.Close(); } catch { }

            connected = false;
        }

        // =====================================================
        // RX LOOP
        // =====================================================

        private void RxLoop()
        {
            byte[] buffer = new byte[1024];
            var sb = new StringBuilder();

            while (running && client?.Connected == true)
            {
                try
                {
                    if (stream.DataAvailable)
                    {
                        int len;

                        lock (ioLock)
                        {
                            len = stream.Read(buffer, 0, buffer.Length);
                        }

                        if (len > 0)
                        {
                            sb.Append(Encoding.ASCII.GetString(buffer, 0, len));
                            Process(sb.ToString());
                            sb.Clear();
                        }
                    }
                    else
                    {
                        Thread.Sleep(20);
                    }
                }
                catch (Exception ex)
                {
                    log.LogMessage("RX", ex.Message);
                }
            }
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

                // timeout → error
                if ((DateTime.Now - verifyStart).TotalMilliseconds > VERIFY_TIMEOUT_MS)
                {
                    lock (stateLock)
                    {
                        shutterState = ShutterState.shutterError;
                    }

                    verifyMode = false;
                    continue;
                }

                // actively request state
                SendRaw("xx00100");
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
            log.LogMessage("RX", msg);

            if (msg == lastFrame)
                return;

            lastFrame = msg;

            // =====================================================
            // IGNORE ACK COMPLETELY
            // =====================================================
            if (msg.StartsWith("0ATC"))
                return;

            lock (stateLock)
            {
                lastRealTelemetry = DateTime.Now;

                

                // =====================================================
                // OPEN
                // =====================================================
                if (msg.Contains("open") && !msg.Contains("close"))
                {
                    shutterState = ShutterState.shutterOpen;
                    verifyMode = false;
                    return;
                }

                // =====================================================
                // CLOSED
                // =====================================================
                if (msg.Contains("closed"))
                {
                    shutterState = ShutterState.shutterClosed;
                    verifyMode = false;
                    return;
                }

                // =====================================================
                // MOVING
                // =====================================================
                if (msg.Contains("opening"))
                {
                    shutterState = ShutterState.shutterOpening;
                    return;
                }

                if (msg.Contains("closing"))
                {
                    shutterState = ShutterState.shutterClosing;
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
            SendRaw(cmd);

            lock (stateLock)
            {
                verifyMode = true;
                verifyStart = DateTime.Now;
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