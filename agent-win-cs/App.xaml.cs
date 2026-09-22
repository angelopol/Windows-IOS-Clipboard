using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Drawing = System.Drawing;
using WinForms = System.Windows.Forms;

namespace ClipboardAgent
{
    public partial class App : Application
    {
        private const int HOTKEY_ID = 1;

        private Mutex _mutex;
        private MessageWindow _msg;
        private ApiClient _api;
        private WinForms.NotifyIcon _tray;
        private Config _config;
        private DispatcherTimer _retryTimer;
        private FlyoutWindow _flyout;

        private bool _autoSend = true;
        private string _lastClip = "";
        private readonly List<string> _pending = new List<string>();
        private IntPtr _pasteTarget = IntPtr.Zero;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            WinForms.Application.EnableVisualStyles();

            // Una sola instancia.
            _mutex = new Mutex(true, "ClipboardAgent_SingleInstance", out bool createdNew);
            if (!createdNew) { Shutdown(); return; }

            _config = Config.Load();
            if (_config == null)
            {
                var setup = new SetupWindow();
                if (setup.ShowDialog() != true) { Shutdown(); return; }
                _config = setup.Result;
                _config.Save();
            }

            _api = new ApiClient(_config.ServerUrl, _config.Token);
            _api.Unauthorized += () =>
                Dispatcher.Invoke(() => Notify("Token inválido o revocado. Reconfigura el agente."));

            _msg = new MessageWindow();
            _msg.HotkeyPressed += OnHotkey;
            _msg.ClipboardUpdated += OnClipboardUpdated;

            bool hk = Native.RegisterHotKey(
                _msg.Handle, HOTKEY_ID,
                Native.MOD_CONTROL | Native.MOD_ALT | Native.MOD_NOREPEAT, Native.VK_V);
            Native.AddClipboardFormatListener(_msg.Handle);

            try
            {
                if (System.Windows.Clipboard.ContainsText())
                    _lastClip = System.Windows.Clipboard.GetText();
            }
            catch { }

            SetupTray();
            if (!hk) Notify("No se pudo registrar Ctrl+Alt+V (¿ya está en uso?).");

            _retryTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(15) };
            _retryTimer.Tick += (s, a) => RetryPending();
            _retryTimer.Start();

