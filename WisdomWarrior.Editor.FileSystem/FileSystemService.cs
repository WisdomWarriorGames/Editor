using System.Text.RegularExpressions;
using WisdomWarrior.Editor.Core.Models;
using WisdomWarrior.Editor.FileSystem.Models;

namespace WisdomWarrior.Editor.FileSystem;

public class FileSystemService
{
    private static readonly Regex DuplicateSuffixRegex = new(@" \((\d+)\)$", RegexOptions.Compiled);

    private readonly HashSet<string> _ignoredFolders = new(StringComparer.OrdinalIgnoreCase)
    {
        "bin",
        "obj",
        ".git",
        ".vs",
        ".idea",
        "Properties"
    };

    private readonly HashSet<string> _ignoredExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".csproj",
        ".sln",
        ".user",
        ".suo",
        ".tmp"
    };

    public event Action<FileSystemPathRenamedEvent>? PathRenamed;

    public void CreateFolder(string dir, string name)
    {
        var fullPath = Path.Combine(dir, name);
        if (!Directory.Exists(fullPath))
        {
            Directory.CreateDirectory(fullPath);
        }
    }

    public void DeleteFolder(string dir)
    {
        if (!Directory.Exists(dir)) return;

        Directory.Delete(dir, true);
    }

    public void RenameFolder(string dir, string newName)
    {
        var parent = Path.GetDirectoryName(dir);
        var destPath = Path.Combine(parent!, newName);

        if (!FolderExists(dir))
        {
            CreateFolder(parent, newName);
            return;
        }

        Directory.Move(dir, destPath);
        RaisePathRenamed(dir, destPath, isDirectory: true);
    }

    public bool FolderExists(string dir, string newName)
    {
        var parent = Path.GetDirectoryName(dir);
        var destPath = Path.Combine(parent!, newName);

        return FolderExists(destPath);
    }

    public bool FolderExists(string dir)
    {
        return Directory.Exists(dir);
    }

    public bool FileExists(string path, string newName)
    {
        var parent = Path.GetDirectoryName(path);
        var destPath = Path.Combine(parent!, newName);

        return FileExists(destPath);
    }

    public bool FileExists(string path)
    {
        return File.Exists(path);
    }

    public void DeleteFile(string fileFullPath)
    {
        if (!File.Exists(fileFullPath)) return;

        File.Delete(fileFullPath);
    }

    public void RenameFile(string fileFullPath, string newName)
    {
        var parent = Path.GetDirectoryName(fileFullPath);
        var destPath = Path.Combine(parent!, newName);

        File.Move(fileFullPath, destPath);
        RaisePathRenamed(fileFullPath, destPath, isDirectory: false);
    }

    public void Move(string targetDirectory, string sourceAssetPath, IProgress<string>? progress = null)
    {
        var isFile = File.Exists(sourceAssetPath);
        var isDir = Directory.Exists(sourceAssetPath);

        if (!isFile && !isDir) return;

        var assetName = Path.GetFileName(sourceAssetPath);
        progress?.Report($"Copying: {assetName}");

        var destinationPath = Path.Combine(targetDirectory, assetName);

        if (isDir && targetDirectory.Equals(sourceAssetPath, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (isFile)
        {
            if (!File.Exists(destinationPath))
            {
                File.Move(sourceAssetPath, destinationPath);
                RaisePathRenamed(sourceAssetPath, destinationPath, isDirectory: false);
            }
        }
        else if (isDir)
        {
            if (!Directory.Exists(destinationPath))
            {
                Directory.Move(sourceAssetPath, destinationPath);
                RaisePathRenamed(sourceAssetPath, destinationPath, isDirectory: true);
            }
        }
    }

    public async Task CopyAsset(string targetDirectory, string sourceAssetPath, IProgress<string>? progress = null)
    {
        await CopySingleAssetAsync(targetDirectory, sourceAssetPath, CopyAssetMode.ImportExternal, allowDuplicateNaming: false, progress);
    }

    public async Task CopyAssetsAsync(IEnumerable<string> sourcePaths, string targetDirectory, CopyAssetMode mode, IProgress<string>? progress = null)
    {
        if (string.IsNullOrWhiteSpace(targetDirectory) || !Directory.Exists(targetDirectory))
        {
            return;
        }

        foreach (var sourcePath in sourcePaths)
        {
            await CopySingleAssetAsync(targetDirectory, sourcePath, mode, allowDuplicateNaming: true, progress);
        }
    }

    public FileSystemNode? GetFileSystemTree(string path)
    {
        if (ShouldIgnore(path)) return null;
        return BuildNodeRecursive(path);
    }

    public bool ShouldIgnore(string path)
    {
        var name = Path.GetFileName(path);
        var ext = Path.GetExtension(path);

        if (Directory.Exists(path) && _ignoredFolders.Contains(name)) return true;
        if (!Directory.Exists(path) && _ignoredExtensions.Contains(ext)) return true;

        return false;
    }

    private async Task CopySingleAssetAsync(
        string targetDirectory,
        string sourceAssetPath,
        CopyAssetMode mode,
        bool allowDuplicateNaming,
        IProgress<string>? progress)
    {
        var isFile = File.Exists(sourceAssetPath);
        var isDir = Directory.Exists(sourceAssetPath);

        if (!isFile && !isDir) return;
        if (!Directory.Exists(targetDirectory)) return;

        if (mode == CopyAssetMode.ImportExternal && ShouldIgnore(sourceAssetPath))
        {
            return;
        }

        if (isDir && IsInvalidCopyDestination(sourceAssetPath, targetDirectory))
        {
            return;
        }

        progress?.Report($"Copying: {Path.GetFileName(sourceAssetPath)}");

        var destinationPath = allowDuplicateNaming
            ? GetUniqueDestinationPath(targetDirectory, sourceAssetPath)
            : Path.Combine(targetDirectory, Path.GetFileName(sourceAssetPath));

        if (isFile)
        {
            await CopyFileAsync(sourceAssetPath, destinationPath, progress);
            return;
        }

        await CopyDirectoryRecursive(
            sourceAssetPath,
            destinationPath,
            applyIgnoreFilter: mode == CopyAssetMode.ImportExternal,
            progress);
    }

    private async Task CopyFileAsync(string source, string destination, IProgress<string>? progress)
    {
        if (File.Exists(destination)) return;

        progress?.Report($"Copying: {Path.GetFileName(source)}");

        await using var sourceStream = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
        await using var destinationStream = new FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);

        await sourceStream.CopyToAsync(destinationStream);
    }

    private async Task CopyDirectoryRecursive(
        string sourceDir,
        string destDir,
        bool applyIgnoreFilter,
        IProgress<string>? progress = null)
    {
        Directory.CreateDirectory(destDir);

        foreach (var file in Directory.GetFiles(sourceDir))
        {
            if (applyIgnoreFilter && ShouldIgnore(file)) continue;

            var destFile = Path.Combine(destDir, Path.GetFileName(file));
            await CopyFileAsync(file, destFile, progress);
            await Task.Delay(5);
        }

        foreach (var dir in Directory.GetDirectories(sourceDir))
        {
            if (applyIgnoreFilter && ShouldIgnore(dir)) continue;

            var destSubDir = Path.Combine(destDir, Path.GetFileName(dir));
            await CopyDirectoryRecursive(dir, destSubDir, applyIgnoreFilter, progress);
        }
    }

    private string GetUniqueDestinationPath(string targetDirectory, string sourceAssetPath)
    {
        var originalName = Path.GetFileName(sourceAssetPath);
        var desiredPath = Path.Combine(targetDirectory, originalName);

        if (!File.Exists(desiredPath) && !Directory.Exists(desiredPath))
        {
            return desiredPath;
        }

        var isDirectory = Directory.Exists(sourceAssetPath);
        var originalBaseName = isDirectory
            ? Path.GetFileName(sourceAssetPath)
            : Path.GetFileNameWithoutExtension(sourceAssetPath);
        var extension = isDirectory ? string.Empty : Path.GetExtension(sourceAssetPath);
        var normalizedBaseName = StripDuplicateSuffix(originalBaseName);

        var nextDuplicateNumber = Directory.EnumerateFileSystemEntries(targetDirectory)
            .Select(entry => GetDuplicateNumber(entry, normalizedBaseName, extension, isDirectory))
            .Where(number => number.HasValue)
            .Select(number => number!.Value)
            .DefaultIfEmpty(0)
            .Max() + 1;

        var duplicateName = isDirectory
            ? $"{normalizedBaseName} ({nextDuplicateNumber})"
            : $"{normalizedBaseName} ({nextDuplicateNumber}){extension}";

        return Path.Combine(targetDirectory, duplicateName);
    }

    private static int? GetDuplicateNumber(string entryPath, string normalizedBaseName, string extension, bool isDirectory)
    {
        var entryIsDirectory = Directory.Exists(entryPath);
        if (entryIsDirectory != isDirectory)
        {
            return null;
        }

        if (!isDirectory && !string.Equals(Path.GetExtension(entryPath), extension, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        var candidateName = isDirectory
            ? Path.GetFileName(entryPath)
            : Path.GetFileNameWithoutExtension(entryPath);

        if (string.Equals(candidateName, normalizedBaseName, StringComparison.OrdinalIgnoreCase))
        {
            return 0;
        }

        var match = DuplicateSuffixRegex.Match(candidateName);
        if (!match.Success)
        {
            return null;
        }

        var candidateBaseName = candidateName[..match.Index];
        if (!string.Equals(candidateBaseName, normalizedBaseName, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return int.TryParse(match.Groups[1].Value, out var duplicateNumber)
            ? duplicateNumber
            : null;
    }

    private static string StripDuplicateSuffix(string value)
    {
        var match = DuplicateSuffixRegex.Match(value);
        return match.Success ? value[..match.Index] : value;
    }

    private static bool IsInvalidCopyDestination(string sourceDirectory, string targetDirectory)
    {
        var fullSourcePath = Path.GetFullPath(sourceDirectory);
        var fullTargetPath = Path.GetFullPath(targetDirectory);

        if (string.Equals(fullSourcePath, fullTargetPath, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        var relativePath = Path.GetRelativePath(fullSourcePath, fullTargetPath);
        return !relativePath.StartsWith("..", StringComparison.Ordinal)
               && !Path.IsPathRooted(relativePath);
    }

    private void RaisePathRenamed(string oldPath, string newPath, bool isDirectory)
    {
        if (string.Equals(oldPath, newPath, StringComparison.OrdinalIgnoreCase))
            return;

        PathRenamed?.Invoke(new FileSystemPathRenamedEvent
        {
            OldPath = oldPath,
            NewPath = newPath,
            IsDirectory = isDirectory
        });
    }

    private FileSystemNode BuildNodeRecursive(string path)
    {
        var isDirectory = Directory.Exists(path);
        var node = new FileSystemNode(path, isDirectory);

        if (isDirectory)
        {
            var entries = Directory.EnumerateFileSystemEntries(path).Where(entry => !ShouldIgnore(entry));

            foreach (var entry in entries)
            {
                node.Children.Add(BuildNodeRecursive(entry));
            }
        }

        return node;
    }
}
