using Microsoft.AspNetCore.Mvc;
using CoreLib.Services;
using CoreLib.Models;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DatabaseController : ControllerBase
    {
        private readonly DatabaseService _databaseService;
        private readonly ILogger<DatabaseController> _logger;

        public DatabaseController(DatabaseService databaseService, ILogger<DatabaseController> logger)
        {
            _databaseService = databaseService;
            _logger = logger;
        }

        /// <summary>
        /// Створити нову базу даних
        /// </summary>
        [HttpPost("create")]
        public IActionResult CreateDatabase([FromBody] CreateDatabaseRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Name))
                    return BadRequest(new { error = "Database name cannot be empty" });

                var database = _databaseService.CreateDatabase(request.Name);
                return Ok(new { 
                    message = "Database created successfully", 
                    name = database.Name 
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating database");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Завантажити базу даних з файлу
        /// </summary>
        [HttpPost("load")]
        [RequestSizeLimit(52428800)] // 50 MB
        public async Task<IActionResult> LoadDatabase(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { error = "Database file is required" });

                // Зберігаємо файл тимчасово
                var tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}_{file.FileName}");
                
                using (var stream = new FileStream(tempPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                try
                {
                    var database = await _databaseService.LoadDatabaseAsync(tempPath);
                    
                    return Ok(new { 
                        message = "Database loaded successfully", 
                        name = database.Name,
                        tableCount = database.Tables.Count
                    });
                }
                catch
                {
                    throw;
                }
            }
            catch (FileNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading database");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Зберегти базу даних у файл
        /// </summary>
        [HttpPost("save")]
        public async Task<IActionResult> SaveDatabase([FromBody] SaveDatabaseRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.FilePath))
                    return BadRequest(new { error = "File path cannot be empty" });

                await _databaseService.SaveDatabaseAsync(request.FilePath);
                
                return Ok(new { message = "Database saved successfully" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving database");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Отримати інформацію про поточну базу даних
        /// </summary>
        [HttpGet("info")]
        public IActionResult GetDatabaseInfo()
        {
            try
            {
                if (_databaseService.CurrentDatabase == null)
                    return NotFound(new { error = "No database is currently loaded" });

                var db = _databaseService.CurrentDatabase;
                return Ok(new
                {
                    name = db.Name,
                    tableCount = db.Tables.Count,
                    tables = db.Tables.Select(t => new
                    {
                        name = t.Name,
                        columnCount = t.Columns.Count,
                        rowCount = t.GetRowCount()
                    }).ToList()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting database info");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Отримати список таблиць
        /// </summary>
        [HttpGet("tables")]
        public IActionResult GetTableNames()
        {
            try
            {
                var tableNames = _databaseService.GetTableNames();
                return Ok(new { tables = tableNames });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting table names");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Отримати статистику бази даних
        /// </summary>
        [HttpGet("statistics")]
        public IActionResult GetStatistics()
        {
            try
            {
                var stats = _databaseService.GetStatistics();
                return Ok(stats);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting statistics");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Валідувати поточну базу даних
        /// </summary>
        [HttpGet("validate")]
        public IActionResult ValidateDatabase()
        {
            try
            {
                var validation = _databaseService.ValidateCurrentDatabase();
                return Ok(new
                {
                    isValid = validation.IsValid,
                    errors = validation.Errors
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating database");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Закрити поточну базу даних
        /// </summary>
        [HttpPost("close")]
        public async Task<IActionResult> CloseDatabase()
        {
            try
            {
                await _databaseService.CloseDatabase();
                return Ok(new { message = "Database closed successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error closing database");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }
    }

    // Request models
    public class CreateDatabaseRequest
    {
        public string Name { get; set; } = string.Empty;
    }

    public class LoadDatabaseRequest
    {
        public string FilePath { get; set; } = string.Empty;
    }

    public class SaveDatabaseRequest
    {
        public string FilePath { get; set; } = string.Empty;
    }
}