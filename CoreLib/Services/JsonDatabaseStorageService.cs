// CoreLib/Services/JsonDatabaseStorageService.cs
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using CoreLib.Models;
using CoreLib.Serialization;

namespace CoreLib.Services
{
    public class JsonDatabaseStorageService : IDatabaseStorageService
    {
        private readonly IFileStorageService _fileStorage;
        private readonly FileService _fileService;

        public JsonDatabaseStorageService(IFileStorageService fileStorage, FileService fileService)
        {
            _fileStorage = fileStorage ?? throw new ArgumentNullException(nameof(fileStorage));
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        }

        public async Task<Database> LoadDatabaseAsync(string filePath, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

            if (!await _fileStorage.ExistsAsync(filePath))
                throw new FileNotFoundException($"Database file not found: {filePath}");

            try
            {
                var jsonBytes = await _fileStorage.ReadAllBytesAsync(filePath);
                
                await using var stream = new MemoryStream(jsonBytes);
                using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
                var root = doc.RootElement;

                var database = new Database
                {
                    Name = root.GetProperty("name").GetString() ?? string.Empty,
                    CreatedAt = root.GetProperty("createdAt").GetDateTime(),
                    ModifiedAt = root.GetProperty("modifiedAt").GetDateTime(),
                    Version = root.TryGetProperty("version", out var vProp) ? vProp.GetString() ?? "1.0" : "1.0"
                };

                if (root.TryGetProperty("tables", out var tablesElement))
                {
                    foreach (var tableElement in tablesElement.EnumerateArray())
                    {
                        var table = new Table
                        {
                            Name = tableElement.GetProperty("name").GetString() ?? string.Empty,
                            CreatedAt = tableElement.GetProperty("createdAt").GetDateTime(),
                            ModifiedAt = tableElement.GetProperty("modifiedAt").GetDateTime()
                        };

                        if (tableElement.TryGetProperty("columns", out var columnsElement))
                        {
                            foreach (var columnElement in columnsElement.EnumerateArray())
                            {
                                var columnName = columnElement.GetProperty("name").GetString() ?? string.Empty;
                                var columnType = Enum.Parse<DataType>(columnElement.GetProperty("type").GetString() ?? "String");
                                
                                var column = new Column(columnName, columnType);

                                if (columnElement.TryGetProperty("values", out var valuesElement))
                                {
                                    foreach (var valueElement in valuesElement.EnumerateArray())
                                    {
                                        object? value = await ParseValue(valueElement, columnType, ct);
                                        column.Values.Add(value);
                                    }
                                }

                                table.Columns.Add(column);
                            }
                        }

                        database.Tables.Add(table);
                    }
                }

                return database;
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"Invalid database file format: {ex.Message}", ex);
            }
        }

        private async Task<object?> ParseValue(JsonElement element, DataType dataType, CancellationToken ct)
        {
            if (element.ValueKind == JsonValueKind.Null)
                return null;

            return dataType switch
            {
                DataType.Integer => element.ValueKind == JsonValueKind.Number 
                    ? element.GetInt32() 
                    : int.TryParse(element.GetString(), out var i) ? i : null,
                
                DataType.Real => element.ValueKind == JsonValueKind.Number 
                    ? element.GetDouble() 
                    : double.TryParse(element.GetString(), out var d) ? d : null,
                
                DataType.Char => element.GetString()?[0],
                
                DataType.String => element.GetString(),
                
                DataType.IntegerInterval => new IntegerInterval(
                    element.GetProperty("min").GetInt32(),
                    element.GetProperty("max").GetInt32()
                ),
                
                DataType.TextFile => await ParseFileRecord(element, ct),
                
                _ => element.GetString()
            };
        }

        private async Task<FileRecord?> ParseFileRecord(JsonElement element, CancellationToken ct)
        {
            var fileName = element.GetProperty("fileName").GetString() ?? string.Empty;
            var mimeType = element.GetProperty("mimeType").GetString() ?? "text/plain";
            var size = element.GetProperty("size").GetInt64();
            var uploadedAt = element.GetProperty("uploadedAt").GetDateTime();

            byte[]? content = null;
            
            if (element.TryGetProperty("content", out var contentProp) && 
                contentProp.ValueKind == JsonValueKind.String)
            {
                var base64 = contentProp.GetString();
                if (!string.IsNullOrWhiteSpace(base64))
                {
                    content = Convert.FromBase64String(base64);
                }
            }

            return new FileRecord
            {
                FileName = fileName,
                Content = content,
                MimeType = mimeType,
                Size = size,
                UploadedAt = uploadedAt
            };
        }

        public async Task SaveDatabaseAsync(Database database, string filePath, CancellationToken ct = default)
        {
            if (database == null) throw new ArgumentNullException(nameof(database));
            if (string.IsNullOrWhiteSpace(filePath)) throw new ArgumentException("File path cannot be null or empty", nameof(filePath));

            string tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".json");

