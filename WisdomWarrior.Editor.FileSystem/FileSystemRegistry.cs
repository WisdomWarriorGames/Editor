using WisdomWarrior.Editor.FileSystem.Models;

namespace WisdomWarrior.Editor.FileSystem;

public class FileSystemRegistry(FileSystemService fileSystemService) : IDisposable
{
    private readonly object _lock = new();
    private FileSystemWatcher? _watcher;
    private FileSystemNode _currentNode;

    public string RootDir { get; private set; }
    public Dictionary<string, FileSystemNode> Nodes { get; } = new();
    public Dictionary<string, FileSystemNode> Directories { get; } = new();
    public event Action? RegistryUpdated;

    public FileSystemNode CurrentNode
    {
        get => _currentNode;
        private set
        {
            _currentNode = value;
            RegistryUpdated?.Invoke();
        }
    }
    
    public void SetCurrentNode(string path)
    {
        if (Nodes.TryGetValue(path, out var node) && node.IsFolder)
        {
            CurrentNode = node;
        }
    }

    public void Initialize(string projectPath)
    {
        lock (_lock)
        {
            Nodes.Clear();
            Directories.Clear();

            RootDir = projectPath;
            var rootNode = fileSystemService.GetFileSystemTree(projectPath);

            if (rootNode != null)
            {
                PopulateDictionaryRecursive(rootNode);
                CurrentNode = rootNode;
            }
        }

        SetupWatcher(projectPath);
    }

    private void SetupWatcher(string path)
    {
        _watcher = new FileSystemWatcher(path)
        {
            IncludeSubdirectories = true,
            EnableRaisingEvents = true,
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.DirectoryName | NotifyFilters.LastWrite,
            InternalBufferSize = 65536
        };

        _watcher.Created += (s, e) => HandleCreated(e.FullPath);
        _watcher.Deleted += (s, e) => HandleDeleted(e.FullPath);
        _watcher.Renamed += (s, e) => HandleRenamed(e.OldFullPath, e.FullPath);
    }

    private void PopulateDictionaryRecursive(FileSystemNode node)
    {
        Nodes[node.FullPath] = node;

        if (node.IsFolder)
        {
            Directories[node.FullPath] = node;
        }

        foreach (var child in node.Children)
        {
            PopulateDictionaryRecursive(child);
        }
    }

    private void HandleCreated(string path)
    {
        if (fileSystemService.ShouldIgnore(path)) return;

        lock (_lock)
        {
            var isDir = Directory.Exists(path);
            var newNode = new FileSystemNode(path, isDir);

            Nodes[path] = newNode;
            if (isDir) Directories[path] = newNode;

            var parentPath = Path.GetDirectoryName(path);
            if (parentPath != null && Nodes.TryGetValue(parentPath, out var parentNode))
            {
                if (parentNode.Children.All(c => c.FullPath != path))
                {
                    parentNode.Children.Add(newNode);
                }
            }
        }

        RegistryUpdated?.Invoke();
    }

    private void HandleDeleted(string path)
    {
        lock (_lock)
        {
            if (!Nodes.TryGetValue(path, out var node)) return;

            var parentPath = Path.GetDirectoryName(path);
            if (parentPath != null && Nodes.TryGetValue(parentPath, out var parentNode))
            {
                parentNode.Children.Remove(node);
            }

            RemoveRecursive(node);
        }

        RegistryUpdated?.Invoke();
    }

    private void RemoveRecursive(FileSystemNode node)
    {
        Nodes.Remove(node.FullPath);
        if (node.IsFolder) Directories.Remove(node.FullPath);

        foreach (var child in node.Children)
        {
            RemoveRecursive(child);
        }
    }

    private void HandleRenamed(string oldPath, string newPath)
    {
        HandleDeleted(oldPath);
        HandleCreated(newPath);
    }

    public void Dispose()
    {
        if (_watcher != null)
        {
            _watcher.EnableRaisingEvents = false;
            _watcher.Dispose();
        }
    }
}