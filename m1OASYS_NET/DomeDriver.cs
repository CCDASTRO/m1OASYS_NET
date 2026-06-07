using ASCOM;
using ASCOM.DeviceInterface;
using ASCOM.Utilities;
using System;
using System.Collections;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
namespace m1OASYS_NET
{
    [ComVisible(true)]
    [Guid("B2E6A6F2-4C6D-4F2A-9A11-123456789ABC")] // MUST be unique
    [ProgId("ASCOM.m1OASYS_NET.Dome")]
    [ClassInterface(ClassInterfaceType.None)]
   
    public class DomeDriver : IDomeV2
    {

        #region COM Registration

        [ComRegisterFunction]
        public static void RegisterASCOM(Type t)
        {
            using (Profile profile = new Profile())
            {
                profile.DeviceType = "Dome";

                // MUST match your ProgID exactly
                string driverID = "ASCOM.m1OASYS_NET.Dome";

                profile.Register(driverID, "m1OASYS Dome Driver");

                profile.WriteValue(driverID, "CLSID", t.GUID.ToString("B"));
                profile.WriteValue(driverID, "Description", "m1OASYS Dome Driver");
                profile.WriteValue(driverID, "InterfaceVersion", "2");
            }
        }

        [ComUnregisterFunction]
        public static void UnregisterASCOM(Type t)
        {
            using (Profile profile = new Profile())
            {
                profile.DeviceType = "Dome";

                string driverID = "ASCOM.m1OASYS_NET.Dome";

                profile.Unregister(driverID);
            }
        }

        #endregion
        private Thread uiThread;

      
        private DomeController dome = new DomeController();
        private bool connected;
        private StatusForm statusForm;
        private const string ID = "ASCOM.m1OASYS_NET.Dome";

        // ---------------- ASCOM CONNECT ----------------
        public bool Connected
        {
            get => connected;

            set
            {
                if (value)
                {
                    Profile p =
                        new Profile();

                    p.DeviceType =
                        "Dome";

                    string ip =
    p.GetValue(
        ID,
        "IP",
        "",
        "127.0.0.1");

                    string connectionMethod =
                        p.GetValue(
                            ID,
                            "ConnectionMethod",
                            "",
                            "TCP/IP");

                    int port = 0;

                    // Only needed for TCP/IP connections
                    if (!connectionMethod.Equals(
                            "Serial",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        if (!int.TryParse(
                                p.GetValue(
                                    ID,
                                    "Port",
                                    "",
                                    "2101"),
                                out port))
                        {
                            throw new Exception(
                                "Invalid Port setting.");
                        }
                    }

                    string comPort =
                        p.GetValue(
                            ID,
                            "ComPort",
                            "",
                            "COM1");

                    bool pulseTelemetry;

                    if (!bool.TryParse(
                            p.GetValue(
                                ID,
                                "UsePulseTelemetry",
                                "",
                                "False"),
                            out pulseTelemetry))
                    {
                        pulseTelemetry = false;
                    }

                    bool scopeSafety;

                    if (!bool.TryParse(
                            p.GetValue(
                                ID,
                                "UseScopeSafety",
                                "",
                                "False"),
                            out scopeSafety))
                    {
                        scopeSafety = false;
                    }

                    int openPulseCount;

                    if (!int.TryParse(
                            p.GetValue(
                                ID,
                                "OpenPulseCount",
                                "",
                                "5000"),
                            out openPulseCount))
                    {
                        openPulseCount = 5000;
                    }

                    RoofTelemetry.OpenPulseCount =
                        openPulseCount;
                    RoofTelemetry.OpenPulseCount =
                        openPulseCount;

                    dome.Connect(
                        connectionMethod,
                        ip,
    port,
    comPort,
    pulseTelemetry,
    scopeSafety);

                    connected = true;

                    // ---------------------------------
                    // Launch UI on dedicated STA thread
                    // ---------------------------------

                    if (statusForm == null ||
                        statusForm.IsDisposed)
                    {
                        uiThread =
                            new Thread(() =>
                            {
                                statusForm =
                                    new StatusForm();

                                Application.Run(
                                    statusForm);
                            });

                        uiThread.SetApartmentState(
                            ApartmentState.STA);

                        uiThread.IsBackground =
                            true;

                        uiThread.Start();
                    }
                }
                else
                {
                    // ---------------------------------
                    // Stop dome controller first
                    // ---------------------------------

                    dome.Disconnect();

                    connected = false;

                    RoofTelemetry.Moving =
                        false;

                    // ---------------------------------
                    // Safely close UI
                    // ---------------------------------

                    try
                    {
                        if (statusForm != null &&
                            !statusForm.IsDisposed)
                        {
                            statusForm.Invoke(
                                new Action(() =>
                                {
                                    try
                                    {
                                        statusForm.ForceClose();
                                    }
                                    catch
                                    {
                                    }
                                }));
                        }
                    }
                    catch
                    {
                    }

                    statusForm = null;
                }
            }
        }
        // ---------------- REQUIRED ASCOM ENTRY ----------------
        public void SetupDialog()
        {
            new SetupDialogForm().ShowDialog();
        }

        // ---------------- SHUTTER CONTROL ----------------
        public void OpenShutter() => dome.OpenShutter();
        public void CloseShutter() => dome.CloseShutter();
        public void AbortSlew() => dome.Abort();

        public ShutterState ShutterStatus => dome.ShutterStatus;
        public bool Slewing => dome.Slewing;

        // ---------------- IDENTIFICATION ----------------
        public string Name => "m1OASYS Dome";
        public string Description => "TCP Dome Driver";
        public string DriverInfo => "m1OASYS ASCOM Dome Driver";
        public string DriverVersion
        {
            get
            {
                Version v =
                    Assembly.GetExecutingAssembly()
                        .GetName()
                        .Version;

                return $"{v.Major}.{v.Minor}.{v.Build}";
            }
        }
        public short InterfaceVersion => 2;

        // ---------------- CAPABILITIES ----------------
        public bool CanFindHome => false;
        public bool CanPark => false;
        public bool CanSetPark => false;
        public bool CanSetShutter => true;
        public bool CanSlave => false;
        public bool CanSyncAzimuth => false;
        public bool CanSetAltitude => false;
        public bool CanSetAzimuth => false;

        public bool AtHome => false;
        public bool AtPark => false;

        public double Altitude => 0;
        public double Azimuth => 0;

        public bool Slaved
        {
            get => false;
            set => throw new PropertyNotImplementedException();
        }

        // ---------------- ASCOM ACTION SYSTEM ----------------
        public string Action(string ActionName, string ActionParameters)
        {
            throw new ActionNotImplementedException(ActionName);
        }

        public ArrayList SupportedActions => new ArrayList();

        // ---------------- COMMAND INTERFACE ----------------
        public void CommandBlind(string Command, bool Raw) { }
        public bool CommandBool(string Command, bool Raw) => false;
        public string CommandString(string Command, bool Raw) => "";

        // ---------------- MOTION ----------------
        public void FindHome() => throw new MethodNotImplementedException();
        public void Park() => throw new MethodNotImplementedException();
        public void SetPark() => throw new MethodNotImplementedException();
        public void SlewToAzimuth(double Azimuth) => throw new MethodNotImplementedException();
        public void SlewToAltitude(double Altitude) => throw new MethodNotImplementedException();
        public void SyncToAzimuth(double Azimuth) => throw new MethodNotImplementedException();

        // ---------------- CLEANUP ----------------
        public void Dispose()
        {
            dome.Disconnect();
        }
    }
}