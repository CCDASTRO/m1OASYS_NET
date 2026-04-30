using ASCOM;
using ASCOM.DeviceInterface;
using ASCOM.Utilities;
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Timers;
using Timer = System.Timers.Timer;

namespace m1OASYS_NET
{
    public class DomeController
    {
        private TcpClient client;
        private NetworkStream stream;
        private Timer pollTimer;

        private bool connected;
        private bool moving;

        private bool scopeSafe;
        private bool scopeSafeEnabled;

        private ShutterState shutterState = ShutterState.shutterError;

        private readonly object lockObj = new object();
        private TraceLogger log;

        public DomeController()
        {
            log = new TraceLogger("", "DomeController");
            log.Enabled = true;

            pollTimer = new Timer(1000);
            pollTimer.Elapsed += Poll;
            pollTimer.AutoReset = true;
        }

        public void Connect(string ip, int port, bool scopeSafeEnable)
        {
            scopeSafeEnabled = scopeSafeEnable;
            client = new TcpClient();
            client.ReceiveTimeout = 2000;
            client.SendTimeout = 2000;
            client.Connect(ip, port);
            stream = client.GetStream();
            stream.ReadTimeout = 2000;

            string response = SendCommandWithRetry("vn", 5);
            if (response != null && (response.Contains("D6Z") || response.Contains("XK") || response.Contains("XX")))
            {
                connected = true;
                pollTimer.Start();
                log.LogMessage("Connect", "Connected successfully.");
                return;
            }

            Disconnect();
            throw new Exception($"Connection failed. Response: '{response}'");
        }

        public void Disconnect()
        {
            pollTimer?.Stop();
            try { stream?.Close(); } catch { }
            try { client?.Close(); } catch { }
            connected = false;
        }

        private string SendCommandWithRetry(string cmd, int maxAttempts)
        {
            string command = Crc32.CalculateCRC(cmd);
            byte[] data = Encoding.ASCII.GetBytes(command);

            for (int attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    stream.Write(data, 0, data.Length);
                    Thread.Sleep(100); // Brief pause for device processing

                    byte[] buffer = new byte[256];
                    int len = stream.Read(buffer, 0, buffer.Length);
                    return Encoding.ASCII.GetString(buffer, 0, len);
                }
                catch (Exception ex)
                {
                    log.LogMessage("SendCommand", $"Attempt {attempt} failed: {ex.Message}");
                    if (attempt < maxAttempts)
                        Thread.Sleep(200 * (int)Math.Pow(2, attempt)); // Exponential backoff
                }
            }
            return null;
        }

        private void Poll(object sender, ElapsedEventArgs e)
        {
            if (client == null || !client.Connected)
                return;

            try
            {
                lock (lockObj)
                {
                    string response = SendCommandWithRetry("xx00100", 3);
                    if (response == null) return;

                    scopeSafe = response.Contains("Secure");

                    if (moving)
                    {
                        if (response.Contains("open"))
                        {
                            shutterState = ShutterState.shutterOpen;
                            moving = false;
                        }
                        else if (response.Contains("closed"))
                        {
                            shutterState = ShutterState.shutterClosed;
                            moving = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.LogMessage("Poll", ex.Message);
            }
        }

        public void OpenShutter()
        {
            moving = true;
            shutterState = ShutterState.shutterOpening;
            SendCommandWithRetry("tn00100", 3);
        }

        public void CloseShutter()
        {
            if (scopeSafeEnabled && !scopeSafe)
                throw new DriverException("Scope not safe - blocked");

            moving = true;
            shutterState = ShutterState.shutterClosing;
            SendCommandWithRetry("tn00200", 3);
        }

        public void Abort()
        {
            moving = false;
            shutterState = ShutterState.shutterError;
            SendCommandWithRetry("tn00300", 3);
        }

        public bool IsConnected => connected;
        public bool Slewing => moving;
        public ShutterState ShutterStatus => shutterState;
    }
}