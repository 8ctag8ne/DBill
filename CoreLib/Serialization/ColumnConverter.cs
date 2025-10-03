// CoreLib/Serialization/ColumnConverter.cs
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using CoreLib.Models;

namespace CoreLib.Serialization
{
    public class ColumnConverter : JsonConverter<Column>
    {
        private readonly JsonConverter<FileRecord>? _fileRecordConverter;

        public ColumnConverter(JsonConverter<FileRecord>? fileRecordConverter = null)
        {
            _fileRecordConverter = fileRecordConverter;
        }

        public override Column Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
                throw new JsonException("Expected start of object");

            var column = new Column();

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
                        case "name":
                            column.Name = reader.GetString() ?? string.Empty;
                            break;

                        case "type":
                            var typeString = reader.GetString();
                            column.Type = Enum.Parse<DataType>(typeString ?? "String", true);
                            break;

                        case "values":
                            column.Values = ReadValues(ref reader, column.Type, options);
                            break;
                    }
                }
            }

            return column;
        }

        private List<object?> ReadValues(ref Utf8JsonReader reader, DataType dataType, JsonSerializerOptions options)
        {
            var values = new List<object?>();

            if (reader.TokenType != JsonTokenType.StartArray)
                throw new JsonException("Expected start of array for values");

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                    break;

                if (reader.TokenType == JsonTokenType.Null)
                {
                    values.Add(null);
                    continue;
                }

                object? value = dataType switch
                {
                    DataType.Integer => ReadInteger(ref reader),
                    DataType.Real => ReadReal(ref reader),
                    DataType.Char => ReadChar(ref reader),
                    DataType.String => reader.GetString(),
                    DataType.TextFile => ReadFileRecord(ref reader, options),
                    DataType.IntegerInterval => ReadIntegerInterval(ref reader, options),
                    _ => reader.GetString()
                };

                values.Add(value);
            }

            return values;
        }

        private int? ReadInteger(ref Utf8JsonReader reader)
        {
            if (reader.TokenType == JsonTokenType.Number)
                return reader.GetInt32();
            
            if (reader.TokenType == JsonTokenType.String)
            {
                var str = reader.GetString();
                return int.TryParse(str, out var result) ? result : null;
            }
            
            return null;
        }

        private double? ReadReal(ref Utf8JsonReader reader)
        {
            if (reader.TokenType == JsonTokenType.Number)
                return reader.GetDouble();
            
            if (reader.TokenType == JsonTokenType.String)
            {
                var str = reader.GetString();
                return double.TryParse(str, out var result) ? result : null;
            }
            
            return null;
        }

        private char? ReadChar(ref Utf8JsonReader reader)
        {
            var str = reader.GetString();
            return !string.IsNullOrEmpty(str) ? str[0] : null;
        }

        private FileRecord? ReadFileRecord(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            if (_fileRecordConverter != null)
            {
                return _fileRecordConverter.Read(ref reader, typeof(FileRecord), options);
            }

            // Fallback - використовуємо стандартну десеріалізацію
            using var doc = JsonDocument.ParseValue(ref reader);
            return JsonSerializer.Deserialize<FileRecord>(doc.RootElement.GetRawText(), options);
        }

        private IntegerInterval? ReadIntegerInterval(ref Utf8JsonReader reader, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null)
                return null;

            using var doc = JsonDocument.ParseValue(ref reader);
            return JsonSerializer.Deserialize<IntegerInterval>(doc.RootElement.GetRawText(), options);
        }

        public override async void Write(Utf8JsonWriter writer, Column value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("name", value.Name);
            writer.WriteString("type", value.Type.ToString());

            writer.WritePropertyName("values");
            writer.WriteStartArray();

            foreach (var val in value.Values)
            {
                if (val == null)
                {
                    writer.WriteNullValue();
                }
                else if (val is FileRecord fileRecord)
                {
                    if (_fileRecordConverter != null)
                    {
                        _fileRecordConverter.Write(writer, fileRecord, options);
                    }
                    else
                    {
                        JsonSerializer.Serialize(writer, fileRecord, options);
                    }
                }
                else if (val is IntegerInterval interval)
                {
                    JsonSerializer.Serialize(writer, interval, options);
                }
                else
                {
                    JsonSerializer.Serialize(writer, val, options);
                }
            }

            writer.WriteEndArray();
            writer.WriteEndObject();
        }
    }
}