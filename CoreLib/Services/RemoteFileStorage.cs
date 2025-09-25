// using System;
// using System.Net.Http;
// using System.Threading.Tasks;

// namespace CoreLib.Services
// {
//     /// <summary>
//     /// Remote file storage implementation (for web API)
//     /// This is a placeholder - implement according to your remote storage solution
//     /// </summary>
//     public class RemoteFileStorageService : IFileStorageService
//     {
//         private readonly HttpClient _httpClient;
//         private readonly string _baseUrl;

//         public RemoteFileStorageService(HttpClient httpClient, string baseUrl)
//         {
//             _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
//             _baseUrl = baseUrl ?? throw new ArgumentNullException(nameof(baseUrl));
//         }

//         public async Task<bool> ExistsAsync(string path)
//         {
//             try
//             {
//                 var response = await _httpClient.HeadAsync($"{_baseUrl}/files/{path}");
//                 return response.IsSuccessStatusCode;
//             }
//             catch
//             {
//                 return false;
//             }
//         }

//         public async Task<byte[]> ReadAllBytesAsync(string path)
//         {
//             var response = await _httpClient.GetAsync($"{_baseUrl}/files/{path}");
//             response.EnsureSuccessStatusCode();
//             return await response.Content.ReadAsByteArrayAsync();
//         }

//         public async Task WriteAllBytesAsync(string path, byte[] content)
//         {
//             var byteContent = new ByteArrayContent(content);
//             var response = await _httpClient.PutAsync($"{_baseUrl}/files/{path}", byteContent);
//             response.EnsureSuccessStatusCode();
//         }

//         public async Task<bool> DeleteAsync(string path)
//         {
//             try
//             {
//                 var response = await _httpClient.DeleteAsync($"{_baseUrl}/files/{path}");
//                 return response.IsSuccessStatusCode;
//             }
//             catch
//             {
//                 return false;
//             }
//         }

//         public async Task<string[]> ListFilesAsync(string directory)
//         {
//             try
//             {
//                 var response = await _httpClient.GetAsync($"{_baseUrl}/files?directory={directory}");
//                 response.EnsureSuccessStatusCode();
//                 var content = await response.Content.ReadAsStringAsync();
//                 return System.Text.Json.JsonSerializer.Deserialize<string[]>(content) ?? Array.Empty<string>();
//             }
//             catch
//             {
//                 return Array.Empty<string>();
//             }
//         }

//         public async Task CreateDirectoryAsync(string path)
//         {
//             var response = await _httpClient.PostAsync($"{_baseUrl}/directories/{path}", null);
//             response.EnsureSuccessStatusCode();
//         }
//     }
// }