using WisdomWarrior.Editor.Core.Models;
using WisdomWarrior.Editor.FileSystem.Models;

namespace WisdomWarrior.Editor.FileSystem;

public class FileSystemService
{
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

    public async Task CopyAsset(string targetDirectory, string sourceAssetPath, IProgress<string>? progress = null)
    {
        var isFile = File.Exists(sourceAssetPath);
        var isDir = Directory.Exists(sourceAssetPath);

        if (!isFile && !isDir) return;

        var assetName = Path.GetFileName(sourceAssetPath);
        progress?.Report($"Copying: {assetName}");

        if (isDir && targetDirectory.Equals(sourceAssetPath, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var destinationPath = Path.Combine(targetDirectory, Path.GetFileName(sourceAssetPath));

        if (File.Exists(sourceAssetPath))
        {
            await CopyFileAsync(sourceAssetPath, destinationPath, progress);
        }
        else if (Directory.Exists(sourceAssetPath))
        {
            await CopyDirectoryRecursive(sourceAssetPath, destinationPath, progress);
        }
    }

    private async Task CopyFileAsync(string source, string destination, IProgress<string>? progress)
    {
        if (File.Exists(destination)) return;

        progress?.Report($"Copying: {Path.GetFileName(source)}");

        await using var sourceStream = new FileStream(source, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, true);
        await using var destinationStream = new FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);

        await sourceStream.CopyToAsync(destinationStream);
    }

    private async Task CopyDirectoryRecursive(string sourceDir, string destDir, IProgress<string>? progress = null)
    {
        Directory.CreateDirectory(destDir);

        foreach (var file in Directory.GetFiles(sourceDir))
        {
            if (ShouldIgnore(file)) continue;
            var destFile = Path.Combine(destDir, Path.GetFileName(file));

            await CopyFileAsync(file, destFile, progress);
            await Task.Delay(5);
        }

        foreach (var dir in Directory.GetDirectories(sourceDir))
        {
            if (ShouldIgnore(dir)) continue;
            var destSubDir = Path.Combine(destDir, Path.GetFileName(dir));
            await CopyDirectoryRecursive(dir, destSubDir, progress);
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

    private FileSystemNode BuildNodeRecursive(string path)
    {
        bool isDirectory = Directory.Exists(path);
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
