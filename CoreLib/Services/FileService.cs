using System;
using System.IO;
using System.Threading.Tasks;
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

        public async Task<FileRecord> CreateFileRecordAsync(string fileName, byte[] content, string? mimeType = null)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentException("File name cannot be null or empty", nameof(fileName));

            if (content == null)
                throw new ArgumentNullException(nameof(content));

            mimeType ??= GetMimeType(Path.GetExtension(fileName));
            
            return await Task.FromResult(new FileRecord(fileName, content, mimeType));
        }

        public async Task<FileRecord> LoadFileRecordAsync(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

            if (!await _fileStorage.ExistsAsync(filePath))
                throw new FileNotFoundException($"File not found: {filePath}");

            var content = await _fileStorage.ReadAllBytesAsync(filePath);
            var fileName = Path.GetFileName(filePath);
            var mimeType = GetMimeType(Path.GetExtension(filePath));

            return new FileRecord(fileName, content, mimeType);
        }

        public async Task<string> SaveFileRecordAsync(FileRecord fileRecord, string? customFileName = null)
        {
            if (fileRecord == null)
                throw new ArgumentNullException(nameof(fileRecord));

            var fileName = customFileName ?? $"{Guid.NewGuid()}_{fileRecord.FileName}";
            var filePath = Path.Combine(_uploadsDirectory, fileName);

            // Ensure uploads directory exists
            await _fileStorage.CreateDirectoryAsync(_uploadsDirectory);
            
            await _fileStorage.WriteAllBytesAsync(filePath, fileRecord.Content);
            return filePath;
        }

        public async Task<bool> DeleteFileRecordAsync(string filePath)
        {
            return await _fileStorage.DeleteAsync(filePath);
        }

        public ValidationResult ValidateFileRecord(FileRecord fileRecord, long maxSizeBytes = 10 * 1024 * 1024) // 10MB default
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
    }
}