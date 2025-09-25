using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CoreLib.Models;

namespace CoreLib.Services
{
    /// <summary>
    /// JSON-based database storage service
    /// </summary>
    public class JsonDatabaseStorageService : IDatabaseStorageService
    {
        private readonly IFileStorageService _fileStorage;
        private readonly JsonSerializerOptions _jsonOptions;

        public JsonDatabaseStorageService(IFileStorageService fileStorage)
        {
            _fileStorage = fileStorage ?? throw new ArgumentNullException(nameof(fileStorage));
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                Converters = { new JsonStringEnumConverter() }
            };
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
                var database = JsonSerializer.Deserialize<Database>(jsonContent, _jsonOptions);
                
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
                var jsonContent = JsonSerializer.Serialize(database, _jsonOptions);
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