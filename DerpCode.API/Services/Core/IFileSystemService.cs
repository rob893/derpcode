using System.Threading;
using System.Threading.Tasks;

namespace DerpCode.API.Services.Core;

public interface IFileSystemService
{
    string GetTempPath();

    string CombinePaths(params string[] paths);

    void CreateDirectory(string path);

    bool DirectoryExists(string path);

    void DeleteDirectory(string path, bool recursive = false);

    bool FileExists(string path);

    string GetCurrentDirectory();

    string? GetFileName(string? path);

    string[] GetDirectories(string path);

    Task WriteAllTextAsync(string path, string content, CancellationToken cancellationToken = default);

    Task<string> ReadAllTextAsync(string path, CancellationToken cancellationToken = default);
}