            // Auto-registro de inicio con Windows la primera vez (solo .exe, no env).
            if (!_config.FromEnv && !_config.AutostartSetup)
            {
                if (Autostart.Enable())
                {
                    _config.AutostartSetup = true;
                    _config.Save();
                }
            }
        }

        // ----------------------------------------------------------- clipboard
        private void OnClipboardUpdated()
        {
            if (!_autoSend) return;
            string text = null;
            try
            {
                if (System.Windows.Clipboard.ContainsText())
                    text = System.Windows.Clipboard.GetText();
            }
            catch { return; }
            if (string.IsNullOrEmpty(text) || text == _lastClip) return;
            _lastClip = text;
            _ = PushAsync(text);
        }

        private async Task PushAsync(string text)
        {
            bool ok = await _api.PostClipAsync(text);
            if (!ok)
            {
                lock (_pending)
                {
                    _pending.Add(text);
                    if (_pending.Count > 50) _pending.RemoveAt(0);
                }
            }
        }

        private void RetryPending()
        {
            List<string> batch;
            lock (_pending)
            {
                if (_pending.Count == 0) return;
                batch = new List<string>(_pending);
                _pending.Clear();
            }
            foreach (var text in batch) _ = RetryOne(text);
        }

        private async Task RetryOne(string text)
        {
            if (!await _api.PostClipAsync(text))
                lock (_pending) { _pending.Add(text); }
        }

        // -------------------------------------------------------------- hotkey
        private async void OnHotkey()
        {
            if (_flyout != null && _flyout.IsVisible) return;
            _pasteTarget = Native.GetForegroundWindow();

            var clips = await _api.GetClipsAsync("shared");
            if (clips == null || clips.Count == 0) return;

            _flyout = new FlyoutWindow(clips);
            _flyout.ItemChosen += OnItemChosen;
            _flyout.Closed += (s, a) => _flyout = null;
            _flyout.Show();
            _flyout.Activate();
        }

        private void OnItemChosen(string text)
        {
            if (_pasteTarget != IntPtr.Zero) Native.SetForegroundWindow(_pasteTarget);
            _lastClip = text; // evita el eco (que el watcher lo reenvíe)
            try { System.Windows.Clipboard.SetText(text); } catch { }

            var t = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(120) };
            t.Tick += (s, a) => { t.Stop(); SendPaste(); };
            t.Start();
        }

        private static void SendPaste()
        {
            Native.keybd_event(Native.VK_CONTROL, 0, 0, UIntPtr.Zero);
            Native.keybd_event(Native.VK_V, 0, 0, UIntPtr.Zero);
            Native.keybd_event(Native.VK_V, 0, Native.KEYEVENTF_KEYUP, UIntPtr.Zero);
            Native.keybd_event(Native.VK_CONTROL, 0, Native.KEYEVENTF_KEYUP, UIntPtr.Zero);
        }

        // ---------------------------------------------------------------- tray
        private void SetupTray()
        {
            _tray = new WinForms.NotifyIcon
            {
                Icon = CreateIcon(),
                Visible = true,
                Text = "Clipboard compartido",
            };

            var menu = new WinForms.ContextMenuStrip();

            var autoItem = new WinForms.ToolStripMenuItem("Auto-enviar")
            { Checked = _autoSend, CheckOnClick = true };
            autoItem.CheckedChanged += (s, e) => _autoSend = autoItem.Checked;

            var startItem = new WinForms.ToolStripMenuItem("Iniciar con Windows")
            { Checked = Autostart.IsEnabled(), CheckOnClick = true };
            startItem.CheckedChanged += (s, e) =>
            {
                bool ok = startItem.Checked ? Autostart.Enable() : Autostart.Disable();
                if (!ok) Notify("No se pudo cambiar el inicio con Windows.");
            };

            var openItem = new WinForms.ToolStripMenuItem("Ver portapapeles (Ctrl+Alt+V)");
            openItem.Click += (s, e) => OnHotkey();

            var exitItem = new WinForms.ToolStripMenuItem("Salir");
            exitItem.Click += (s, e) => ExitApp();

            menu.Items.Add(autoItem);
            menu.Items.Add(startItem);
            menu.Items.Add(openItem);
            menu.Items.Add(new WinForms.ToolStripSeparator());
            menu.Items.Add(exitItem);

            _tray.ContextMenuStrip = menu;
            _tray.DoubleClick += (s, e) => OnHotkey();
        }

        private static Drawing.Icon CreateIcon()
        {
            using var bmp = new Drawing.Bitmap(32, 32);
            using (var g = Drawing.Graphics.FromImage(bmp))
            {
                g.SmoothingMode = Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.Clear(Drawing.Color.Transparent);
                using var body = new Drawing.SolidBrush(Drawing.Color.FromArgb(0x4F, 0x8C, 0xFF));
                g.FillRectangle(body, 8, 7, 16, 19);
                using var clip = new Drawing.SolidBrush(Drawing.Color.White);
                g.FillRectangle(clip, 12, 4, 8, 5);
            }
            return Drawing.Icon.FromHandle(bmp.GetHicon());
        }

        private void Notify(string message)
        {
            try { _tray?.ShowBalloonTip(3000, "Clipboard", message, WinForms.ToolTipIcon.Info); }
            catch { }
        }

        private void ExitApp()
        {
            try { Native.UnregisterHotKey(_msg.Handle, HOTKEY_ID); } catch { }
            try { Native.RemoveClipboardFormatListener(_msg.Handle); } catch { }
            if (_tray != null) { _tray.Visible = false; _tray.Dispose(); }
            _msg?.Dispose();
            Shutdown();
        }
    }
}
