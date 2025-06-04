using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace DerpCode.API.Services;

public class FileSystemService : IFileSystemService
{
    public string GetTempPath()
    {
        return Path.GetTempPath();
    }

    public string CombinePaths(params string[] paths)
    {
        return Path.Combine(paths);
    }

    public void CreateDirectory(string path)
    {
        Directory.CreateDirectory(path);
    }

    public bool DirectoryExists(string path)
    {
        return Directory.Exists(path);
    }

    public void DeleteDirectory(string path, bool recursive = false)
    {
        Directory.Delete(path, recursive);
    }

    public bool FileExists(string path)
    {
        return File.Exists(path);
    }

    public async Task WriteAllTextAsync(string path, string content, CancellationToken cancellationToken = default)
    {
        await File.WriteAllTextAsync(path, content, cancellationToken).ConfigureAwait(false);
    }

    public async Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken = default)
    {
        return await File.ReadAllTextAsync(path, cancellationToken).ConfigureAwait(false);
    }
}