using Microsoft.AspNetCore.Mvc;
using CoreLib.Services;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FileController : ControllerBase
    {
        private readonly FileService _fileService;
        private readonly ILogger<FileController> _logger;

        public FileController(FileService fileService, ILogger<FileController> logger)
        {
            _fileService = fileService;
            _logger = logger;
        }

        /// <summary>
        /// Завантажити файл за StoragePath
        /// </summary>
        [HttpGet("download")]
        public async Task<IActionResult> DownloadFile([FromQuery] string storagePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(storagePath))
                    return BadRequest(new { error = "Storage path is required" });

                var content = await _fileService.LoadFileAsync(storagePath);
                
                // Витягуємо оригінальне ім'я файлу з StoragePath
                var fileName = Path.GetFileName(storagePath);
                if (fileName.Contains('_'))
                {
                    // Формат: {GUID}_{OriginalName}
                    var parts = fileName.Split('_', 2);
                    if (parts.Length == 2)
                        fileName = parts[1];
                }

                return File(content, "application/octet-stream", fileName);
            }
            catch (FileNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading file");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Завантажити файл (upload) і отримати StoragePath
        /// </summary>
        [HttpPost("upload")]
        [RequestSizeLimit(52428800)] // 50 MB
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { error = "File is required" });

                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                var content = memoryStream.ToArray();

                var storagePath = await _fileService.SaveFileAsync(content, file.FileName);

                return Ok(new 
                { 
                    message = "File uploaded successfully",
                    storagePath,
                    fileName = file.FileName,
                    size = file.Length
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Видалити файл за StoragePath
        /// </summary>
        [HttpDelete]
        public async Task<IActionResult> DeleteFile([FromQuery] string storagePath)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(storagePath))
                    return BadRequest(new { error = "Storage path is required" });

                var result = await _fileService.DeleteFileAsync(storagePath);

                if (!result)
                    return NotFound(new { error = "File not found" });

                return Ok(new { message = "File deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Очистити всі файли з директорії uploads
        /// </summary>
        [HttpPost("cleanup")]
        public async Task<IActionResult> CleanupFiles()
        {
            try
            {
                await _fileService.CleanupAllFilesAsync();
                return Ok(new { message = "All files cleaned up successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up files");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }
    }
}