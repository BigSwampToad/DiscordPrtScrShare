using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace DiscordPrtScrShareService
{
    public static class DisplayHook
    {
        [DllImport("user32.dll")]
        static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, EnumMonitorsDelegate lpfnEnum, IntPtr dwData);

        [StructLayout(LayoutKind.Sequential)]
        public struct Rect
        {
            public int left;
            public int top;
            public int right;
            public int bottom;
        }

        delegate bool EnumMonitorsDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData);

        private const int ENUM_CURRENT_SETTINGS = -1;

        [StructLayout(LayoutKind.Sequential)]
        private struct DEVMODE
        {
            public short dmSize;
            public int dmPelsWidth;
            public int dmPelsHeight;
        }

        [DllImport("user32.dll")]
        private static extern bool EnumDisplaySettings(string deviceName, int modeNum, ref DEVMODE devMode);


        public static Tuple<int, int> GetScreenResolution(int deviceNumber)
        {
            DEVMODE devMode = default;
            devMode.dmSize = (short)Marshal.SizeOf(devMode);
            EnumDisplaySettings(null, ENUM_CURRENT_SETTINGS, ref devMode);
            return new Tuple<int, int>(devMode.dmPelsWidth, devMode.dmPelsHeight);
        }
    }
}
