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
