using Microsoft.AspNetCore.Mvc;
using CoreLib.Services;
using CoreLib.Models;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TableController : ControllerBase
    {
        private readonly TableService _tableService;
        private readonly FileService _fileService;
        private readonly ILogger<TableController> _logger;

        public TableController(TableService tableService, FileService fileService, ILogger<TableController> logger)
        {
            _tableService = tableService;
            _fileService = fileService;
            _logger = logger;
        }

        /// <summary>
        /// Отримати інформацію про таблицю
        /// </summary>
        [HttpGet("{tableName}")]
        public IActionResult GetTable(string tableName)
        {
            try
            {
                var table = _tableService.GetTable(tableName);
                if (table == null)
                    return NotFound(new { error = $"Table '{tableName}' not found" });

                return Ok(new
                {
                    name = table.Name,
                    columns = table.Columns.Select(c => new
                    {
                        name = c.Name,
                        type = c.Type.ToString()
                    }).ToList(),
                    rowCount = table.GetRowCount()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting table");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Створити нову таблицю
        /// </summary>
        [HttpPost("create")]
        public IActionResult CreateTable([FromBody] CreateTableRequest request)
        {
            try
            {
                // Валідація
                var validation = _tableService.ValidateTableCreation(
                    request.TableName, 
                    request.Columns
                );

                if (!validation.IsValid)
                    return BadRequest(new { errors = validation.Errors });

                _tableService.CreateTable(request.TableName, request.Columns);
                
                return Ok(new { message = "Table created successfully" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating table");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Видалити таблицю
        /// </summary>
        [HttpDelete("{tableName}")]
        public IActionResult DeleteTable(string tableName)
        {
            try
            {
                var result = _tableService.DeleteTable(tableName);
                
                if (!result)
                    return NotFound(new { error = $"Table '{tableName}' not found" });

                return Ok(new { message = "Table deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting table");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Отримати всі рядки таблиці
        /// </summary>
        [HttpGet("{tableName}/rows")]
        public IActionResult GetAllRows(string tableName)
        {
            try
            {
                var rows = _tableService.GetAllRows(tableName);
                
                // Серіалізуємо дані з урахуванням спеціальних типів
                var serializedRows = rows.Select(row => 
                    SerializeRow(row)
                ).ToList();

                return Ok(new { rows = serializedRows });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting rows");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Отримати рядок за індексом
        /// </summary>
        [HttpGet("{tableName}/rows/{rowIndex}")]
        public IActionResult GetRow(string tableName, int rowIndex)
        {
            try
            {
                var row = _tableService.GetRow(tableName, rowIndex);
                
                if (row == null || row.Count == 0)
                    return NotFound(new { error = "Row not found" });

                return Ok(SerializeRow(row));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting row");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Додати новий рядок
        /// </summary>
        [HttpPost("{tableName}/rows")]
        [RequestSizeLimit(52428800)] // 50 MB
        public async Task<IActionResult> AddRow(string tableName, [FromForm] AddRowRequest request)
        {
            try
            {
                // Парсимо файли
                var fileRecords = new Dictionary<string, FileRecord?>();
                if (request.Files != null && request.Files.Any())
                {
                    foreach (var file in request.Files)
                    {
                        var columnName = file.Name;
                        using var memoryStream = new MemoryStream();
                        await file.CopyToAsync(memoryStream);
                        var content = memoryStream.ToArray();
                        
                        var fileRecord = await _fileService.CreateFileRecordAsync(
                            file.FileName, 
                            content
                        );
                        
                        fileRecords[columnName] = fileRecord;
                    }
                }

                // Парсимо звичайні дані
                var rawData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object?>>(
                    request.Data ?? "{}"
                );

                // Парсимо та валідуємо
                var (values, validation) = _tableService.ParseAndValidateRowData(
                    tableName, 
                    rawData ?? new Dictionary<string, object?>(), 
                    fileRecords
                );

                if (!validation.IsValid)
                    return BadRequest(new { errors = validation.Errors });

                _tableService.AddRow(tableName, values);
                
                return Ok(new { message = "Row added successfully" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding row");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Оновити рядок
        /// </summary>
        [HttpPut("{tableName}/rows/{rowIndex}")]
        [RequestSizeLimit(52428800)] // 50 MB
        public async Task<IActionResult> UpdateRow(
            string tableName, 
            int rowIndex, 
            [FromForm] UpdateRowRequest request)
        {
            try
            {
                // Парсимо файли
                var fileRecords = new Dictionary<string, FileRecord?>();
                if (request.Files != null && request.Files.Any())
                {
                    foreach (var file in request.Files)
                    {
                        var columnName = file.Name;
                        using var memoryStream = new MemoryStream();
                        await file.CopyToAsync(memoryStream);
                        var content = memoryStream.ToArray();
                        
                        var fileRecord = await _fileService.CreateFileRecordAsync(
                            file.FileName, 
                            content
                        );
                        
                        fileRecords[columnName] = fileRecord;
                    }
                }

                // Парсимо звичайні дані
                var rawData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object?>>(
                    request.Data ?? "{}"
                );

                // Парсимо та валідуємо
                var (values, validation) = _tableService.ParseAndValidateRowData(
                    tableName, 
                    rawData ?? new Dictionary<string, object?>(), 
                    fileRecords
                );

                if (!validation.IsValid)
                    return BadRequest(new { errors = validation.Errors });

                _tableService.UpdateRow(tableName, rowIndex, values);
                
                return Ok(new { message = "Row updated successfully" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating row");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Видалити рядок
        /// </summary>
        [HttpDelete("{tableName}/rows/{rowIndex}")]
        public IActionResult DeleteRow(string tableName, int rowIndex)
        {
            try
            {
                var result = _tableService.DeleteRow(tableName, rowIndex);
                
                if (!result)
                    return NotFound(new { error = "Row not found" });

                return Ok(new { message = "Row deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting row");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Перейменувати колонку
        /// </summary>
        [HttpPut("{tableName}/columns/rename")]
        public IActionResult RenameColumn(string tableName, [FromBody] RenameColumnRequest request)
        {
            try
            {
                var (success, error) = _tableService.TryRenameColumn(
                    tableName, 
                    request.OldName, 
                    request.NewName
                );

                if (!success)
                    return BadRequest(new { error });

                return Ok(new { message = "Column renamed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error renaming column");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Переставити колонки
        /// </summary>
        [HttpPut("{tableName}/columns/reorder")]
        public IActionResult ReorderColumns(string tableName, [FromBody] ReorderColumnsRequest request)
        {
            try
            {
                // Валідація
                var validation = _tableService.ValidateColumnOperation(
                    tableName, 
                    "reorder", 
                    newOrder: request.NewOrder
                );

                if (!validation.IsValid)
                    return BadRequest(new { errors = validation.Errors });

                var result = _tableService.ReorderColumns(tableName, request.NewOrder);
                
                if (!result)
                    return BadRequest(new { error = "Failed to reorder columns" });

                return Ok(new { message = "Columns reordered successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reordering columns");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Отримати список імен колонок
        /// </summary>
        [HttpGet("{tableName}/columns")]
        public IActionResult GetColumnNames(string tableName)
        {
            try
            {
                var table = _tableService.GetTable(tableName);
                if (table == null)
                    return NotFound(new { error = $"Table '{tableName}' not found" });

                var columns = table.Columns.Select(c => new
                {
                    name = c.Name,
                    type = c.Type.ToString()
                }).ToList();

                return Ok(new { columns });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting column names");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        // Helper method для серіалізації рядків
        private Dictionary<string, object?> SerializeRow(Dictionary<string, object?> row)
        {
            var result = new Dictionary<string, object?>();
            
            foreach (var kvp in row)
            {
                if (kvp.Value is FileRecord fileRecord)
                {
                    result[kvp.Key] = new
                    {
                        fileName = fileRecord.FileName,
                        size = fileRecord.Size,
                        mimeType = fileRecord.MimeType,
                        storagePath = fileRecord.StoragePath
                    };
                }
                else if (kvp.Value is IntegerInterval interval)
                {
                    result[kvp.Key] = new
                    {
                        min = interval.Min,
                        max = interval.Max
                    };
                }
                else
                {
                    result[kvp.Key] = kvp.Value;
                }
            }
            
            return result;
        }
    }

    // Request models
    public class CreateTableRequest
    {
        public string TableName { get; set; } = string.Empty;
        public List<Column> Columns { get; set; } = new();
    }

    public class AddRowRequest
    {
        public string? Data { get; set; }
        public IFormFileCollection? Files { get; set; }
    }

    public class UpdateRowRequest
    {
        public string? Data { get; set; }
        public IFormFileCollection? Files { get; set; }
    }

    public class RenameColumnRequest
    {
        public string OldName { get; set; } = string.Empty;
        public string NewName { get; set; } = string.Empty;
    }

    public class ReorderColumnsRequest
    {
        public List<string> NewOrder { get; set; } = new();
    }
}