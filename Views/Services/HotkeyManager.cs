// Services/HotkeyManager.cs
using System;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace CustomSearchSpotlight.Services
{
    public class HotkeyManager
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private const int HOTKEY_ID = 9000;
        private IntPtr _windowHandle;

        public HotkeyManager()
        {
            // Alt+Space jako domyślny hotkey
            HotkeyModifier = KeyModifier.Alt;
            HotkeyKey = Key.Space;
        }

        public KeyModifier HotkeyModifier { get; set; }
        public Key HotkeyKey { get; set; }

        public void Register(IntPtr windowHandle)
        {
            _windowHandle = windowHandle;
            uint modifier = 0;

            if (HotkeyModifier.HasFlag(KeyModifier.Alt)) modifier |= 0x0001;
            if (HotkeyModifier.HasFlag(KeyModifier.Ctrl)) modifier |= 0x0002;
            if (HotkeyModifier.HasFlag(KeyModifier.Shift)) modifier |= 0x0004;
            if (HotkeyModifier.HasFlag(KeyModifier.Win)) modifier |= 0x0008;

            uint vk = (uint)KeyInterop.VirtualKeyFromKey(HotkeyKey);

            if (!RegisterHotKey(_windowHandle, HOTKEY_ID, modifier, vk))
            {
                throw new InvalidOperationException("Failed to register hotkey");
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

    [Flags]
    public enum KeyModifier
    {
        None = 0,
        Alt = 1,
        Ctrl = 2,
        Shift = 4,
        Win = 8
    }
}
