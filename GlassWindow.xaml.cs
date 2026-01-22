using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace CustomSearchApp
{
    public class GlassWindow : Window
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct MARGINS
        {
            public int cxLeftWidth;
            public int cxRightWidth;
            public int cyTopHeight;
            public int cyBottomHeight;
        }

        [DllImport("dwmapi.dll")]
        private static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
        private const int DWMWA_SYSTEMBACKDROP_TYPE = 38;
        private const int DWMSBT_MAINWINDOW = 2; // Acrylic effect

        public GlassWindow()
        {
            this.AllowsTransparency = true;
            this.WindowStyle = WindowStyle.None;
            this.Background = Brushes.Transparent;
            this.Loaded += GlassWindow_Loaded;
        }

        private void GlassWindow_Loaded(object sender, RoutedEventArgs e)
        {
            EnableAcrylicBlur();
            EnableDarkMode();
        }

        private void EnableAcrylicBlur()
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            
            // Ustawienie efektu Acrylic
            int backdropType = DWMSBT_MAINWINDOW;
            DwmSetWindowAttribute(hwnd, DWMWA_SYSTEMBACKDROP_TYPE, ref backdropType, sizeof(int));
            
            // Rozszerzenie ramki do obszaru klienckiego
            MARGINS margins = new MARGINS
            {
                cxLeftWidth = -1,
                cxRightWidth = -1,
                cyTopHeight = -1,
                cyBottomHeight = -1
            };
            
            DwmExtendFrameIntoClientArea(hwnd, ref margins);
        }

        private void EnableDarkMode()
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            int useImmersiveDarkMode = 1;
            DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref useImmersiveDarkMode, sizeof(int));
        }
    }
}
