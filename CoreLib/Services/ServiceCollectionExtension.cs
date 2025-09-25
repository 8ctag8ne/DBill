// using Microsoft.Extensions.DependencyInjection;
// using CoreLib.Validation;

// namespace CoreLib.Services
// {
//     public static class ServiceCollectionExtensions
//     {
//         /// <summary>
//         /// Registers CoreLib services for desktop application (local file storage)
//         /// </summary>
//         public static IServiceCollection AddCoreLibForDesktop(this IServiceCollection services, string uploadsDirectory = "uploads")
//         {
//             // File storage
//             services.AddSingleton<IFileStorageService, LocalFileStorageService>();
            
//             // Database storage
//             services.AddSingleton<IDatabaseStorageService, JsonDatabaseStorageService>();
            
//             // Core services
//             services.AddSingleton<DatabaseService>();
//             services.AddSingleton<TableService>();
//             services.AddSingleton(provider => new FileService(provider.GetRequiredService<IFileStorageService>(), uploadsDirectory));

//             // Validation
//             services.AddSingleton<IDataTypeValidator, DataTypeValidator>();

//             return services;
//         }

//         /// <summary>
//         /// Registers CoreLib services for web API (remote file storage)
//         /// </summary>
//         public static IServiceCollection AddCoreLibForWebApi(this IServiceCollection services, string baseUrl, string uploadsDirectory = "uploads")
//         {
//             // File storage (requires HttpClient to be registered separately)
//             services.AddSingleton<IFileStorageService>(provider => 
//             {
//                 var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
//                 var httpClient = httpClientFactory.CreateClient();
//                 return new RemoteFileStorageService(httpClient, baseUrl);
//             });
            
//             // Database storage
//             services.AddSingleton<IDatabaseStorageService, JsonDatabaseStorageService>();
            
//             // Core services
//             services.AddSingleton<DatabaseService>();
//             services.AddSingleton<TableService>();
//             services.AddSingleton(provider => new FileService(provider.GetRequiredService<IFileStorageService>(), uploadsDirectory));

//             // Validation
//             services.AddSingleton<IDataTypeValidator, DataTypeValidator>();

//             return services;
//         }

//         /// <summary>
//         /// Registers CoreLib services with custom implementations
//         /// </summary>
//         public static IServiceCollection AddCoreLib(this IServiceCollection services, 
//             IFileStorageService fileStorage, 
//             IDatabaseStorageService? databaseStorage = null,
//             string uploadsDirectory = "uploads")
//         {
//             // File storage
//             services.AddSingleton(fileStorage);
            
//             // Database storage
//             services.AddSingleton(databaseStorage ?? new JsonDatabaseStorageService(fileStorage));
            
//             // Core services
//             services.AddSingleton<DatabaseService>();
//             services.AddSingleton<TableService>();
//             services.AddSingleton(provider => new FileService(provider.GetRequiredService<IFileStorageService>(), uploadsDirectory));

//             // Validation
//             services.AddSingleton<IDataTypeValidator, DataTypeValidator>();

//             return services;
//         }
//     }
// }