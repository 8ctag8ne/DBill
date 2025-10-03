using CoreLib.Models;

namespace CoreLib.Services
{
    public class FileService
    {
        private readonly IFileStorageService _fileStorage;
        private readonly string _uploadsDirectory;

        public FileService(IFileStorageService fileStorage, string uploadsDirectory = "uploads")
        {
            _fileStorage = fileStorage ?? throw new ArgumentNullException(nameof(fileStorage));
            _uploadsDirectory = uploadsDirectory;
        }

        // ✅ НОВИЙ: Зберігає файл і повертає StoragePath (ID)
        public async Task<string> SaveFileAsync(byte[] content, string fileName)
        {
            if (content == null || content.Length == 0)
                throw new ArgumentException("Content cannot be empty", nameof(content));

            var fileId = Guid.NewGuid().ToString();
            var newName = $"{fileId}_{fileName}";
            var filePath = Path.Combine(_uploadsDirectory, newName);

            await _fileStorage.CreateDirectoryAsync(_uploadsDirectory);
            await _fileStorage.WriteAllBytesAsync(filePath, content);

            return filePath;
        }

        // ✅ НОВИЙ: Завантажує файл за StoragePath
        public async Task<byte[]> LoadFileAsync(string storagePath)
        {
            if (string.IsNullOrWhiteSpace(storagePath))
                throw new ArgumentException("Storage path cannot be null or empty", nameof(storagePath));

            var filePath = Path.Combine(_uploadsDirectory, storagePath);

            if (!await _fileStorage.ExistsAsync(filePath))
                throw new FileNotFoundException($"File not found: {storagePath}");

            return await _fileStorage.ReadAllBytesAsync(filePath);
        }

        // ✅ НОВИЙ: Видаляє файл за StoragePath
        public async Task<bool> DeleteFileAsync(string storagePath)
        {
            if (string.IsNullOrWhiteSpace(storagePath))
                return false;

            var filePath = Path.Combine(_uploadsDirectory, storagePath);
            return await _fileStorage.DeleteAsync(filePath);
        }

        // ✅ НОВИЙ: Очищає всі файли в директорії
        public async Task CleanupAllFilesAsync()
        {
            if (!await _fileStorage.ExistsAsync(_uploadsDirectory))
                return;

            var files = await _fileStorage.ListFilesAsync(_uploadsDirectory);
            foreach (var file in files)
            {
                await _fileStorage.DeleteAsync(file);
            }
        }

        // Існуючі методи залишаються без змін
        public async Task<FileRecord> CreateFileRecordAsync(string fileName, byte[] content, string? mimeType = null)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("File name cannot be null or empty", nameof(fileName));

            if (content == null)
                throw new ArgumentNullException(nameof(content));

            mimeType ??= GetMimeType(Path.GetExtension(fileName));

            return await Task.FromResult(new FileRecord(fileName, content, mimeType));
        }

        public ValidationResult ValidateFileRecord(FileRecord fileRecord, long maxSizeBytes = 10 * 1024 * 1024)
        {
            var result = new ValidationResult();

            if (fileRecord == null)
            {
                result.AddError("File record cannot be null");
                return result;
            }

            if (string.IsNullOrWhiteSpace(fileRecord.FileName))
                result.AddError("File name is required");

            if (fileRecord.Content == null || fileRecord.Content.Length == 0)
                result.AddError("File content cannot be empty");
            else if (fileRecord.Content.Length > maxSizeBytes)
                result.AddError($"File size ({fileRecord.Size} bytes) exceeds maximum allowed size ({maxSizeBytes} bytes)");

            return result;
        }

        private string GetMimeType(string extension)
        {
            return extension.ToLowerInvariant() switch
            {
                ".txt" => "text/plain",
                ".json" => "application/json",
                ".xml" => "application/xml",
                ".csv" => "text/csv",
                ".html" => "text/html",
                ".htm" => "text/html",
                ".md" => "text/markdown",
                ".log" => "text/plain",
                ".config" => "text/plain",
                _ => "application/octet-stream"
            };
        }

        public Task WriteAllBytesAsync(string path, byte[] content)
        {
            return _fileStorage.WriteAllBytesAsync(path, content);
        }

        public Task<bool> ExistsAsync(string path)
        {
            return _fileStorage.ExistsAsync(path);
        }
        public Task<byte[]> ReadAllBytesAsync(string path)
        {
            return _fileStorage.ReadAllBytesAsync(path);
        }
    }
}