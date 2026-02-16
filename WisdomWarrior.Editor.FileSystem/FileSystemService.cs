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

        Directory.Move(dir, destPath);
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