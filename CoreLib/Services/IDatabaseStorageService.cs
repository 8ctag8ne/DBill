using System.Threading.Tasks;
using CoreLib.Models;

namespace CoreLib.Services
{
    /// <summary>
    /// Interface for database serialization and storage
    /// </summary>
    public interface IDatabaseStorageService
    {
        Task<Database> LoadDatabaseAsync(string filePath);
        Task SaveDatabaseAsync(Database database, string filePath);
        Task<bool> DatabaseExistsAsync(string filePath);
    }
}