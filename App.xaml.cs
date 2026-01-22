using System.Windows;

namespace CustomSearchApp
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Możesz dodać globalne exception handling tutaj
            DispatcherUnhandledException += (sender, args) =>
            {
                MessageBox.Show($"Wystąpił nieoczekiwany błąd:\n{args.Exception.Message}", 
                    "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                args.Handled = true;
            };
        }
    }
}
using CustomSearchApp.Services;
using CustomSearchApp.ViewModels;
using System;
using System.Windows;

namespace CustomSearchApp
{
    public partial class App : Application
    {
        private HotkeyManager _hotkeyManager;
        private MainWindow _mainWindow;
        
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            
            // Utwórz i pokaż główne okno
            _mainWindow = new MainWindow();
            _mainWindow.DataContext = new MainViewModel();
            _mainWindow.Hide(); // Ukryj na starcie
            
            // Zarejestruj globalny hotkey
            _hotkeyManager = new HotkeyManager();
            _hotkeyManager.Register(_mainWindow.GetWindowHandle(), ToggleMainWindow);
            
            // Ustaw zamknięcie hotkeya przy zamykaniu aplikacji
            _mainWindow.Closed += (s, args) => _hotkeyManager.Dispose();
        }
        
        private void ToggleMainWindow()
        {
            if (_mainWindow.IsVisible)
            {
                _mainWindow.Hide();
            }
            else
            {
                _mainWindow.Show();
                _mainWindow.Activate();
                _mainWindow.SearchTextBox.Focus();
            }
        }
    }
}

// Extension method dla Window do pobrania handle
public static class WindowExtensions
{
    [System.Runtime.InteropServices.DllImport("user32.dll")]
    private static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);
    
    public static IntPtr GetWindowHandle(this Window window)
    {
        return new System.Windows.Interop.WindowInteropHelper(window).Handle;
    }
}
