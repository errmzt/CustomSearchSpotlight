// Utils/HotkeyManager.cs
using System;
using System.Runtime.InteropServices;
using System.Windows.Input;

public class HotkeyManager
{
    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
    
    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    
    private const int HOTKEY_ID = 9000;
    private IntPtr _windowHandle;
    
    public HotkeyManager(IntPtr windowHandle)
    {
        _windowHandle = windowHandle;
    }
    
    public void RegisterHotkey()
    {
        // Alt+Space
        RegisterHotKey(_windowHandle, HOTKEY_ID, 0x0001, 0x20); // MOD_ALT = 0x0001, VK_SPACE = 0x20
    }
    
    public void UnregisterHotkey()
    {
        UnregisterHotKey(_windowHandle, HOTKEY_ID);
    }
}
