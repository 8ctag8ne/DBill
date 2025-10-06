using CoreLib.Services;

namespace CoreLib.Tests;
public class MockFileStorage : IFileStorageService
{
    private readonly Dictionary<string, byte[]> _files = new Dictionary<string, byte[]>();

    public Task<bool> ExistsAsync(string path)
    {
        return Task.FromResult(_files.ContainsKey(path));
    }

    public Task<byte[]> ReadAllBytesAsync(string path)
    {
        if (_files.TryGetValue(path, out var content))
        {
            return Task.FromResult(content);
        }
        throw new FileNotFoundException($"File not found: {path}");
    }

    public Task WriteAllBytesAsync(string path, byte[] content)
    {
        _files[path] = content;
        return Task.CompletedTask;
    }

    public Task<bool> DeleteAsync(string path)
    {
        return Task.FromResult(_files.Remove(path));
    }

    public Task<string[]> ListFilesAsync(string directory)
    {
        return Task.FromResult(Array.Empty<string>());
    }

    public Task CreateDirectoryAsync(string path)
    {
        return Task.CompletedTask;
    }
}