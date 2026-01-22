using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;

namespace CustomSearchApp
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            SetupWindow();
        }

        private void SetupWindow()
        {
            // Ustaw pozycję (wyśrodkowane)
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            
            // Obsługa klawiszy
            PreviewKeyDown += MainWindow_PreviewKeyDown;
            
            // Auto-hide przy straceniu focusu
            Deactivated += (s, e) => Hide();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Wymuś acrylic efekt po załadowaniu
            EnableAcrylicBlur();
        }

        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // ESC zamyka okno
            if (e.Key == Key.Escape)
            {
                Hide();
            }
        }

        // ============================
        // GLASS/ACRYLIC EFFECT - MAGIA!
        // ============================
        
        [DllImport("user32.dll")]
        internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        private void EnableAcrylicBlur()
        {
            try
            {
                var windowHelper = new WindowInteropHelper(this);
                var accent = new AccentPolicy();
                
                // Ustawienie acrylic blur
                accent.AccentState = AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND;
                accent.GradientColor = 0x99000000; // Kolor z alpha: #99000000
                
                var accentStructSize = Marshal.SizeOf(accent);
                var accentPtr = Marshal.AllocHGlobal(accentStructSize);
                Marshal.StructureToPtr(accent, accentPtr, false);
                
                var data = new WindowCompositionAttributeData();
                data.Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY;
                data.SizeOfData = accentStructSize;
                data.Data = accentPtr;
                
                SetWindowCompositionAttribute(windowHelper.Handle, ref data);
                
                Marshal.FreeHGlobal(accentPtr);
            }
            catch (Exception ex)
            {
                // Jeśli acrylic nie działa, pokaż komunikat
                SearchBox.Text = $"Glass efekt: {ex.Message}";
            }
        }

        // Struktury dla Win32 API
        internal enum AccentState
        {
            ACCENT_ENABLE_ACRYLICBLURBEHIND = 4
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct AccentPolicy
        {
            public AccentState AccentState;
            public int AccentFlags;
            public int GradientColor;
            public int AnimationId;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct WindowCompositionAttributeData
        {
            public WindowCompositionAttribute Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }

        internal enum WindowCompositionAttribute
        {
            WCA_ACCENT_POLICY = 19
        }
    }
}
