using System.Text.Json;
using System.Text.Json.Serialization;
using CoreLib.Models;

namespace CoreLib.Serialization;
public class FileRecordJsonConverter : JsonConverter<FileRecord>
{
    public override FileRecord Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected StartObject token");

        var fileRecord = new FileRecord();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                return fileRecord;

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();
                reader.Read();

                switch (propertyName?.ToLowerInvariant())
                {
                    case "filename":
                        fileRecord.FileName = reader.GetString() ?? string.Empty;
                        break;
                    case "storagepath":
                        fileRecord.StoragePath = reader.GetString();
                        break;
                    case "content":
                        var base64 = reader.GetString();
                        if (!string.IsNullOrWhiteSpace(base64))
                        {
                            fileRecord.Content = Convert.FromBase64String(base64);
                        }
                        break;
                    case "mimetype":
                        fileRecord.MimeType = reader.GetString() ?? "text/plain";
                        break;
                    case "size":
                        fileRecord.Size = reader.GetInt64();
                        break;
                    case "uploadedat":
                        fileRecord.UploadedAt = reader.GetDateTime();
                        break;
                }
            }
        }

        throw new JsonException("Unexpected end of JSON");
    }

    public override void Write(Utf8JsonWriter writer, FileRecord value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("fileName", value.FileName);

        if (!string.IsNullOrWhiteSpace(value.StoragePath))
            writer.WriteString("storagePath", value.StoragePath);

        if (value.Content != null && value.Content.Length > 0)
            writer.WriteString("content", Convert.ToBase64String(value.Content));
        else
            writer.WriteString("content", "");

        writer.WriteString("mimeType", value.MimeType);
        writer.WriteNumber("size", value.Size);
        writer.WriteString("uploadedAt", value.UploadedAt);
        writer.WriteEndObject();
    }
}