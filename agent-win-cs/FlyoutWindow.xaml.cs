using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Threading;

namespace ClipboardAgent
{
    public partial class FlyoutWindow : Window
    {
        public event Action<string> ItemChosen;
        private readonly List<ClipItem> _items;
        private bool _chosen;
        private bool _allowClose;

        public FlyoutWindow(List<ClipItem> items)
        {
            InitializeComponent();
            _items = items ?? new List<ClipItem>();
            List.ItemsSource = _items;
            if (_items.Count > 0) List.SelectedIndex = 0;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            PositionNearCursor();
            ForceForeground();

            // Evita que un "deactivate" transitorio al robar el foco cierre la
            // ventana nada más abrirse.
            var t = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(250) };
            t.Tick += (s, a) => { t.Stop(); _allowClose = true; };
            t.Start();
        }

        // Trae la ventana al primer plano y le da el foco de teclado de forma
        // fiable, aunque la lance una app en segundo plano (hotkey global).
        private void ForceForeground()
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            IntPtr fg = Native.GetForegroundWindow();
            uint fgThread = Native.GetWindowThreadProcessId(fg, out _);
            uint thisThread = Native.GetCurrentThreadId();

            if (fg != IntPtr.Zero && fgThread != thisThread)
            {
                Native.AttachThreadInput(thisThread, fgThread, true);
                Native.SetForegroundWindow(hwnd);
                Native.BringWindowToTop(hwnd);
                Native.AttachThreadInput(thisThread, fgThread, false);
            }
            else
            {
                Native.SetForegroundWindow(hwnd);
            }

            Activate();
            List.Focus();
            if (_items.Count > 0)
            {
                var c = List.ItemContainerGenerator
                    .ContainerFromIndex(List.SelectedIndex) as ListBoxItem;
                if (c != null) c.Focus();
                else Keyboard.Focus(List);
            }
        }

        private void PositionNearCursor()
        {
            Native.GetCursorPos(out var p);
            var dpi = VisualTreeHelper.GetDpi(this);
            double x = p.X / dpi.DpiScaleX;
            double y = p.Y / dpi.DpiScaleY;

            var area = SystemParameters.WorkArea; // en DIPs
            double w = ActualWidth, h = ActualHeight;

            double left = x, top = y;
            if (left + w > area.Right) left = area.Right - w;
            if (top + h > area.Bottom) top = area.Bottom - h;
            if (left < area.Left) left = area.Left;
            if (top < area.Top) top = area.Top;

            Left = left;
            Top = top;
        }

        private void Choose()
        {
            if (_chosen) return;
            if (List.SelectedItem is ClipItem it)
            {
                _chosen = true;
                ItemChosen?.Invoke(it.Text ?? "");
                Close();
            }
        }

        // Un solo clic sobre un elemento lo pega (como el flyout de Win11).
        private void OnListClick(object sender, MouseButtonEventArgs e)
        {
            var dep = e.OriginalSource as DependencyObject;
            while (dep != null && !(dep is ListBoxItem))
                dep = VisualTreeHelper.GetParent(dep);

            if (dep is ListBoxItem lbi)
            {
                int idx = List.ItemContainerGenerator.IndexFromContainer(lbi);
                if (idx >= 0)
                {
                    List.SelectedIndex = idx;
                    Choose();
                    e.Handled = true;
                }
            }
        }

        private void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    Close();
                    e.Handled = true;
                    break;
                case Key.Enter:
                    Choose();
                    e.Handled = true;
                    break;
                case Key.Tab:
                    MoveSelection((Keyboard.Modifiers & ModifierKeys.Shift) != 0 ? -1 : 1);
                    e.Handled = true;
                    break;
                default:
                    if (e.Key >= Key.D1 && e.Key <= Key.D9)
                    {
                        int idx = e.Key - Key.D1;
                        if (idx < _items.Count)
                        {
                            List.SelectedIndex = idx;
                            Choose();
                            e.Handled = true;
                        }
                    }
                    break;
            }
        }

        private void MoveSelection(int dir)
        {
            int n = _items.Count;
            if (n == 0) return;
            int idx = List.SelectedIndex + dir;
            if (idx < 0) idx = n - 1;
            if (idx >= n) idx = 0;
            List.SelectedIndex = idx;
            (List.ItemContainerGenerator.ContainerFromIndex(idx) as ListBoxItem)?.Focus();
        }

        private void OnHeaderMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Arrastrar la cabecera mueve la ventana (no tiene barra de título).
            if (e.ChangedButton == MouseButton.Left)
            {
                try { DragMove(); } catch { }
            }
        }

        private void OnDeactivated(object sender, EventArgs e)
        {
            if (_allowClose) Close();
        }
    }
}
