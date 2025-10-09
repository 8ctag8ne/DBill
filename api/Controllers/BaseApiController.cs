using CoreLib.Services;
using Microsoft.AspNetCore.Mvc;

public abstract class BaseApiController : ControllerBase
{
    protected readonly ISessionService SessionService;
    protected DatabaseService _databaseService;
    protected TableService _tableService;

    protected BaseApiController(ISessionService sessionService)
    {
        SessionService = sessionService;
    }

    protected DatabaseService DatabaseService =>
        _databaseService ??= SessionService.GetOrCreateService(() => 
            CreateDatabaseService());

    protected TableService TableService =>
        _tableService ??= SessionService.GetOrCreateService(() =>
            new TableService(DatabaseService));

    private DatabaseService CreateDatabaseService()
    {
        var localStorage = new LocalFileStorage();
        var fileService = new FileService(localStorage, "uploads");
        var databaseStorage = new JsonDatabaseStorageService(localStorage, fileService);
        
        var tempFileService = new FileService(localStorage, "tempFiles");
        var tempDatabaseStorage = new JsonDatabaseStorageService(localStorage, tempFileService);
        
        return new DatabaseService(databaseStorage, tempDatabaseStorage, fileService, tempFileService);
    }
}