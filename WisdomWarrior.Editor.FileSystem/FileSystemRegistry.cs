using WisdomWarrior.Editor.FileSystem.Models;

namespace WisdomWarrior.Editor.FileSystem;

public class FileSystemRegistry(FileSystemService fileSystemService) : IDisposable
{
    private readonly object _lock = new();
    private FileSystemWatcher? _watcher;
    private FileSystemNode _currentNode;

    public string RootDir { get; private set; }
    public string RootName { get; private set; }
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
            StopWatcher();
            Nodes.Clear();
            Directories.Clear();

            RootDir = projectPath;
            RootName = Path.GetFileName(projectPath);
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

    private void StopWatcher()
    {
        if (_watcher != null)
        {
            _watcher.EnableRaisingEvents = false;
            _watcher.Dispose();
        }
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
            child.Parent = node;
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
                    newNode.Parent = parentNode;
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
        lock (_lock)
        {
            if (!Nodes.TryGetValue(oldPath, out var node))
            {
                HandleCreated(newPath);
                return;
            }
            
            UpdatePathsRecursive(node, oldPath, newPath);
            
            node.FullPath = newPath;
            node.Name = Path.GetFileName(newPath);
            
            var oldParentPath = Path.GetDirectoryName(oldPath);
            var newParentPath = Path.GetDirectoryName(newPath);

            if (oldParentPath != newParentPath)
            {
                if (oldParentPath != null && Nodes.TryGetValue(oldParentPath, out var oldParent))
                {
                    oldParent.Children.Remove(node);
                }
                
                if (newParentPath != null && Nodes.TryGetValue(newParentPath, out var newParent))
                {
                    node.Parent = newParent;
                    newParent.Children.Add(node);
                }
            }
        }

        RegistryUpdated?.Invoke();
    }

    private void UpdatePathsRecursive(FileSystemNode node, string oldRoot, string newRoot)
    {
        Nodes.Remove(node.FullPath);
        if (node.IsFolder) Directories.Remove(node.FullPath);
        
        var relative = Path.GetRelativePath(oldRoot, node.FullPath);
        var updatedPath = relative == "." ? newRoot : Path.Combine(newRoot, relative);
        
        node.FullPath = updatedPath;
        
        Nodes[updatedPath] = node;
        if (node.IsFolder) Directories[updatedPath] = node;
        
        foreach (var child in node.Children)
        {
            UpdatePathsRecursive(child, oldRoot, newRoot);
        }
    }

    public void Dispose()
    {
        StopWatcher();
    }
}