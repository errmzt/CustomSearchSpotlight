using System;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace CustomSearchApp
{
    public class HotkeyManager
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int HOTKEY_ID = 9000;
        private IntPtr _windowHandle;

        public void Register(IntPtr windowHandle)
        {
            _windowHandle = windowHandle;
            
            // Alt+Space = MOD_ALT (0x0001) + VK_SPACE (0x20)
            uint modifier = 0x0001; // Alt
            uint vk = 0x20; // Space
            
            if (!RegisterHotKey(_windowHandle, HOTKEY_ID, modifier, vk))
            {
                throw new Exception("Nie udało się zarejestrować hotkeya");
            }
        }

        public void Unregister()
        {
            if (_windowHandle != IntPtr.Zero)
            {
                UnregisterHotKey(_windowHandle, HOTKEY_ID);
            }
        }
    }
}
