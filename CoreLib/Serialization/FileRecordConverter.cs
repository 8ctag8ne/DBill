// CoreLib/Serialization/FileRecordConverter.cs
using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using CoreLib.Models;
using CoreLib.Services;

namespace CoreLib.Serialization
{
    public class FileRecordConverter : JsonConverter<FileRecord>
    {
        private readonly FileService? _fileService;
        private readonly bool _isSerializing;

        public FileRecordConverter(FileService? fileService = null, bool isSerializing = false)
        {
            _fileService = fileService;
            _isSerializing = isSerializing;
        }

        public override FileRecord Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException("Expected start of object");

            string? fileName = null;
            string? storagePath = null;
            byte[]? content = null;
            string mimeType = "text/plain";
            long size = 0;
            DateTime uploadedAt = DateTime.UtcNow;

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                    break;

                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    var propertyName = reader.GetString();
                    reader.Read();

                    switch (propertyName?.ToLowerInvariant())
                    {
                        case "filename":
                            fileName = reader.GetString();
                            break;
                        case "storagepath":
                            storagePath = reader.GetString();
                            break;
                        case "content":
                            // ✅ Обробляємо і null, і string
                            if (reader.TokenType == JsonTokenType.String)
                            {
                                var base64 = reader.GetString();
                                if (!string.IsNullOrWhiteSpace(base64))
                                {
                                    content = Convert.FromBase64String(base64);
                                }
                            }
                            else if (reader.TokenType == JsonTokenType.Null)
                            {
                                content = null;
                            }
                            break;
                        case "mimetype":
                            mimeType = reader.GetString() ?? "text/plain";
                            break;
                        case "size":
                            size = reader.GetInt64();
                            break;
                        case "uploadedat":
                            uploadedAt = reader.GetDateTime();
                            break;
                    }
                }
            }

            var fileRecord = new FileRecord
            {
                FileName = fileName ?? string.Empty,
                StoragePath = storagePath,
                Content = content,
                MimeType = mimeType,
                Size = size,
                UploadedAt = uploadedAt
            };

            return fileRecord;
        }

        public override void Write(Utf8JsonWriter writer, FileRecord value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("fileName", value.FileName);
            writer.WriteString("mimeType", value.MimeType);
            writer.WriteNumber("size", value.Size);
            writer.WriteString("uploadedAt", value.UploadedAt);

            byte[]? contentToWrite = null;

            // ✅ Спочатку перевіряємо Content в пам'яті
            if (value.Content != null && value.Content.Length > 0)
            {
                contentToWrite = value.Content;
            }
            // ✅ Якщо Content немає, але є StoragePath - завантажуємо з диску
            else if (_isSerializing && !string.IsNullOrWhiteSpace(value.StoragePath) && _fileService != null)
            {
                try
                {
                    contentToWrite = _fileService.LoadFileAsync(value.StoragePath).GetAwaiter().GetResult();
                    System.Diagnostics.Debug.WriteLine($"Loaded file content: {Convert.ToBase64String(contentToWrite)}");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error loading file {value.FileName}: {ex.Message}");
                }
            }

            // Записуємо content
            if (contentToWrite != null && contentToWrite.Length > 0)
            {
                writer.WriteString("content", Convert.ToBase64String(contentToWrite));
            }
            else
            {
                writer.WriteString("content", "");
            }

            writer.WriteEndObject();
        }
    }
}