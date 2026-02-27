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
    }

    public void Move(string targetDirectory, string sourceAssetPath)
    {
        var isFile = File.Exists(sourceAssetPath);
        var isDir = Directory.Exists(sourceAssetPath);

        if (!isFile && !isDir) return;

        var assetName = Path.GetFileName(sourceAssetPath);
        var destinationPath = Path.Combine(targetDirectory, assetName);

        if (isDir && targetDirectory.StartsWith(sourceAssetPath, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        if (isFile)
        {
            if (!File.Exists(destinationPath))
            {
                File.Move(sourceAssetPath, destinationPath);
            }
        }
        else if (isDir)
        {
            if (!Directory.Exists(destinationPath))
            {
                Directory.Move(sourceAssetPath, destinationPath);
            }
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