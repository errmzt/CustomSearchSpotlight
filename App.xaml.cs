using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using CustomSearchSpotlight.Services;
using CustomSearchSpotlight.ViewModels;

namespace CustomSearchSpotlight
{
    public partial class App : Application
    {
        private IServiceProvider _serviceProvider;

        public App()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // View Models
            services.AddSingleton<MainViewModel>();

            // Services
            services.AddSingleton<IAIService, GeminiService>();
            services.AddSingleton<ISearchService, WindowsSearchService>();
            services.AddSingleton<IVoiceService, WindowsVoiceService>();
            services.AddSingleton<HotkeyManager>();
            services.AddSingleton<SettingsManager>();

            // Main Window
            services.AddSingleton<MainWindow>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
    }
}
