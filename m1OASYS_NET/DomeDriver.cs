using System;
using System.Collections;
using System.Runtime.InteropServices;
using ASCOM;
using ASCOM.DeviceInterface;
using ASCOM.Utilities;

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

        private DomeController dome = new DomeController();
        private bool connected;

        private const string ID = "ASCOM.m1OASYS_NET.Dome";

        // ---------------- ASCOM CONNECT ----------------
        public bool Connected
        {
            get => connected;
            set
            {
                if (value)
                {
                    Profile p = new Profile();
                    p.DeviceType = "Dome";

                    string ip = p.GetValue(ID, "IP", "", "127.0.0.1");
                    int port = int.Parse(p.GetValue(ID, "Port", "", "0"));
                    bool safe = bool.Parse(p.GetValue(ID, "ScopeSafeEnabled", "", "False"));

                    dome.Connect(ip, port);
                    connected = true;
                }
                else
                {
                    dome.Disconnect();
                    connected = false;
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
        public string DriverVersion => "1.1.3";
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