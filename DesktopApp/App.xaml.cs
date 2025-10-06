//DesktopApp/App.xaml.cs
using CoreLib.Services;
using System.Windows;

namespace DBill.WpfApp
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Ініціалізація сервісів
            var localStorage = new LocalFileStorage();
            
            // Основний FileService для постійних файлів
            var fileService = new FileService(localStorage, "uploads");
            var databaseStorage = new JsonDatabaseStorageService(localStorage, fileService);

            // Тимчасовий FileService для ізольованого завантаження
            var tempFileService = new FileService(localStorage, "tempFiles");
            var tempDatabaseStorage = new JsonDatabaseStorageService(localStorage, tempFileService);
            
            // DatabaseService отримує обидва сервіси
            var databaseService = new DatabaseService(
                databaseStorage, 
                tempDatabaseStorage, 
                fileService,
                tempFileService
            );
            
            var tableService = new TableService(databaseService);

            var mainWindow = new MainWindow(databaseService, tableService, fileService);
            mainWindow.Show();
        }
    }
}