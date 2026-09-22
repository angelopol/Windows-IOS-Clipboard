using System;
using System.Windows.Interop;

namespace ClipboardAgent
{
    // Ventana oculta (nunca visible) para recibir WM_HOTKEY y WM_CLIPBOARDUPDATE.
    public class MessageWindow : IDisposable
    {
        private readonly HwndSource _src;

        public IntPtr Handle => _src.Handle;
        public event Action HotkeyPressed;
        public event Action ClipboardUpdated;

        public MessageWindow()
        {
            var p = new HwndSourceParameters("ClipboardAgentMsgWnd")
            {
                Width = 1,
                Height = 1,
                PositionX = -10000,
                PositionY = -10000,
                WindowStyle = 0, // sin WS_VISIBLE
            };
            _src = new HwndSource(p);
            _src.AddHook(WndProc);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == Native.WM_HOTKEY)
            {
                HotkeyPressed?.Invoke();
                handled = true;
            }
            else if (msg == Native.WM_CLIPBOARDUPDATE)
            {
                ClipboardUpdated?.Invoke();
                handled = true;
            }
            return IntPtr.Zero;
        }

        public void Dispose()
        {
            _src?.RemoveHook(WndProc);
            _src?.Dispose();
        }
    }
}
