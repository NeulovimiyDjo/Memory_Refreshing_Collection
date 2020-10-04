using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Windows;

namespace QRCopyPaste
{
    public partial class App : Application
    {
        public IServiceProvider ServiceProvider { get; private set; }

        public IConfiguration Configuration { get; private set; }


        protected override void OnStartup(StartupEventArgs e)
        {
            var builder = new ConfigurationBuilder()
             .SetBasePath(Directory.GetCurrentDirectory())
             .AddJsonFile("app.json", optional: false, reloadOnChange: true);

            Configuration = builder.Build();

            ConfigureQRSenderAndLoaderSettings();


            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);

            ServiceProvider = serviceCollection.BuildServiceProvider();

            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }


        private void ConfigureQRSenderAndLoaderSettings()
        {
            var appsettings = Configuration.GetSection("AppSettings");


            QRSenderSettings.ChunkSize = int.Parse(
                appsettings.GetSection(nameof(QRSenderSettings))
                .GetSection(nameof(QRSenderSettings.ChunkSize)).Value
            );

            QRSenderSettings.SendDelayMilliseconds = int.Parse(
                appsettings.GetSection(nameof(QRSenderSettings))
                .GetSection(nameof(QRSenderSettings.SendDelayMilliseconds)).Value
            );


            QRReceiverSettings.ScanPeriodForSettingsMessageMilliseconds = int.Parse(
                appsettings.GetSection(nameof(QRReceiverSettings))
                .GetSection(nameof(QRReceiverSettings.ScanPeriodForSettingsMessageMilliseconds)).Value
            );

            QRReceiverSettings.MaxMillisecondsToContinueSinceLastSuccessfulQRRead = int.Parse(
                appsettings.GetSection(nameof(QRReceiverSettings))
                .GetSection(nameof(QRReceiverSettings.MaxMillisecondsToContinueSinceLastSuccessfulQRRead)).Value
            );
        }


        private void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient(typeof(MainWindow));
        }
    }
}
