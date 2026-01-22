using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace CustomSearchApp
{
    public class GlassWindow : Window
    {
        public GlassWindow()
        {
            // Podstawowe ustawienia
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            Topmost = true;
            
            // Obsługa klawiszy
            PreviewKeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape) Hide();
            };
            
            // Auto-hide
            Deactivated += (s, e) => Hide();
        }
        
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            
            // Włącz acrylic blur po inicjalizacji okna
            EnableAcrylicBlur();
        }
        
        private void EnableAcrylicBlur()
        {
            var windowHelper = new WindowInteropHelper(this);
            
            // Jeśli handle jest dostępny, użyj acrylic
            if (windowHelper.Handle != IntPtr.Zero)
            {
                var accent = new AccentPolicy();
                accent.AccentState = AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND;
                accent.GradientColor = 0x99000000; // Półprzezroczyste czarne tło
                
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
        }
        
        [DllImport("user32.dll")]
        internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);
        
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
