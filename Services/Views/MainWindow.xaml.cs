// Views/MainWindow.xaml.cs
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using CustomSearchSpotlight.ViewModels;

namespace CustomSearchSpotlight.Views
{
    public partial class MainWindow : Window
    {
        private const int WM_HOTKEY = 0x0312;
        private const int HOTKEY_ID = 9000;
        
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
            
            SetupWindowProperties();
            SetupGlassEffect();
        }

        private void SetupWindowProperties()
        {
            // Rozmiar jak chciałeś
            Width = 750;
            Height = 850;
            
            // Przezroczystość i styl
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            
            // Pozycja
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
            
            // Cień
            DropShadowEffect = new System.Windows.Media.Effects.DropShadowEffect
            {
                Color = Colors.Black,
                BlurRadius = 30,
                Opacity = 0.5,
                ShadowDepth = 0
            };
            
            // Esc zamyka
            PreviewKeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape)
                {
                    Hide();
                }
                else if (e.Key == Key.Enter && SearchBox.IsFocused)
                {
                    (DataContext as MainViewModel)?.ExecuteSearchCommand.Execute(null);
                }
            };
            
            // Auto-hide przy straceniu focusu
            Deactivated += (s, e) => Hide();
        }

        private void SetupGlassEffect()
        {
            // Wymuś acrylic blur przez Win32 API
            var windowHelper = new WindowInteropHelper(this);
            EnableAcrylicBlur(windowHelper.Handle);
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            
            // Hook dla hotkeyów
            var source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
            source?.AddHook(HwndHook);
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_HOTKEY && wParam.ToInt32() == HOTKEY_ID)
            {
                // Alt+Space pokazuje/ukrywa
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

        [DllImport("user32.dll")]
        internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        private void EnableAcrylicBlur(IntPtr hwnd)
        {
            var accent = new AccentPolicy
            {
                AccentState = AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND,
                GradientColor = 0x99000000 // #99000000 z alpha
            };

            var accentStructSize = Marshal.SizeOf(accent);
            var accentPtr = Marshal.AllocHGlobal(accentStructSize);
            Marshal.StructureToPtr(accent, accentPtr, false);

            var data = new WindowCompositionAttributeData
            {
                Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY,
                SizeOfData = accentStructSize,
                Data = accentPtr
            };

            SetWindowCompositionAttribute(hwnd, ref data);
            Marshal.FreeHGlobal(accentPtr);
        }

        internal enum AccentState
        {
            ACCENT_DISABLED = 0,
            ACCENT_ENABLE_GRADIENT = 1,
            ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
            ACCENT_ENABLE_BLURBEHIND = 3,
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
