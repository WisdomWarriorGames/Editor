using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WisdomWarrior.Editor.FileSystem;

namespace WisdomWarrior.Editor.AssetBrowser.ViewModels;

public partial class DirectoryViewModel : ObservableObject
{
    private readonly FileSystemService _fileSystemService;
    private FileSystemRegistry _registry;

    [ObservableProperty] private string _rootDir;

    public ObservableCollection<AssetViewModel> SelectedItems { get; } = [];
    public ObservableCollection<AssetViewModel> Assets { get; } = [];

    public DirectoryViewModel(FileSystemService fileSystemService)
    {
        _fileSystemService = fileSystemService;
        RootDir = "Assets";
    }

    public void Initialize(FileSystemRegistry registry)
    {
        _registry = registry;
        RootDir = registry.RootName;

        _registry.RegistryUpdated += OnRegistryUpdated;
        Refresh();
    }

    private void OnRegistryUpdated()
    {
        Dispatcher.UIThread.InvokeAsync(Refresh);
    }

    private void Refresh()
    {
        var activeNode = _registry.CurrentNode;
        if (activeNode == null) return;

        var sortedChildren = _registry.CurrentNode.Children.OrderByDescending(a => a.IsFolder).ThenBy(a => a.Name);
        Assets.Clear();
        foreach (var child in sortedChildren)
        {
            Assets.Add(new AssetViewModel(child, _fileSystemService));
        }
    }

    [RelayCommand]
    public void ResetChanges()
    {
        foreach (var asset in Assets)
        {
            if (asset.IsValid)
            {
                asset.CommitEdit();
                asset.CancelEdit();
            }
        }

        SelectedItems.Clear();
    }

    [RelayCommand]
    public void DeleteSelected()
    {
        foreach (var selected in SelectedItems)
        {
            if (selected.IsFolder)
            {
                _fileSystemService.DeleteFolder(selected.FullPath);
            }
            else
            {
                _fileSystemService.DeleteFile(selected.FullPath);
            }
        }
    }

    [RelayCommand]
    public void CreateFolder()
    {
        var dir = Path.Combine(_registry.RootDir, "New Folder");
        Assets.Add(new AssetViewModel(dir, "New Folder", _fileSystemService));
    }
}