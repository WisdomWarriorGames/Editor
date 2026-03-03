using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SukiUI.Toasts;
using WisdomWarrior.Editor.Core;
using WisdomWarrior.Editor.Core.Helpers;
using WisdomWarrior.Editor.Core.Services;
using WisdomWarrior.Editor.FileSystem;
using WisdomWarrior.Editor.FileSystem.Helpers;

namespace WisdomWarrior.Editor.AssetBrowser.ViewModels;

public partial class DirectoryViewModel : ObservableObject
{
    private const int MAX_BREADCRUMBS = 4;

    private readonly FileSystemService _fileSystemService;
    private readonly SelectionManager _selectionManager;
    private FileSystemRegistry _registry;

    [ObservableProperty] private string _rootDir;

    public ObservableCollection<AssetViewModel> SelectedItems { get; } = [];
    public ObservableCollection<AssetViewModel> Assets { get; } = [];
    public ObservableCollection<BreadcrumbViewModel> Breadcrumbs { get; set; } = new();

    public DirectoryViewModel(FileSystemService fileSystemService, SelectionManager selectionManager)
    {
        _fileSystemService = fileSystemService;
        _selectionManager = selectionManager;
        RootDir = "Assets";

        SelectedItems.CollectionChanged += OnCollectionChanged;
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (sender is not IEnumerable<AssetViewModel> assets) return;
        if (assets.Count() > 1) return;

        var asset = assets.FirstOrDefault();

        if (asset == null) return;

        _selectionManager.SetSelection(asset.Node);
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

    private async void Refresh()
    {
        var activeNode = _registry.CurrentNode;
        if (activeNode == null) return;

        Assets.Clear();
        SetupBreadcrumbs();

        await Task.Run(() =>
        {
            var sortedChildren = activeNode.Children
                .OrderByDescending(a => a.IsFolder)
                .ThenBy(a => a.Name)
                .ToList();

            foreach (var child in sortedChildren)
            {
                var vm = new AssetViewModel(child, _fileSystemService);

                Dispatcher.UIThread.Post(() =>
                {
                    if (_registry.CurrentNode == activeNode)
                    {
                        Assets.Add(vm);
                    }
                });
            }
        });
    }

    private void SetupBreadcrumbs()
    {
        var activeNode = _registry.CurrentNode;
        if (activeNode == null) return;

        Breadcrumbs.Clear();

        var breadcrumbs = new List<BreadcrumbViewModel>();
        breadcrumbs.Add(new BreadcrumbViewModel(activeNode.Name, activeNode.FullPath, _fileSystemService, _registry));

        while (activeNode.Parent != null)
        {
            activeNode = activeNode.Parent;
            breadcrumbs.Add(new BreadcrumbViewModel(activeNode.Name, activeNode.FullPath, _fileSystemService, _registry));
        }

        breadcrumbs.Reverse();
        if (!breadcrumbs.Any()) return;

        if (breadcrumbs.Count <= MAX_BREADCRUMBS)
        {
            foreach (var breadcrumb in breadcrumbs) Breadcrumbs.Add(breadcrumb);
            return;
        }

        Breadcrumbs.Add(breadcrumbs[0]);
        var b3 = breadcrumbs[^3];
        Breadcrumbs.Add(new BreadcrumbViewModel("...", b3.FullPath, _fileSystemService, _registry));

        for (int i = breadcrumbs.Count - (MAX_BREADCRUMBS - 1); i < breadcrumbs.Count; i++)
        {
            Breadcrumbs.Add(breadcrumbs[i]);
        }
    }

    public void Navigate(AssetViewModel asset)
    {
        if (!asset.IsFolder) return;

        _registry.SetCurrentNode(asset.FullPath);
    }

    public void RemoveNew(AssetViewModel asset)
    {
        Assets.Remove(asset);
    }

    private bool CanAcceptDrop(object? droppedItem)
    {
        return droppedItem.CanAccept<IStorageItem>();
    }

    [RelayCommand(CanExecute = nameof(CanAcceptDrop))]
    private async Task AcceptDropAsync(object? droppedItem)
    {
        droppedItem.ProcessFileSystemDropAsync(_registry.CurrentNode.FullPath, _fileSystemService);
    }

    [RelayCommand]
    public void ResetChanges()
    {
        foreach (var asset in Assets)
        {
            if (asset.IsValid)
            {
                asset.IsNew = false;
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
        if (_registry == null || _registry.CurrentNode == null) return;

        var baseName = "New Folder";
        var finalName = baseName;
        var count = 1;

        while (_registry.CurrentNode.Children.Any(x => x.Name == finalName))
        {
            finalName = $"{baseName} ({count++})";
        }

        var dir = Path.Combine(_registry.CurrentNode.FullPath, finalName);
        Assets.Add(new AssetViewModel(dir, finalName, _fileSystemService, RemoveNew));
    }

    [RelayCommand]
    public void OpenFolder()
    {
        if (_registry == null || _registry.CurrentNode == null) return;

        var path = _registry.CurrentNode.FullPath;

        if (string.IsNullOrEmpty(path) || !Directory.Exists(path)) return;

        Process.Start(new ProcessStartInfo
        {
            FileName = path,
            UseShellExecute = true,
            Verb = "open"
        });
    }
}