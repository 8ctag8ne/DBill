using System;
using System.IO;
using System.Threading.Tasks;

namespace DBill.WpfApp
{
    // Реалізація локального файлового сховища для WPF-додатку
    public class LocalFileStorageService : CoreLib.Services.IFileStorageService
    {
        public async Task<bool> ExistsAsync(string path)
        {
            return await Task.FromResult(File.Exists(path));
        }

        public async Task<byte[]> ReadAllBytesAsync(string path)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException($"File not found: {path}");
            return await File.ReadAllBytesAsync(path);
        }

        public async Task WriteAllBytesAsync(string path, byte[] content)
        {
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            await File.WriteAllBytesAsync(path, content);
        }

        public async Task<bool> DeleteAsync(string path)
        {
            if (File.Exists(path))
            {
                File.Delete(path);
                return await Task.FromResult(true);
            }
            return await Task.FromResult(false);
        }

        public async Task<string[]> ListFilesAsync(string directory)
        {
            if (!Directory.Exists(directory))
                return Array.Empty<string>();
            return await Task.FromResult(Directory.GetFiles(directory));
        }

        public async Task CreateDirectoryAsync(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            await Task.CompletedTask;
        }
    }
}