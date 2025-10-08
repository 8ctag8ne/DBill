using CoreLib.Services;
using Microsoft.AspNetCore.Http.Features;
using WebAPI.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS policy
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Налаштування для великих файлів
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 52428800; // 50 MB
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 52428800; // 50 MB
});

// Реєстрація сервісів бібліотеки
// Використовуємо вашу існуючу реалізацію LocalFileStorage з CoreLib
builder.Services.AddSingleton<IFileStorageService, LocalFileStorage>();

// Основний FileService для постійних файлів
builder.Services.AddSingleton<FileService>(sp =>
{
    var localStorage = sp.GetRequiredService<IFileStorageService>();
    return new FileService(localStorage, "uploads");
});

// Тимчасовий FileService для ізольованого завантаження
builder.Services.AddSingleton<FileService>(sp =>
{
    var localStorage = sp.GetRequiredService<IFileStorageService>();
    return new FileService(localStorage, "tempFiles");
});

// DatabaseStorage Services
builder.Services.AddSingleton<IDatabaseStorageService>(sp =>
{
    var localStorage = sp.GetRequiredService<IFileStorageService>();
    var fileService = new FileService(localStorage, "uploads");
    return new JsonDatabaseStorageService(localStorage, fileService);
});

builder.Services.AddSingleton<IDatabaseStorageService>(sp =>
{
    var localStorage = sp.GetRequiredService<IFileStorageService>();
    var tempFileService = new FileService(localStorage, "tempFiles");
    return new JsonDatabaseStorageService(localStorage, tempFileService);
});

// DatabaseService з інжектованими залежностями
builder.Services.AddSingleton<DatabaseService>(sp =>
{
    var localStorage = sp.GetRequiredService<IFileStorageService>();
    
    var fileService = new FileService(localStorage, "uploads");
    var databaseStorage = new JsonDatabaseStorageService(localStorage, fileService);
    
    var tempFileService = new FileService(localStorage, "tempFiles");
    var tempDatabaseStorage = new JsonDatabaseStorageService(localStorage, tempFileService);
    
    return new DatabaseService(
        databaseStorage,
        tempDatabaseStorage,
        fileService,
        tempFileService
    );
});

// TableService
builder.Services.AddSingleton<TableService>(sp =>
{
    var databaseService = sp.GetRequiredService<DatabaseService>();
    return new TableService(databaseService);
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

// Додаємо middleware для обробки помилок
app.UseErrorHandling();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();