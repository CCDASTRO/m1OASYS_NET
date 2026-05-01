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
        private Thread readThread;
        private volatile bool isRunning = false;

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

            string response = SendCommandWithRetry("vn", 3);
            if (response != null && (response.Contains("D6Z") || response.Contains("VN") || response.Contains("XK") || response.Contains("XX")))
            {
                isRunning = true;
                StartReadLoop();
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
            isRunning = false;
            pollTimer?.Stop();

            try { readThread?.Join(1000); } catch { }
            try { stream?.Close(); stream?.Dispose(); } catch { }
            try { client?.Close(); client?.Dispose(); } catch { }

            connected = false;
        }

        private void StartReadLoop()
        {
            readThread = new Thread(ReadLoop);
            readThread.IsBackground = true;
            readThread.Start();
        }

        private void ReadLoop()
        {
            byte[] buffer = new byte[256];
            StringBuilder partialMessage = new StringBuilder();

            while (isRunning && client?.Connected == true)
            {
                try
                {
                    int bytesRead = stream.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0) break;

                    string data = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    partialMessage.Append(data);

                    int endIndex;
                    while ((endIndex = partialMessage.ToString().IndexOf("\r\n")) != -1)
                    {
                        string message = partialMessage.ToString(0, endIndex);
                        partialMessage.Remove(0, endIndex + 2);
                        HandleUnsolicitedMessage(message);
                    }
                }
                catch (IOException) { break; }
                catch (Exception ex) { log.LogMessage("ReadLoop", ex.Message); break; }
            }
        }

        private void HandleUnsolicitedMessage(string message)
        {
            log.LogMessage("ReadLoop", $"Unsolicited: {message}");
            if (message.Contains("Secure")) scopeSafe = true;
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
                    Thread.Sleep(500);

                    byte[] buffer = new byte[256];
                    int len = stream.Read(buffer, 0, buffer.Length);
                    string response = Encoding.ASCII.GetString(buffer, 0, len);
                    log.LogMessage("SendCommand", $"Response for '{cmd}': '{response}'");
                    return response;
                }
                catch (Exception ex)
                {
                    log.LogMessage("SendCommand", $"Attempt {attempt} failed: {ex.Message}");
                    if (attempt < maxAttempts)
                        Thread.Sleep(200 * (int)Math.Pow(2, attempt));
                }
            }
            return null;
        }

        private void Poll(object sender, ElapsedEventArgs e)
        {
            try
            {
                lock (lockObj)
                {
                    if (client == null || !client.Connected || !isRunning)
                        return;

                    string response = SendCommandWithRetry("xx00100", 3);
                    if (response == null) return;

                    scopeSafe = response.Contains("Secure");

                    // Always evaluate shutter state from response
                    if (response.IndexOf("opening", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        shutterState = ShutterState.shutterOpening;
                        moving = true;
                    }
                    else if (response.IndexOf("closing", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        shutterState = ShutterState.shutterClosing;
                        moving = true;
                    }
                    else if (response.IndexOf("open", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        shutterState = ShutterState.shutterOpen;
                        moving = false;
                    }
                    else if (response.IndexOf("closed", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        shutterState = ShutterState.shutterClosed;
                        moving = false;
                    }
                    else
                    {
                        shutterState = ShutterState.shutterError;
                        moving = false;
                    }

                    log.LogMessage("Poll", $"Response='{response}', State={shutterState}, Moving={moving}");
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