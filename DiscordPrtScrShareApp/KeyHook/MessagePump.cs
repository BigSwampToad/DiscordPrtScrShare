using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace DiscordPrtScrShareApp
{
    public class MessagePump
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool PeekMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);

        [DllImport("user32.dll")]
        private static extern bool TranslateMessage([In] ref MSG lpMsg);

        [DllImport("user32.dll")]
        private static extern IntPtr DispatchMessage([In] ref MSG lpmsg);

        private const int PM_REMOVE = 0x0001;

        private bool _isRunning;

        private struct POINT
        {
            public int X { get; }
            public int Y { get; }
            public POINT(int x, int y)
            {
                X = x;
                Y = y;
            }
        }


        [StructLayout(LayoutKind.Sequential)]
        private struct MSG
        {
            IntPtr hwnd;
            uint message;
            UIntPtr wParam;
            IntPtr lParam;
            int time;
            POINT pt;
        }

        public void Start()
        {
            _isRunning = true;
            StartMessagePump();
        }

        public void Stop()
        {
            _isRunning = false;
        }

        private void StartMessagePump()
        {
            while (_isRunning)
            {
                var foundMessage = PeekMessage(out MSG msg, IntPtr.Zero, 0, 0, PM_REMOVE);
                if (foundMessage)
                {
                    TranslateMessage(ref msg);
                    DispatchMessage(ref msg);
                }
                else
                {
                    Thread.Sleep(1);
                }

            }
        }
    }
}