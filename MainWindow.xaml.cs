using System;
using System.Windows;
using System.Windows.Interop;
using System.Runtime.InteropServices;

namespace CustomSearchApp
{
    public partial class MainWindow : Window
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
        
        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        
        private const int HOTKEY_ID = 1;
        private IntPtr _windowHandle;
        
        public MainWindow()
        {
            InitializeComponent();
        }
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Get window handle for hotkey
            _windowHandle = new WindowInteropHelper(this).Handle;
            
            // Register Alt+Space hotkey
            RegisterHotKey(_windowHandle, HOTKEY_ID, 0x0001, 0x20); // MOD_ALT, VK_SPACE
            
            // Setup window hook for hotkey messages
            var source = HwndSource.FromHwnd(_windowHandle);
            source?.AddHook(HwndHook);
            
            StatusText.Text = "Status: Uruchomiony (Alt+Space)";
        }
        
        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            
            if (msg == WM_HOTKEY && wParam.ToInt32() == HOTKEY_ID)
            {
                // Toggle window visibility
                if (Visibility == Visibility.Visible)
                {
                    Hide();
                }
                else
                {
                    Show();
                    Activate();
                    Topmost = true;
                }
                handled = true;
            }
            return IntPtr.Zero;
        }
        
        private void TestHotkey_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Hotkey zarejestrowany!\nNaciśnij Alt+Space by pokazać/ukryć okno.");
        }
        
        private void TestGlass_Click(object sender, RoutedEventArgs e)
        {
            // Change opacity to show glass effect
            this.Opacity = this.Opacity == 1.0 ? 0.9 : 1.0;
            StatusText.Text = $"Status: Przezroczystość: {this.Opacity * 100}%";
        }
        
        private void TestAI_Click(object sender, RoutedEventArgs e)
        {
            StatusText.Text = "Status: AI Mock Response...";
            
            // Simulate AI response
            MessageBox.Show("🤖 Gemini AI Mock Response:\n\n" +
                          "Cześć! To jest testowa odpowiedź AI.\n\n" +
                          "Aby użyć prawdziwego Gemini:\n" +
                          "1. Zdobądź klucz API z Google AI Studio\n" +
                          "2. Dodaj go do appsettings.json\n" +
                          "3. Uruchom pełną wersję aplikacji");
            
            StatusText.Text = "Status: Gotowy";
        }
        
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        
        protected override void OnClosed(EventArgs e)
        {
            // Cleanup hotkey
            if (_windowHandle != IntPtr.Zero)
            {
                UnregisterHotKey(_windowHandle, HOTKEY_ID);
            }
            base.OnClosed(e);
        }
    }
}
