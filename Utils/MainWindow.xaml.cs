protected override void OnSourceInitialized(EventArgs e)
{
    base.OnSourceInitialized(e);
    
    var windowHandle = new System.Windows.Interop.WindowInteropHelper(this).Handle;
    var hotkeyManager = new HotkeyManager(windowHandle);
    hotkeyManager.RegisterHotkey();
    
    // Obsługa wiadomości hotkey
    System.Windows.Interop.HwndSource.FromHwnd(windowHandle).AddHook(HwndHook);
}

private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
{
    const int WM_HOTKEY = 0x0312;
    
    if (msg == WM_HOTKEY && wParam.ToInt32() == 9000)
    {
        // Pokaż/ukryj okno
        if (IsVisible)
            Hide();
        else
        {
            Show();
            Activate();
            SearchBox.Focus();
        }
        
        handled = true;
    }
    
    return IntPtr.Zero;
}
