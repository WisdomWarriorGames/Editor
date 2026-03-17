using System.Collections.ObjectModel;
using System.Diagnostics;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WisdomWarrior.Editor.AssetBrowser.Services;
using WisdomWarrior.Editor.Core.Helpers;
using WisdomWarrior.Editor.Core.Services;
using WisdomWarrior.Editor.FileSystem;
using WisdomWarrior.Editor.FileSystem.Helpers;

namespace WisdomWarrior.Editor.AssetBrowser.ViewModels;

public partial class DirectoryViewModel : ObservableObject
{
    private const int MaxBreadcrumbs = 4;

    private readonly FileSystemService _fileSystemService;
    private readonly IAssetClipboardActionService _clipboardActionService;
    private readonly SelectionManager _selectionManager;
    private FileSystemRegistry _registry = null!;

    [ObservableProperty] private string _rootDir = "Assets";

    public ObservableCollection<AssetViewModel> SelectedItems { get; } = [];
    public ObservableCollection<AssetViewModel> Assets { get; } = [];
    public ObservableCollection<BreadcrumbViewModel> Breadcrumbs { get; } = [];

    public DirectoryViewModel(
        FileSystemService fileSystemService,
        IAssetClipboardActionService clipboardActionService,
        SelectionManager selectionManager)
    {
        _fileSystemService = fileSystemService;
        _clipboardActionService = clipboardActionService;
        _selectionManager = selectionManager;
    }

    public void Initialize(FileSystemRegistry registry)
    {
        _registry = registry;
        RootDir = registry.RootName;

        _registry.RegistryUpdated += OnRegistryUpdated;
        Refresh();
    }

    public void Navigate(AssetViewModel asset)
    {
        if (!asset.IsFolder)
        {
            return;
        }

        _registry.SetCurrentNode(asset.FullPath);
    }

    public void RemoveNew(AssetViewModel asset)
    {
        Assets.Remove(asset);
    }

    private void OnRegistryUpdated()
    {
        Dispatcher.UIThread.InvokeAsync(Refresh);
    }

    private async void Refresh()
    {
        var activeNode = _registry.CurrentNode;
        if (activeNode == null)
        {
            return;
        }

        Assets.Clear();
        SetupBreadcrumbs();

        await Task.Run(() =>
        {
            var sortedChildren = activeNode.Children
                .OrderByDescending(child => child.IsFolder)
                .ThenBy(child => child.Name)
                .ToList();

            foreach (var child in sortedChildren)
            {
                var vm = new AssetViewModel(child, _fileSystemService, _clipboardActionService, _selectionManager);
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
        if (activeNode == null)
        {
            return;
        }

        Breadcrumbs.Clear();

        var breadcrumbs = new List<BreadcrumbViewModel>
        {
            new(activeNode.Name, activeNode.FullPath, _fileSystemService, _clipboardActionService, _registry)
        };

        while (activeNode.Parent != null)
        {
            activeNode = activeNode.Parent;
            breadcrumbs.Add(new BreadcrumbViewModel(activeNode.Name, activeNode.FullPath, _fileSystemService, _clipboardActionService, _registry));
        }

        breadcrumbs.Reverse();
        if (breadcrumbs.Count <= MaxBreadcrumbs)
        {
            foreach (var breadcrumb in breadcrumbs)
            {
                Breadcrumbs.Add(breadcrumb);
            }

            return;
        }

        Breadcrumbs.Add(breadcrumbs[0]);
        Breadcrumbs.Add(new BreadcrumbViewModel("...", breadcrumbs[^3].FullPath, _fileSystemService, _clipboardActionService, _registry));

        for (var index = breadcrumbs.Count - (MaxBreadcrumbs - 1); index < breadcrumbs.Count; index++)
        {
            Breadcrumbs.Add(breadcrumbs[index]);
        }
    }

    private bool CanAcceptDrop(object? droppedItem)
    {
        return droppedItem.CanAccept<IStorageItem>();
    }

    [RelayCommand(CanExecute = nameof(CanAcceptDrop))]
    private void AcceptDrop(object? droppedItem)
    {
        droppedItem.ProcessFileSystemDropAsync(_registry.CurrentNode.FullPath, _fileSystemService);
    }

    [RelayCommand]
    private async Task CopySelected()
    {
        await _clipboardActionService.CopyPathsAsync(SelectedItems.Select(item => item.FullPath).ToArray());
    }

    [RelayCommand]
    private async Task PasteHere()
    {
        if (_registry?.CurrentNode == null)
        {
            return;
        }

        await _clipboardActionService.PasteIntoAsync(_registry.CurrentNode.FullPath);
    }

    [RelayCommand]
    public void ResetChanges()
    {
        foreach (var asset in Assets)
        {
            if (!asset.IsValid)
            {
                continue;
            }

            asset.IsNew = false;
            asset.CommitEdit();
            asset.CancelEdit();
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
        if (_registry?.CurrentNode == null)
        {
            return;
        }

        const string baseName = "New Folder";
        var finalName = baseName;
        var count = 1;

        while (_registry.CurrentNode.Children.Any(child => child.Name == finalName))
        {
            finalName = $"{baseName} ({count++})";
        }

        var directoryPath = Path.Combine(_registry.CurrentNode.FullPath, finalName);
        Assets.Add(new AssetViewModel(directoryPath, finalName, _fileSystemService, _clipboardActionService, RemoveNew));
    }

    [RelayCommand]
    public void OpenFolder()
    {
        if (_registry?.CurrentNode == null)
        {
            return;
        }

        var path = _registry.CurrentNode.FullPath;
        if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
        {
            return;
        }

        Process.Start(new ProcessStartInfo
        {
            FileName = path,
            UseShellExecute = true,
            Verb = "open"
        });
    }
}
