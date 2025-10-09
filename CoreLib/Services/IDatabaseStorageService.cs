using CoreLib.Models;

namespace CoreLib.Services
{
    /// <summary>
    /// Interface for database serialization and storage
    /// </summary>
    public interface IDatabaseStorageService
    {
        Task<Database> LoadDatabaseAsync(string filePath, CancellationToken ct = default);
        Task SaveDatabaseAsync(Database database, string filePath, CancellationToken ct = default);
        Task<bool> DatabaseExistsAsync(string filePath);

        Task<byte[]> ExportDatabaseToBytesAsync(Database database, CancellationToken ct = default);
    }
}