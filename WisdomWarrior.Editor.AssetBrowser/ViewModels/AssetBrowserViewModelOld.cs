using System.Collections.ObjectModel;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WisdomWarrior.Editor.FileSystem;

namespace WisdomWarrior.Editor.AssetBrowser.ViewModels;

public partial class AssetBrowserViewModelOld : ObservableObject, IAssetHandler
{
    private FileSystemRegistry? _registry;
    private readonly WorkspaceService _workspace;
    private readonly FileSystemService _fileSystem;

    [ObservableProperty] private bool _isReady = false;

    public ObservableCollection<AssetItem> Items { get; } = new();
    private AssetItem? _lastSelectedItem;

    public AssetBrowserViewModelOld(WorkspaceService workspace, FileSystemService fileSystem)
    {
        _workspace = workspace;
        _fileSystem = fileSystem;

        _workspace.WorkspaceInitialized += Initialize;
    }

    private void Initialize(FileSystemRegistry registry)
    {
        _registry = registry;
        _registry.RegistryUpdated += OnRegistryChanged;
        Refresh();

        IsReady = true;
    }

    private void OnRegistryChanged()
    {
        Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(Refresh);
    }

    public void Refresh()
    {
        var activeNode = _registry.CurrentNode;
        if (activeNode == null) return;

        Items.Clear();
        foreach (var childNode in activeNode.Children)
        {
            Items.Add(new AssetItem(this, childNode));
        }
    }

    public void OnSelected(AssetItem item, KeyModifiers modifiers)
    {
        if (modifiers.HasFlag(KeyModifiers.Control))
        {
            item.IsSelected = !item.IsSelected;
            if (item.IsSelected)
            {
                _lastSelectedItem = item;
            }
        }
        else if (modifiers.HasFlag(KeyModifiers.Shift) && _lastSelectedItem != null)
        {
            var start = Items.IndexOf(_lastSelectedItem);
            var end = Items.IndexOf(item);

            if (start != -1 && end != -1)
            {
                var lower = Math.Min(start, end);
                var upper = Math.Max(start, end);

                for (int i = lower; i <= upper; i++)
                {
                    Items[i].IsSelected = true;
                }
            }
        }
        else
        {
            foreach (var i in Items) i.IsSelected = false;
            item.IsSelected = true;
            _lastSelectedItem = item;
        }
    }

    public void OnDoubleClicked(AssetItem item)
    {
        if (item.IsFolder)
        {
            _registry.SetCurrentNode(item.FullPath);
        }
    }

    public void OnRenameCommitted(AssetItem item, string newName)
    {
        if (item.IsFolder)
        {
            _fileSystem.RenameFolder(item.FullPath, newName);
        }
        else
        {
            _fileSystem.RenameFile(item.FullPath, newName);
        }
    }

    public void OnDeleteRequested(AssetItem item)
    {
        if (item.IsEditing) return;

        if (item.IsFolder)
        {
            _fileSystem.DeleteFolder(item.FullPath);
        }
        else
        {
            _fileSystem.DeleteFile(item.FullPath);
        }
    }

    [RelayCommand]
    public void NavigateUp()
    {
        if (!IsReady)
        {
            return;
        }

        var parentPath = Path.GetDirectoryName(_registry.CurrentNode.FullPath);
        if (parentPath != null)
        {
            _registry.SetCurrentNode(parentPath);
        }
    }

    [RelayCommand]
    public void CreateFolder()
    {
        if (!IsReady)
        {
            return;
        }

        var baseName = "New Folder";
        var finalName = baseName;
        var count = 1;

        while (_registry.CurrentNode.Children.Any(x => x.Name == finalName))
        {
            finalName = $"{baseName} ({count++})";
        }

        _fileSystem.CreateFolder(_registry.CurrentNode.FullPath, finalName);

        // 2. The FileSystemWatcher will trigger the Registry update.
        // To make it feel "snappy," we can force a focus/edit on the new item
        // once it appears in our 'Items' collection.
    }

    [RelayCommand]
    public void DeleteSelected()
    {
        var pathsToDelete = Items.Where(x => x.IsSelected)
            .Select(x => (x.FullPath, x.IsFolder))
            .ToList();

        if (pathsToDelete.Count == 0) return;

        foreach (var (path, isFolder) in pathsToDelete)
        {
            try
            {
                if (isFolder) _fileSystem.DeleteFolder(path);
                else _fileSystem.DeleteFile(path);
            }
            catch (Exception ex)
            {
                // Log the error (maybe show a SukiUI Toast/Notification)
                Console.WriteLine($"Delete failed: {ex.Message}");
            }
        }
    }

    [RelayCommand]
    public void ClearSelection()
    {
        foreach (var item in Items)
        {
            item.IsSelected = false;
            if (item.IsEditing)
            {
                item.CancelRenameCommand.Execute(null);
            }
        }

        _lastSelectedItem = null;
    }
}