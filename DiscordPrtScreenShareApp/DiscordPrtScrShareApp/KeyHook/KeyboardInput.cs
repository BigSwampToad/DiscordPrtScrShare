using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace DiscordPrtScrShareApp
{
    class KeyboardInput : IDisposable
    {
        private readonly Thread _pumpThread;
        private MessagePump _pump;

        public delegate void KeyPressedHandler(int vKey);
        public event KeyPressedHandler KeyPressed;

        public KeyboardInput()
        {
            _pumpThread = new Thread(StartMessagePumpThread);
            _pumpThread.Start();
        }

        private void StartMessagePumpThread()
        {
            _pump = new MessagePump();
            using (KeyboardHook hook = new KeyboardHook())
            {
                hook.OnKeyPressed += Hook_OnKeyPressed;
                _pump.Start();
                hook.OnKeyPressed -= Hook_OnKeyPressed;
            }
        }

        private void Hook_OnKeyPressed(object sender, int key)
        {
            KeyPressed?.Invoke(key);
        }

        public void Dispose()
        {
            if (_pump == null) return;
            _pump.Stop();
        }
    }
}
