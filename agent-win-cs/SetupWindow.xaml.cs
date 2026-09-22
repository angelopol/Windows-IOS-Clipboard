using System.Windows;

namespace ClipboardAgent
{
    public partial class SetupWindow : Window
    {
        public Config Result { get; private set; }

        public SetupWindow()
        {
            InitializeComponent();
            Loaded += (s, e) => UrlBox.Focus();
        }

        private void OnSave(object sender, RoutedEventArgs e)
        {
            var url = (UrlBox.Text ?? "").Trim().TrimEnd('/');
            var token = (TokenBox.Text ?? "").Trim();
            if (string.IsNullOrWhiteSpace(url) || url == "https:" || string.IsNullOrWhiteSpace(token))
            {
                System.Windows.MessageBox.Show(this, "Rellena la URL del servidor y el token.",
                    "Clipboard", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            Result = new Config { ServerUrl = url, Token = token };
            DialogResult = true;
        }

        private void OnCancel(object sender, RoutedEventArgs e) => DialogResult = false;
    }
}
