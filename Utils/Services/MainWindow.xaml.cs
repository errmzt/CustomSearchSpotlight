// W MainWindow.xaml.cs
protected override void OnActivated(EventArgs e)
{
    base.OnActivated(e);
    
    // Ustawienie przezroczystości
    SetWindowBlur(this);
}

private void SetWindowBlur(Window window)
{
    var windowHelper = new System.Windows.Interop.WindowInteropHelper(window);
    
    var accent = new AccentPolicy();
    accent.AccentState = AccentState.ACCENT_ENABLE_ACRYLICBLURBEHIND;
    accent.GradientColor = 0x99000000; // #99000000
    
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

// Win32 API interop
[DllImport("user32.dll")]
internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

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
