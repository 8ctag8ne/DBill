using CoreLib.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;

namespace DBill.WpfApp
{
    // App.xaml.cs або точка входу
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Ініціалізація сервісів
            var localStorage = new LocalFileStorage();
            var fileService = new FileService(localStorage, "uploads");
            var tempFileService = new FileService(localStorage, "tempFiles");
            var databaseStorage = new JsonDatabaseStorageService(localStorage, fileService);
            var tempDatabaseStorage = new JsonDatabaseStorageService(localStorage, tempFileService);
            var databaseService = new DatabaseService(databaseStorage, tempDatabaseStorage, fileService);
            var tableService = new TableService(databaseService);

            var mainWindow = new MainWindow(databaseService, tableService, fileService);
            mainWindow.Show();
        }
    }
}