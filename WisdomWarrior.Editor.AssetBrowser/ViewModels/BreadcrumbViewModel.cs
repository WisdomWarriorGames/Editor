using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WisdomWarrior.Editor.AssetBrowser.Services;
using WisdomWarrior.Editor.Core.Helpers;
using WisdomWarrior.Editor.FileSystem;
using WisdomWarrior.Editor.FileSystem.Helpers;

namespace WisdomWarrior.Editor.AssetBrowser.ViewModels;

public partial class BreadcrumbViewModel : ObservableObject
{
    private readonly FileSystemService _fileSystemService;
    private readonly IAssetClipboardActionService _clipboardActionService;
    private readonly FileSystemRegistry _registry;

    [ObservableProperty] private string _name = string.Empty;

    public string FullPath { get; }

    public BreadcrumbViewModel(
        string name,
        string fullPath,
        FileSystemService fileSystemService,
        IAssetClipboardActionService clipboardActionService,
        FileSystemRegistry registry)
    {
        _fileSystemService = fileSystemService;
        _clipboardActionService = clipboardActionService;
        _registry = registry;
        Name = name;
        FullPath = fullPath;
    }

    private bool CanAcceptDrop(object? droppedItem)
    {
        return droppedItem.CanAccept<AssetViewModel>();
    }

    [RelayCommand(CanExecute = nameof(CanAcceptDrop))]
    private void AcceptDrop(object? droppedItem)
    {
        droppedItem.ProcessFileSystemDropAsync(FullPath, _fileSystemService);
    }

    [RelayCommand]
    private async Task PasteHere()
    {
        await _clipboardActionService.PasteIntoAsync(FullPath);
    }

    [RelayCommand]
    public void Navigate(string fullPath)
    {
        _registry.SetCurrentNode(fullPath);
    }
}
