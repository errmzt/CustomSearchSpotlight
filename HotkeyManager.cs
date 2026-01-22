// HotkeyManager.cs - debug version
public class HotkeyManager
{
    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
    
    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    
    public void Register(IntPtr windowHandle)
    {
        try
        {
            // Alt = 0x0001, Space = 0x20 (VK_SPACE)
            bool success = RegisterHotKey(windowHandle, 1, 0x0001, 0x20);
            
            if (!success)
            {
                // Spróbuj z Ctrl+Space
                success = RegisterHotKey(windowHandle, 1, 0x0002, 0x20);
                
                if (!success)
                {
                    // Spróbuj z Win+Space
                    success = RegisterHotKey(windowHandle, 1, 0x0008, 0x20);
                }
            }
            
            Console.WriteLine($"Hotkey registration: {success}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Hotkey error: {ex.Message}");
        }
    }
}
using System;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Interop;

namespace CustomSearchApp.Services
{
    public class HotkeyManager : IDisposable
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int HOTKEY_ID = 9000;
        private const uint MOD_ALT = 0x0001;
        private const uint VK_SPACE = 0x20;

        private IntPtr _windowHandle;
        private HwndSource _source;
        private Action _hotkeyAction;

        public void Register(IntPtr windowHandle, Action hotkeyAction)
        {
            _windowHandle = windowHandle;
            _hotkeyAction = hotkeyAction;
            
            // Rejestracja źródła wiadomości okna
            _source = HwndSource.FromHwnd(windowHandle);
            _source.AddHook(HwndHook);

            // Rejestracja hotkeya
            if (!RegisterHotKey(windowHandle, HOTKEY_ID, MOD_ALT, VK_SPACE))
            {
                throw new InvalidOperationException("Nie można zarejestrować skrótu klawiszowego.");
            }
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            
            if (msg == WM_HOTKEY && wParam.ToInt32() == HOTKEY_ID)
            {
                _hotkeyAction?.Invoke();
                handled = true;
            }
            
            return IntPtr.Zero;
        }

        public void Dispose()
        {
            if (_source != null)
            {
                _source.RemoveHook(HwndHook);
                _source.Dispose();
                _source = null;
            }
            
            if (_windowHandle != IntPtr.Zero)
            {
                UnregisterHotKey(_windowHandle, HOTKEY_ID);
                _windowHandle = IntPtr.Zero;
            }
        }
    }
}
