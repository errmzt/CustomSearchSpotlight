// Program.cs
using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CustomSearchSpotlight.Services;
using CustomSearchSpotlight.ViewModels;
using CustomSearchSpotlight.Views;

namespace CustomSearchSpotlight
{
    public partial class App : Application
    {
        private IHost _host;

        public App()
        {
            _host = Host.CreateDefaultBuilder()
                .ConfigureServices((context, services) =>
                {
                    // ViewModels
                    services.AddSingleton<MainViewModel>();
                    
                    // Services
                    services.AddSingleton<IAIService, GeminiService>();
                    services.AddSingleton<ISearchService, WindowsSearchService>();
                    services.AddSingleton<IVoiceService, WindowsVoiceService>();
                    services.AddSingleton<HotkeyManager>();
                    services.AddSingleton<SettingsManager>();
                    
                    // Views
                    services.AddSingleton<MainWindow>();
                })
                .Build();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            await _host.StartAsync();
            
            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
            
            // Zarejestruj hotkey
            var hotkeyManager = _host.Services.GetRequiredService<HotkeyManager>();
            hotkeyManager.Register();
            
            base.OnStartup(e);
        }

        protected override async void OnExit(ExitEventArgs e)
        {
            using (_host)
            {
                await _host.StopAsync(TimeSpan.FromSeconds(5));
            }
            base.OnExit(e);
        }
    }
}
