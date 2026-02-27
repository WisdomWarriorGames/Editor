using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
        return droppedItem is AssetViewModel;
    }

    [RelayCommand(CanExecute = nameof(CanAcceptDrop))]
    private void AcceptDrop(object? droppedItem)
    {
        if (droppedItem is AssetViewModel sourceAsset)
        {
            _fileSystemService.Move(FullPath, sourceAsset.FullPath);
        }
    }
    
    [RelayCommand]
    public void Navigate(string fullPath)
    {
        _registry.SetCurrentNode(fullPath);
    }
}