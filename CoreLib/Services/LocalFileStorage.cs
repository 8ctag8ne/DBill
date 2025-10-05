// CoreLib/Services/LocalFileStorage.cs
namespace CoreLib.Services
{
    /// <summary>
    /// Local file system storage implementation
    /// </summary>
    public class LocalFileStorage : IFileStorageService
    {
        public Task<bool> ExistsAsync(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return Task.FromResult(false);

            return Task.FromResult(File.Exists(path) || Directory.Exists(path));
        }

        public async Task<byte[]> ReadAllBytesAsync(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Path cannot be null or empty", nameof(path));

            if (!File.Exists(path))
                throw new FileNotFoundException($"File not found: {path}");

            return await File.ReadAllBytesAsync(path);
        }

        public async Task WriteAllBytesAsync(string path, byte[] content)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Path cannot be null or empty", nameof(path));

            if (content == null)
                throw new ArgumentNullException(nameof(content));

            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            await File.WriteAllBytesAsync(path, content);
        }

        public Task<bool> DeleteAsync(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return Task.FromResult(false);

            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                    return Task.FromResult(true);
                }
                
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, recursive: true);
                    return Task.FromResult(true);
                }

                return Task.FromResult(false);
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        public Task<string[]> ListFilesAsync(string directory)
        {
            if (string.IsNullOrWhiteSpace(directory))
                throw new ArgumentException("Directory path cannot be null or empty", nameof(directory));

            if (!Directory.Exists(directory))
                return Task.FromResult(Array.Empty<string>());

            return Task.FromResult(Directory.GetFiles(directory));
        }

        public Task CreateDirectoryAsync(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Path cannot be null or empty", nameof(path));

            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            return Task.CompletedTask;
        }
    }
}