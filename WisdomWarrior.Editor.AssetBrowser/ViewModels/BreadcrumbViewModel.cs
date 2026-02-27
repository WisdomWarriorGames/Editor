using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SukiUI.Toasts;
using WisdomWarrior.Editor.Core;
using WisdomWarrior.Editor.FileSystem;

namespace WisdomWarrior.Editor.AssetBrowser.ViewModels;

public partial class BreadcrumbViewModel : ObservableObject
{
    private readonly FileSystemService _fileSystemService;
    private readonly FileSystemRegistry _registry;

    [ObservableProperty] private string _name = string.Empty;

    public string FullPath { get; set; }

    public BreadcrumbViewModel(string name, string fullPath, FileSystemService fileSystemService, FileSystemRegistry registry)
    {
        _fileSystemService = fileSystemService;
        _registry = registry;
        Name = name;
        FullPath = fullPath;
    }

    private bool CanAcceptDrop(object? droppedItem)
    {
        if (droppedItem is AssetViewModel)
        {
            return true;
        }

        if (droppedItem is IEnumerable<object> internalList)
        {
            return internalList.All(item => item is AssetViewModel);
        }

        return false;
    }

    [RelayCommand(CanExecute = nameof(CanAcceptDrop))]
    private async Task AcceptDrop(object? droppedItem)
    {
        var loadingToast = EditorUI.ToastManager.CreateToast()
            .WithTitle("Moving Assets")
            .WithContent("Processing files...")
            .WithLoadingState(true)
            .Queue();

        await Task.Run(() =>
        {
            if (droppedItem is AssetViewModel singleAsset)
            {
                _fileSystemService.Move(FullPath, singleAsset.FullPath);
            }
            else if (droppedItem is IEnumerable<object> internalList)
            {
                foreach (var item in internalList)
                {
                    if (item is AssetViewModel assetToMove)
                    {
                        _fileSystemService.Move(FullPath, assetToMove.FullPath);
                    }
                }
            }
        });

        EditorUI.ToastManager.Dismiss(loadingToast);
    }

    [RelayCommand]
    public void Navigate(string fullPath)
    {
        _registry.SetCurrentNode(fullPath);
    }
}