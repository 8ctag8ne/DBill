// using System;
// using System.IO;
// using System.Threading.Tasks;

// namespace CoreLib.Services
// {
//     /// <summary>
//     /// Local file system storage implementation (for desktop client)
//     /// </summary>
//     public class LocalFileStorageService : IFileStorageService
//     {
//         public async Task<bool> ExistsAsync(string path)
//         {
//             return await Task.FromResult(File.Exists(path));
//         }

//         public async Task<byte[]> ReadAllBytesAsync(string path)
//         {
//             if (!File.Exists(path))
//                 throw new FileNotFoundException($"File not found: {path}");

//             return await File.ReadAllBytesAsync(path);
//         }

//         public async Task WriteAllBytesAsync(string path, byte[] content)
//         {
//             var directory = Path.GetDirectoryName(path);
//             if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
//             {
//                 Directory.CreateDirectory(directory);
//             }

//             await File.WriteAllBytesAsync(path, content);
//         }

//         public async Task<bool> DeleteAsync(string path)
//         {
//             try
//             {
//                 if (File.Exists(path))
//                 {
//                     await Task.Run(() => File.Delete(path));
//                     return true;
//                 }
//                 return false;
//             }
//             catch
//             {
//                 return false;
//             }
//         }

//         public async Task<string[]> ListFilesAsync(string directory)
//         {
//             if (!Directory.Exists(directory))
//                 return Array.Empty<string>();

//             return await Task.FromResult(Directory.GetFiles(directory));
//         }

//         public async Task CreateDirectoryAsync(string path)
//         {
//             if (!Directory.Exists(path))
//             {
//                 await Task.Run(() => Directory.CreateDirectory(path));
//             }
//         }
//     }
// }