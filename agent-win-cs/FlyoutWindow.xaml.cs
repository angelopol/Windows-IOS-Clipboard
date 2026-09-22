using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace ClipboardAgent
{
    public partial class FlyoutWindow : Window
    {
        public event Action<string> ItemChosen;
        private readonly List<ClipItem> _items;
        private bool _chosen;

        public FlyoutWindow(List<ClipItem> items)
        {
            InitializeComponent();
            _items = items ?? new List<ClipItem>();
            List.ItemsSource = _items;
            if (_items.Count > 0) List.SelectedIndex = 0;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Activate();
            List.Focus();
            Keyboard.Focus(List);
            PositionNearCursor();
        }

        private void PositionNearCursor()
        {
            Native.GetCursorPos(out var p);
            var dpi = VisualTreeHelper.GetDpi(this);
            double x = p.X / dpi.DpiScaleX;
            double y = p.Y / dpi.DpiScaleY;

            var area = SystemParameters.WorkArea; // en DIPs
            double w = ActualWidth, h = ActualHeight;

            double left = x;
            double top = y;
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

        private void OnItemActivated(object sender, MouseButtonEventArgs e) => Choose();

        private void OnHeaderMouseDown(object sender, MouseButtonEventArgs e)
        {
            // Arrastrar la cabecera mueve la ventana (no tiene barra de título).
            if (e.ChangedButton == MouseButton.Left)
            {
                try { DragMove(); } catch { }
            }
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Close();
                e.Handled = true;
            }
            else if (e.Key == Key.Enter)
            {
                Choose();
                e.Handled = true;
            }
            else if (e.Key >= Key.D1 && e.Key <= Key.D9)
            {
                int idx = e.Key - Key.D1;
                if (idx < _items.Count)
                {
                    List.SelectedIndex = idx;
                    Choose();
                    e.Handled = true;
                }
            }
        }

        private void OnDeactivated(object sender, EventArgs e) => Close();
    }
}
