// Конвертер для Database (щоб правильно обробляти Column.Values)
using System.Text.Json;
using System.Text.Json.Serialization;
using CoreLib.Models;

namespace CoreLib.Serialization;
public class DatabaseJsonConverter : JsonConverter<Database>
{
    public override Database Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected StartObject token");

        var database = new Database();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                return database;

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();
                reader.Read();

                switch (propertyName?.ToLowerInvariant())
                {
                    case "name":
                        database.Name = reader.GetString() ?? string.Empty;
                        break;
                    case "createdat":
                        database.CreatedAt = reader.GetDateTime();
                        break;
                    case "modifiedat":
                        database.ModifiedAt = reader.GetDateTime();
                        break;
                    case "version":
                        database.Version = reader.GetString() ?? "1.0";
                        break;
                    case "tables":
                        ReadTables(ref reader, database, options);
                        break;
                }
            }
        }

        return database;
    }

    private void ReadTables(ref Utf8JsonReader reader, Database database, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException("Expected StartArray token for tables");

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
                return;

            if (reader.TokenType == JsonTokenType.StartObject)
            {
                var table = ReadTable(ref reader, options);
                database.Tables.Add(table);
            }
        }
    }

    private Table ReadTable(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        var table = new Table();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                return table;

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();
                reader.Read();

                switch (propertyName?.ToLowerInvariant())
                {
                    case "name":
                        table.Name = reader.GetString() ?? string.Empty;
                        break;
                    case "createdat":
                        table.CreatedAt = reader.GetDateTime();
                        break;
                    case "modifiedat":
                        table.ModifiedAt = reader.GetDateTime();
                        break;
                    case "columns":
                        ReadColumns(ref reader, table, options);
                        break;
                }
            }
        }

        return table;
    }

    private void ReadColumns(ref Utf8JsonReader reader, Table table, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException("Expected StartArray token for columns");

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
                return;

            if (reader.TokenType == JsonTokenType.StartObject)
            {
                var column = ReadColumn(ref reader, options);
                table.Columns.Add(column);
            }
        }
    }

    private Column ReadColumn(ref Utf8JsonReader reader, JsonSerializerOptions options)
    {
        string columnName = string.Empty;
        DataType columnType = DataType.String;
        var values = new List<object?>();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                var column = new Column(columnName, columnType);
                foreach (var value in values)
                    column.Values.Add(value);
                return column;
            }

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();
                reader.Read();

                switch (propertyName?.ToLowerInvariant())
                {
                    case "name":
                        columnName = reader.GetString() ?? string.Empty;
                        break;
                    case "type":
                        columnType = (DataType)reader.GetInt32();
                        break;
                    case "values":
                        ReadValues(ref reader, values, columnType, options);
                        break;
                }
            }
        }

        var col = new Column(columnName, columnType);
        foreach (var value in values)
            col.Values.Add(value);
        return col;
    }

    private void ReadValues(ref Utf8JsonReader reader, List<object?> values, DataType dataType, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartArray)
            throw new JsonException("Expected StartArray token for values");

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndArray)
                return;

            var value = ReadValue(ref reader, dataType, options);
            values.Add(value);
        }
    }

    private object? ReadValue(ref Utf8JsonReader reader, DataType dataType, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
            return null;

        return dataType switch
        {
            DataType.Integer => reader.TokenType == JsonTokenType.Number
                ? reader.GetInt32()
                : null,

            DataType.Real => reader.TokenType == JsonTokenType.Number
                ? reader.GetDouble()
                : null,

            DataType.Char => reader.GetString()?[0],

            DataType.String => reader.GetString(),

            DataType.IntegerInterval => JsonSerializer.Deserialize<IntegerInterval>(ref reader, options),

            DataType.TextFile => JsonSerializer.Deserialize<FileRecord>(ref reader, options),

            _ => reader.GetString()
        };
    }

    public override void Write(Utf8JsonWriter writer, Database value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("name", value.Name);
        writer.WriteString("createdAt", value.CreatedAt);
        writer.WriteString("modifiedAt", value.ModifiedAt);
        writer.WriteString("version", value.Version);

        writer.WritePropertyName("tables");
        writer.WriteStartArray();

        foreach (var table in value.Tables)
        {
            WriteTable(writer, table, options);
        }

        writer.WriteEndArray();
        writer.WriteEndObject();
    }

    private void WriteTable(Utf8JsonWriter writer, Table table, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("name", table.Name);
        writer.WriteString("createdAt", table.CreatedAt);
        writer.WriteString("modifiedAt", table.ModifiedAt);

        writer.WritePropertyName("columns");
        writer.WriteStartArray();

        foreach (var column in table.Columns)
        {
            WriteColumn(writer, column, options);
        }

        writer.WriteEndArray();
        writer.WriteEndObject();
    }

    private void WriteColumn(Utf8JsonWriter writer, Column column, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("name", column.Name);
        writer.WriteNumber("type", (int)column.Type);

        writer.WritePropertyName("values");
        writer.WriteStartArray();

        foreach (var value in column.Values)
        {
            WriteValue(writer, value, column.Type, options);
        }

        writer.WriteEndArray();
        writer.WriteEndObject();
    }

    private void WriteValue(Utf8JsonWriter writer, object? value, DataType dataType, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
            return;
        }

        switch (dataType)
        {
            case DataType.Integer:
                writer.WriteNumberValue((int)value);
                break;

            case DataType.Real:
                writer.WriteNumberValue((double)value);
                break;

            case DataType.Char:
                writer.WriteStringValue(value.ToString());
                break;

            case DataType.String:
                writer.WriteStringValue((string)value);
                break;

            case DataType.IntegerInterval:
                JsonSerializer.Serialize(writer, (IntegerInterval)value, options);
                break;

            case DataType.TextFile:
                JsonSerializer.Serialize(writer, (FileRecord)value, options);
                break;

            default:
                writer.WriteStringValue(value.ToString());
                break;
        }
    }
}