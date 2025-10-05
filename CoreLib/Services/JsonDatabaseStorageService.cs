// CoreLib/Services/JsonDatabaseStorageService.cs
using System.Text.Json;
using CoreLib.Models;
using CoreLib.Serialization;

namespace CoreLib.Services
{
    public class JsonDatabaseStorageService : IDatabaseStorageService
    {
        private readonly IFileStorageService _fileStorage;
        private readonly FileService _fileService;
        private readonly JsonSerializerOptions _jsonOptions;

        public JsonDatabaseStorageService(IFileStorageService fileStorage, FileService fileService)
        {
            _fileStorage = fileStorage ?? throw new ArgumentNullException(nameof(fileStorage));
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters = 
                {
                    new DatabaseJsonConverter(),
                    new IntegerIntervalJsonConverter(),
                    new FileRecordJsonConverter()
                }
            };
        }

        public async Task<Database> LoadDatabaseAsync(string filePath, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

            if (!await _fileStorage.ExistsAsync(filePath))
                throw new FileNotFoundException($"Database file not found: {filePath}");

            try
            {
                var jsonBytes = await _fileStorage.ReadAllBytesAsync(filePath);
                var database = JsonSerializer.Deserialize<Database>(jsonBytes, _jsonOptions);
                
                if (database == null)
                    throw new InvalidOperationException("Failed to deserialize database");

                return database;
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Invalid database file format: {ex.Message}", ex);
            }
        }

        public async Task SaveDatabaseAsync(Database database, string filePath, CancellationToken ct = default)
        {
            if (database == null) 
                throw new ArgumentNullException(nameof(database));
            if (string.IsNullOrWhiteSpace(filePath)) 
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

            try
            {
                // Завантажуємо всі файли в пам'ять перед серіалізацією
                await LoadAllFileContentsForSave(database, ct);

                // Серіалізуємо все одним махом
                var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(database, _jsonOptions);
                
                // Зберігаємо у файл
                await _fileStorage.WriteAllBytesAsync(filePath, jsonBytes);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to save database to file: {ex.Message}", ex);
            }
        }

        private async Task LoadAllFileContentsForSave(Database database, CancellationToken ct)
        {
            foreach (var table in database.Tables)
            {
                foreach (var column in table.Columns)
                {
                    if (column.Type != DataType.TextFile)
                        continue;

                    foreach (var value in column.Values)
                    {
                        if (ct.IsCancellationRequested)
                            break;

                        if (value is FileRecord fileRecord)
                        {
                            if (fileRecord.Content == null || fileRecord.Content.Length == 0)
                            {
                                if (!string.IsNullOrWhiteSpace(fileRecord.StoragePath))
                                {
                                    try
                                    {
                                        fileRecord.Content = await _fileService.LoadFileAsync(fileRecord.StoragePath);
                                    }
                                    catch (Exception ex)
                                    {
                                        System.Diagnostics.Debug.WriteLine($"Error loading file {fileRecord.StoragePath}: {ex.Message}");
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public async Task<bool> DatabaseExistsAsync(string filePath)
        {
            return await _fileStorage.ExistsAsync(filePath);
        }
    }
}