using CoreLib.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;

namespace DBill.WpfApp
{
    public partial class App : Application
    {
        public static IServiceProvider ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += (s, ex) =>
            {
                var msg = ex.ExceptionObject is Exception exc ? exc.ToString() : ex.ExceptionObject?.ToString() ?? "Unknown error";
                MessageBox.Show(msg, "Unhandled Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            };
            this.DispatcherUnhandledException += (s, ex) =>
            {
                MessageBox.Show(ex.Exception.ToString(), "Unhandled UI Exception", MessageBoxButton.OK, MessageBoxImage.Error);
                ex.Handled = true;
            };

            var services = new ServiceCollection();
            // Реєстрація сервісів
            services.AddSingleton<IFileStorageService, LocalFileStorageService>();
            services.AddSingleton<IDatabaseStorageService, JsonDatabaseStorageService>();
            services.AddSingleton<DatabaseService>();
            services.AddSingleton<TableService>();
            services.AddSingleton<FileService>();
            services.AddSingleton<MainWindow>();

            ServiceProvider = services.BuildServiceProvider();

            var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }
    }
}