using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SukiUI.Toasts;
using WisdomWarrior.Editor.Core;
using WisdomWarrior.Editor.Core.Helpers;
using WisdomWarrior.Editor.FileSystem;
using WisdomWarrior.Editor.FileSystem.Helpers;

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
        if (droppedItem.CanAccept<AssetViewModel>()) return true;

        return false;
    }

    [RelayCommand(CanExecute = nameof(CanAcceptDrop))]
    private async Task AcceptDrop(object? droppedItem)
    {
        droppedItem.ProcessFileSystemDropAsync(FullPath, _fileSystemService);
    }

    [RelayCommand]
    public void Navigate(string fullPath)
    {
        _registry.SetCurrentNode(fullPath);
    }
}