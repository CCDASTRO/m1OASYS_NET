using System;
using System.Net.Sockets;
using System.Text;
using System.Timers;
using Timer = System.Timers.Timer;

using ASCOM;
using ASCOM.DeviceInterface;
using ASCOM.Utilities;

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

            connected = true;
            pollTimer.Start();
        }

        public void Disconnect()
        {
            pollTimer?.Stop();

            try { stream?.Close(); } catch { }
            try { client?.Close(); } catch { }

            connected = false;
        }

        private void Send(string cmd)
        {
            if (client == null || !client.Connected)
                return;

            byte[] data = Encoding.ASCII.GetBytes(cmd + "\r\n");
            stream.Write(data, 0, data.Length);
        }

        private void Poll(object sender, ElapsedEventArgs e)
        {
            if (client == null || !client.Connected)
                return;

            try
            {
                lock (lockObj)
                {
                    Send("xx00100");

                    byte[] buffer = new byte[256];
                    int len = stream.Read(buffer, 0, buffer.Length);

                    string resp = Encoding.ASCII.GetString(buffer, 0, len);

                    scopeSafe = resp.Contains("Secure");

                    if (moving)
                    {
                        if (resp.Contains("open"))
                        {
                            shutterState = ShutterState.shutterOpen;
                            moving = false;
                        }
                        else if (resp.Contains("closed"))
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
            Send("tn00100");
        }

        public void CloseShutter()
        {
            if (scopeSafeEnabled && !scopeSafe)
                throw new DriverException("Scope not safe - blocked");

            moving = true;
            shutterState = ShutterState.shutterClosing;
            Send("tn00200");
        }

        public void Abort()
        {
            moving = false;
            shutterState = ShutterState.shutterError;
            Send("tn00300");
        }

        public bool IsConnected => connected;
        public bool Slewing => moving;
        public ShutterState ShutterStatus => shutterState;
    }
}