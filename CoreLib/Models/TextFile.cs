// CoreLib/Models/FileRecord.cs
using System;
using System.Text;
using System.Text.Json.Serialization;

namespace CoreLib.Models
{
    public class FileRecord
    {
        public string FileName { get; set; } = string.Empty;
        
        // Шлях до файлу у FileService
        public string? StoragePath { get; set; }
        
        // Content використовується тільки під час серіалізації/десеріалізації
        // У звичайному режимі роботи він null
        public byte[]? Content { get; set; }
        
        public string MimeType { get; set; } = "text/plain";
        public long Size { get; set; }
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        [JsonConstructor]
        public FileRecord() { }

        public FileRecord(string fileName, byte[] content, string mimeType = "text/plain")
        {
            FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
            Content = content ?? throw new ArgumentNullException(nameof(content));
            MimeType = mimeType ?? "text/plain";
            Size = content.Length;
        }

        // Новий конструктор для роботи зі StoragePath
        public FileRecord(string fileName, string storagePath, long size, string mimeType = "text/plain")
        {
            FileName = fileName;
            StoragePath = storagePath;
            Size = size;
            MimeType = mimeType;
        }

        public string GetContentAsString(Encoding? encoding = null)
        {
            if (Content == null || Content.Length == 0)
                return string.Empty;

            encoding ??= Encoding.UTF8;
            return encoding.GetString(Content);
        }
    }
}