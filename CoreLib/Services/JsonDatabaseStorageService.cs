using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CoreLib.Models;
using CoreLib.Serialization;

namespace CoreLib.Services
{
    public class JsonDatabaseStorageService : IDatabaseStorageService
    {
        private readonly IFileStorageService _fileStorage;
        private readonly FileService _fileService;

        public JsonDatabaseStorageService(IFileStorageService fileStorage, FileService fileService)
        {
            _fileStorage = fileStorage ?? throw new ArgumentNullException(nameof(fileStorage));
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        }

        public async Task<Database> LoadDatabaseAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

            if (!await _fileStorage.ExistsAsync(filePath))
                throw new FileNotFoundException($"Database file not found: {filePath}");

            try
            {
                var jsonBytes = await _fileStorage.ReadAllBytesAsync(filePath);
                var jsonContent = Encoding.UTF8.GetString(jsonBytes);
                
                var fileRecordConverter = new FileRecordConverter(_fileService, isSerializing: false);
                var columnConverter = new ColumnConverter(fileRecordConverter);
                
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    Converters = 
                    { 
                        new JsonStringEnumConverter(),
                        columnConverter,  // ✅ Додаємо ColumnConverter
                        fileRecordConverter
                    }
                };
                
                var database = JsonSerializer.Deserialize<Database>(jsonContent, options);
                
                if (database == null)
                    throw new InvalidOperationException("Failed to deserialize database from file");

                return database;
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Invalid database file format: {ex.Message}", ex);
            }
        }

        public async Task SaveDatabaseAsync(Database database, string filePath)
        {
            if (database == null)
                throw new ArgumentNullException(nameof(database));

            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

            try
            {
                var fileRecordConverter = new FileRecordConverter(_fileService, isSerializing: true);
                var columnConverter = new ColumnConverter(fileRecordConverter);
                
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    Converters = 
                    { 
                        new JsonStringEnumConverter(),
                        columnConverter,  // ✅ Додаємо ColumnConverter
                        fileRecordConverter
                    }
                };
                
                var jsonContent = JsonSerializer.Serialize(database, options);
                var jsonBytes = Encoding.UTF8.GetBytes(jsonContent);
                await _fileStorage.WriteAllBytesAsync(filePath, jsonBytes);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to save database to file: {ex.Message}", ex);
            }
        }
        public async Task<bool> DatabaseExistsAsync(string filePath)
        {
            return await _fileStorage.ExistsAsync(filePath);
        }
    }
}