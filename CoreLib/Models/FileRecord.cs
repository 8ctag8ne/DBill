using System;
using System.Text;
using System.Text.Json.Serialization;

namespace CoreLib.Models
{
    public class FileRecord
    {
        public string FileName { get; set; } = string.Empty;
        public byte[] Content { get; set; } = Array.Empty<byte>();
        public string MimeType { get; set; } = "text/plain";
        public long Size => Content?.Length ?? 0;
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

        [JsonConstructor]
        public FileRecord() { }

        public FileRecord(string fileName, byte[] content, string mimeType = "text/plain")
        {
            FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
            Content = content ?? throw new ArgumentNullException(nameof(content));
            MimeType = mimeType ?? "text/plain";
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