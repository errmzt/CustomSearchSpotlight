using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using CustomSearchSpotlight.ViewModels;
using System.Runtime.InteropServices;

namespace CustomSearchSpotlight
{
    public partial class MainWindow : Window
    {
        private const int WM_HOTKEY = 0x0312;
        private const int HOTKEY_ID = 9000;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
            SetupWindow();
        }

        private void SetupWindow()
        {
            // Set the window to be transparent and without borders
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = System.Windows.Media.Brushes.Transparent;

            // Set size and position
            Width = 750;
            Height = 850;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            // Register hotkey
            var hwnd = new WindowInteropHelper(this).Handle;
            HotkeyManager.RegisterHotKey(hwnd, HOTKEY_ID, 0x0001, 0x20); // Alt+Space

            // Listen for hotkey
            HwndSource source = HwndSource.FromHwnd(hwnd);
            source.AddHook(HwndHook);
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY && wParam.ToInt32() == HOTKEY_ID)
            {
                // Toggle window visibility
                if (IsVisible)
                {
                    Hide();
                }
                else
                {
                    Show();
                    Activate();
                    Topmost = true;
                    SearchBox.Focus();
                }
                handled = true;
            }
            return IntPtr.Zero;
        }

        private void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Hide();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            // Unregister hotkey
            var hwnd = new WindowInteropHelper(this).Handle;
            HotkeyManager.UnregisterHotKey(hwnd, HOTKEY_ID);
            base.OnClosed(e);
        }
    }

    internal static class HotkeyManager
    {
        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    }
}
