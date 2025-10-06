using System.Text.Json;
using CoreLib.Models;

namespace CoreLib.Services
{
    public class InMemoryStorageService : IDatabaseStorageService
    {
        private readonly Dictionary<string, string> _storage = new Dictionary<string, string>();

        public Task SaveDatabaseAsync(Database database, string filePath, CancellationToken ct = default)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(database, options);
            _storage[filePath] = json;
            return Task.CompletedTask;
        }

        public Task<Database> LoadDatabaseAsync(string filePath, CancellationToken ct = default)
        {
            if (_storage.TryGetValue(filePath, out var json))
            {
                try
                {
                    var database = JsonSerializer.Deserialize<Database>(json);
                    return Task.FromResult(database ?? throw new InvalidOperationException("Failed to deserialize database"));
                }
                catch (JsonException ex)
                {
                    throw new InvalidOperationException($"Failed to parse database file: {ex.Message}");
                }
            }

            throw new FileNotFoundException($"Database file not found: {filePath}");
        }

        // Додатковий метод для очищення тестових даних
        public void Clear()
        {
            _storage.Clear();
        }

        public Task<bool> DatabaseExistsAsync(string filePath)
        {
            throw new NotImplementedException();
        }
    }
}