            try
            {
                await using (var fs = new FileStream(tempFile, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
                await using (var writer = new Utf8JsonWriter(fs, new JsonWriterOptions { Indented = true }))
                {
                    writer.WriteStartObject();

                    // Серіалізуємо властивості Database
                    writer.WriteString("name", database.Name);
                    writer.WriteString("createdAt", database.CreatedAt.ToString("O"));
                    writer.WriteString("modifiedAt", database.ModifiedAt.ToString("O"));
                    writer.WriteString("version", database.Version);

                    // Tables
                    writer.WritePropertyName("tables");
                    writer.WriteStartArray();

                    foreach (var table in database.Tables)
                    {
                        if (ct.IsCancellationRequested) break;

                        writer.WriteStartObject();
                        writer.WriteString("name", table.Name);
                        writer.WriteString("createdAt", table.CreatedAt.ToString("O"));
                        writer.WriteString("modifiedAt", table.ModifiedAt.ToString("O"));

                        // Columns
                        writer.WritePropertyName("columns");
                        writer.WriteStartArray();

                        foreach (var column in table.Columns)
                        {
                            writer.WriteStartObject();
                            writer.WriteString("name", column.Name);
                            writer.WriteString("type", column.Type.ToString());

                            writer.WritePropertyName("values");
                            writer.WriteStartArray();

                            foreach (var val in column.Values)
                            {
                                if (ct.IsCancellationRequested) break;

                                if (val == null)
                                {
                                    writer.WriteNullValue();
                                }
                                else if (val is FileRecord fr)
                                {
                                    writer.WriteStartObject();
                                    writer.WriteString("fileName", fr.FileName);
                                    writer.WriteString("mimeType", fr.MimeType);
                                    writer.WriteNumber("size", fr.Size);
                                    writer.WriteString("uploadedAt", fr.UploadedAt.ToString("O"));

                                    byte[]? contentToWrite = null;

                                    if (fr.Content != null && fr.Content.Length > 0)
                                    {
                                        contentToWrite = fr.Content;
                                    }
                                    else if (!string.IsNullOrWhiteSpace(fr.StoragePath) && _fileService != null)
                                    {
                                        try
                                        {
                                            // Асинхронно завантажуємо ЛИШЕ цей файл
                                            await writer.FlushAsync(ct);
                                            contentToWrite = await _fileService.LoadFileAsync(fr.StoragePath);
                                        }
                                        catch (Exception ex)
                                        {
                                            System.Diagnostics.Debug.WriteLine($"Error loading file {fr.StoragePath}: {ex.Message}");
                                            contentToWrite = null;
                                        }
                                    }

                                    if (contentToWrite != null && contentToWrite.Length > 0)
                                    {
                                        writer.WriteString("content", Convert.ToBase64String(contentToWrite));
                                    }
                                    else
                                    {
                                        writer.WriteString("content", "");
                                    }

                                    writer.WriteEndObject();
                                    
                                    // Очищуємо з пам'яті
                                    contentToWrite = null;
                                    await writer.FlushAsync(ct);
                                }
                                else if (val is IntegerInterval interval)
                                {
                                    writer.WriteStartObject();
                                    writer.WriteNumber("min", interval.Min);
                                    writer.WriteNumber("max", interval.Max);
                                    writer.WriteEndObject();
                                }
                                else if (val is int intVal)
                                {
                                    writer.WriteNumberValue(intVal);
                                }
                                else if (val is double doubleVal)
                                {
                                    writer.WriteNumberValue(doubleVal);
                                }
                                else if (val is char charVal)
                                {
                                    writer.WriteStringValue(charVal.ToString());
                                }
                                else
                                {
                                    writer.WriteStringValue(val?.ToString() ?? "");
                                }
                            }

                            writer.WriteEndArray(); // values
                            writer.WriteEndObject(); // column
                            await writer.FlushAsync(ct);
                        }

                        writer.WriteEndArray(); // columns
                        writer.WriteEndObject(); // table
                        await writer.FlushAsync(ct);
                    }

                    writer.WriteEndArray(); // tables
                    writer.WriteEndObject(); // root
                    await writer.FlushAsync(ct);
                }

                // Переміщаємо результат у кінцеве сховище
                var bytes = await File.ReadAllBytesAsync(tempFile, ct);
                await _fileStorage.WriteAllBytesAsync(filePath, bytes);
                
                if (File.Exists(tempFile))
                    File.Delete(tempFile);
            }
            catch (Exception ex)
            {
                try { if (File.Exists(tempFile)) File.Delete(tempFile); } catch { }
                throw new InvalidOperationException($"Failed to save database to file: {ex.Message}", ex);
            }
        }
        public async Task<bool> DatabaseExistsAsync(string filePath)
        {
            return await _fileStorage.ExistsAsync(filePath);
        }
    }
}