using System.Collections.ObjectModel;
using WisdomWarrior.Editor.AssetBrowser.Models;

namespace WisdomWarrior.Editor.AssetBrowser.Services;

public class ContentService
{
    private readonly string _rootPath;
    private FileSystemWatcher? _watcher;

    public event Action? RefreshRequested;

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

    public ContentService(string projectPath)
    {
        _rootPath = projectPath;

        if (!Directory.Exists(_rootPath))
        {
            Directory.CreateDirectory(_rootPath);
        }

        SetupWatcher();
    }

    public string RootPath => _rootPath;

    public void CreateFolder(string parentPath, string folderName)
    {
        var fullPath = Path.Combine(parentPath, folderName);
        if (!Directory.Exists(fullPath))
        {
            Directory.CreateDirectory(fullPath);
        }
    }

    public void RenameItem(string sourcePath, string newName, bool isFolder)
    {
        var parent = Path.GetDirectoryName(sourcePath);
        var destPath = Path.Combine(parent!, newName);

        if (isFolder)
        {
            Directory.Move(sourcePath, destPath);
        }
        else
        {
            File.Move(sourcePath, destPath);
        }
    }

    public IEnumerable<AssetItem> GetItems(string path)
    {
        if (!Directory.Exists(path)) return Enumerable.Empty<AssetItem>();

        var items = new List<AssetItem>();

        try
        {
            foreach (var dir in Directory.GetDirectories(path))
            {
                var info = new DirectoryInfo(dir);

                if (_ignoredFolders.Contains(info.Name)) continue;
                if (info.Attributes.HasFlag(FileAttributes.Hidden)) continue;

                items.Add(new AssetItem
                {
                    Name = info.Name,
                    FullPath = info.FullName,
                    IsFolder = true
                });
            }

            foreach (var file in Directory.GetFiles(path))
            {
                var info = new FileInfo(file);

                if (_ignoredExtensions.Contains(info.Extension)) continue;
                if (info.Attributes.HasFlag(FileAttributes.Hidden)) continue;

                items.Add(new AssetItem
                {
                    Name = info.Name,
                    FullPath = info.FullName,
                    IsFolder = false,
                    Extension = info.Extension
                });
            }
        }
        catch (UnauthorizedAccessException)
        {
        }

        return items;
    }

    private void SetupWatcher()
    {
        _watcher = new FileSystemWatcher(_rootPath)
        {
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName,
            IncludeSubdirectories = true,
            EnableRaisingEvents = true
        };

        _watcher.Created += OnFileChanged;
        _watcher.Deleted += OnFileChanged;
        _watcher.Renamed += OnFileChanged;
    }

    private void OnFileChanged(object sender, FileSystemEventArgs e)
    {
        if (IsIgnored(e.FullPath)) return;

        RefreshRequested?.Invoke();
    }

    private bool IsIgnored(string path)
    {
        if (_ignoredExtensions.Contains(Path.GetExtension(path))) return true;

        var segments = path.Split(Path.DirectorySeparatorChar);

        foreach (var segment in segments)
        {
            if (_ignoredFolders.Contains(segment)) return true;
        }

        return false;
    }
}