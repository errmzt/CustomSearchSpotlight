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
