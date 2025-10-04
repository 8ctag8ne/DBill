using CoreLib.Models;

namespace CoreLib.Services
{
    public class DatabaseService
    {
        private readonly IDatabaseStorageService _storageService;
        private readonly IDatabaseStorageService _tempStorageService;
        private readonly FileService _fileService;
        private Database? _currentDatabase;

        public Database? CurrentDatabase
        { 
            get => _currentDatabase;
            set => _currentDatabase = value;
        }

        public DatabaseService(
            IDatabaseStorageService storageService, 
            IDatabaseStorageService tempStorageService,
            FileService fileService)
        {
            _storageService = storageService ?? throw new ArgumentNullException(nameof(storageService));
            _tempStorageService = tempStorageService ?? throw new ArgumentNullException(nameof(tempStorageService));
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        }

        public Database CreateDatabase(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Database name cannot be null or empty", nameof(name));

            _currentDatabase = new Database(name);
            return _currentDatabase;
        }

        /// <summary>
        /// Безпечно завантажує базу даних. У разі помилки поточна база залишається незмінною.
        /// </summary>
        public async Task<Database> LoadDatabaseAsync(string filePath)
        {
            // Завантажуємо в тимчасове сховище
            var tempDatabase = await _tempStorageService.LoadDatabaseAsync(filePath);

            // Валідація
            var validation = tempDatabase.Validate();
            if (!validation.IsValid)
                throw new InvalidOperationException($"Database validation failed: {string.Join(", ", validation.Errors)}");

            // Зберігаємо файли на диск у тимчасовому контексті
            await SaveFileContentsForDatabaseAsync(tempDatabase);

            // Якщо все успішно - очищаємо стару базу і копіюємо нову
            await CloseDatabase();
            _currentDatabase = tempDatabase;

            return _currentDatabase;
        }

        public async Task SaveDatabaseAsync(string filePath)
        {
            if (_currentDatabase == null)
                throw new InvalidOperationException("No database is currently loaded");

            var validation = _currentDatabase.Validate();
            if (!validation.IsValid)
                throw new InvalidOperationException($"Cannot save invalid database: {string.Join(", ", validation.Errors)}");

            await _storageService.SaveDatabaseAsync(_currentDatabase, filePath);
        }

        public List<string> GetTableNames()
        {
            return _currentDatabase?.GetTableNames() ?? new List<string>();
        }

        public Table? GetTable(string tableName)
        {
            return _currentDatabase?.GetTable(tableName);
        }

        public bool TableExists(string tableName)
        {
            return _currentDatabase?.TableExists(tableName) ?? false;
        }

        public void CreateTable(Table table)
        {
            if (_currentDatabase == null)
                throw new InvalidOperationException("No database is currently loaded");

            _currentDatabase.AddTable(table);
        }

        public bool DeleteTable(string tableName)
        {
            if (_currentDatabase == null)
                throw new InvalidOperationException("No database is currently loaded");

            var table = _currentDatabase.GetTable(tableName);
            if (table != null)
            {
                DeleteTableFilesAsync(table).Wait();
            }

            return _currentDatabase.RemoveTable(tableName);
        }

        public DatabaseStatistics GetStatistics()
        {
            if (_currentDatabase == null)
                throw new InvalidOperationException("No database is currently loaded");

            return _currentDatabase.GetStatistics();
        }

        public ValidationResult ValidateCurrentDatabase()
        {
            if (_currentDatabase == null)
                return ValidationResult.Failure("No database is currently loaded");

            return _currentDatabase.Validate();
        }

        public async Task CloseDatabase()
        {
            if (_currentDatabase != null)
            {
                await _fileService.CleanupAllFilesAsync();
                _currentDatabase = null;
            }
        }

        private async Task SaveFileContentsForDatabaseAsync(Database database)
        {
            foreach (var table in database.Tables)
            {
                foreach (var column in table.Columns.Where(c => c.Type == DataType.TextFile))
                {
                    var rows = table.GetAllRows();
                    for (int i = 0; i < rows.Count; i++)
                    {
                        if (rows[i][column.Name] is FileRecord fileRecord && 
                            fileRecord.Content != null && fileRecord.Content.Length > 0)
                        {
                            try
                            {
                                var storagePath = await _fileService.SaveFileAsync(
                                    fileRecord.Content, 
                                    fileRecord.FileName);
                                
                                fileRecord.StoragePath = storagePath;
                                fileRecord.Content = null;
                            }
                            catch (Exception ex)
                            {
                                throw new InvalidOperationException(
                                    $"Failed to save file content: {fileRecord.FileName}", ex);
                            }
                        }
                    }
                }
            }
        }

        private async Task DeleteTableFilesAsync(Table table)
        {
            foreach (var column in table.Columns.Where(c => c.Type == DataType.TextFile))
            {
                var rows = table.GetAllRows();
                foreach (var row in rows)
                {
                    if (row[column.Name] is FileRecord fileRecord && 
                        !string.IsNullOrWhiteSpace(fileRecord.StoragePath))
                    {
                        try
                        {
                            await _fileService.DeleteFileAsync(fileRecord.StoragePath);
                        }
                        catch { }
                    }
                }
            }
        }
    }
}