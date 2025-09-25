using System.Threading.Tasks;

namespace CoreLib.Services
{
    /// <summary>
    /// Base interface for file storage operations (local or remote)
    /// </summary>
    public interface IFileStorageService
    {
        Task<bool> ExistsAsync(string path);
        Task<byte[]> ReadAllBytesAsync(string path);
        Task WriteAllBytesAsync(string path, byte[] content);
        Task<bool> DeleteAsync(string path);
        Task<string[]> ListFilesAsync(string directory);
        Task CreateDirectoryAsync(string path);
    }
}