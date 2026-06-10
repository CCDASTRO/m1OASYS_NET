using System;

namespace m1OASYS_NET
{
    public static class RoofTelemetry
    {
        // ---------------------------------
        // Pulse telemetry
        // ---------------------------------

        public static int CurrentPulseCount = 0;
        public static bool CalibrationMode = false;
        public static int OpenPulseCount = 5000;
        public static int LastCalibrationValue = 0;
        public static int PercentOpen = 0;
        public static DateTime LastFaultTime = DateTime.MinValue;

        public static DateTime LastReconnectTime =
            DateTime.MinValue;

        public static string LastWatchdogEvent =
            "";
        // ---------------------------------
        // Motion timing
        // ---------------------------------

        public static DateTime LastPulseTime =
            DateTime.MinValue;

        public static DateTime MotionStartTime =
            DateTime.MinValue;

        // ---------------------------------
        // Roof state
        // ---------------------------------

        public static bool Moving = false;

        public static string ShutterState = "Unknown";
        public static bool ScopeSafetyEnabled = false;

        public static bool OpenLimitActive = false;
        public static bool MountSafe = false;

        public static ScopeSafetyState ScopeSafety =
            ScopeSafetyState.Unknown;
        public static bool ClosedLimitActive = false;
        public static int LastPulseCount = 0;
        // ---------------------------------
        // Fault handling
        // ---------------------------------

        public static bool Faulted = false;

        public static string FaultMessage = "";
    }
}