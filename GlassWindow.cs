// GlassWindow.cs - ulepszona wersja
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

public class GlassWindow : Window
{
    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);
    
    [DllImport("dwmapi.dll")]
    private static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMargins);
    
    private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;
    private const int DWMWA_MICA_EFFECT = 1029;
    private const int DWMWA_SYSTEMBACKDROP_TYPE = 38;
    
    private enum DWM_SYSTEMBACKDROP_TYPE
    {
        DWMSBT_AUTO = 0,
        DWMSBT_NONE = 1,
        DWMSBT_MAINWINDOW = 2,
        DWMSBT_TRANSIENTWINDOW = 3,
        DWMSBT_TABBEDWINDOW = 4
    }
    
    [StructLayout(LayoutKind.Sequential)]
    private struct MARGINS
    {
        public int cxLeftWidth;
        public int cxRightWidth;
        public int cyTopHeight;
        public int cyBottomHeight;
    }
    
    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        
        try
        {
            var hwnd = new WindowInteropHelper(this).Handle;
            
            // Ustaw ciemny tryb
            int darkMode = 1;
            DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref darkMode, sizeof(int));
            
            // Wyłącz systemowe tło
            int backdropType = (int)DWM_SYSTEMBACKDROP_TYPE.DWMSBT_NONE;
            DwmSetWindowAttribute(hwnd, DWMWA_SYSTEMBACKDROP_TYPE, ref backdropType, sizeof(int));
            
            // Rozszerz ramkę do klienta dla przezroczystości
            var margins = new MARGINS
            {
                cxLeftWidth = -1,
                cxRightWidth = -1,
                cyTopHeight = -1,
                cyBottomHeight = -1
            };
            DwmExtendFrameIntoClientArea(hwnd, ref margins);
        }
        catch
        {
            // Fallback: ustaw przezroczystość
            AllowsTransparency = true;
            WindowStyle = WindowStyle.None;
            Background = System.Windows.Media.Brushes.Transparent;
        }
    }
}
