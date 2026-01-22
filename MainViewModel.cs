using System;
using System.Windows;
using System.Windows.Interop;

namespace CustomSearchApp
{
    public partial class MainWindow : GlassWindow
    {
        private HotkeyManager _hotkeyManager;
        
        public MainWindow()
        {
            InitializeComponent();
            SetupHotkey();
        }
        
        private void SetupHotkey()
        {
            _hotkeyManager = new HotkeyManager();
            
            // Pokaż okno przy starcie (tylko do testów)
            Loaded += (s, e) => Show();
        }
        
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            
            // Zarejestruj hotkey
            var windowHelper = new WindowInteropHelper(this);
            _hotkeyManager.Register(windowHelper.Handle);
            
            // Hook dla wiadomości Windows
            var source = HwndSource.FromHwnd(windowHelper.Handle);
            source?.AddHook(HwndHook);
        }
        
        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            
            if (msg == WM_HOTKEY && wParam.ToInt32() == 9000) // Nasz hotkey ID
            {
                // Alt+Space został naciśnięty!
                if (IsVisible)
                {
                    Hide();
                }
                else
                {
                    Show();
                    Activate();
                    Topmost = true;
                    Focus();
                }
                handled = true;
            }
            
            return IntPtr.Zero;
        }
        
        protected override void OnClosed(EventArgs e)
        {
            _hotkeyManager.Unregister();
            base.OnClosed(e);
        }
    }
}
