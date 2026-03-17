using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WisdomWarrior.Editor.AssetBrowser.Services;
using WisdomWarrior.Editor.FileSystem;
using WisdomWarrior.Editor.FileSystem.Models;

namespace WisdomWarrior.Editor.AssetBrowser.ViewModels;

public partial class SolutionViewModel : ObservableObject
{
    private readonly WorkspaceService _workspaceService;
    private readonly IAssetClipboardActionService _clipboardActionService;
    private FileSystemRegistry _registry = null!;

    [ObservableProperty] private FileSystemNode? _selectedNode;

    public IEnumerable<FileSystemNode> FileSystemRoot => _registry?.RootNode != null
        ? [_registry.RootNode]
        : [];

    public SolutionViewModel(WorkspaceService workspaceService, IAssetClipboardActionService clipboardActionService)
    {
        _workspaceService = workspaceService;
        _clipboardActionService = clipboardActionService;

        _workspaceService.WorkspaceInitialized += OnWorkspaceInitialized;
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SelectedNode) && SelectedNode != null)
        {
            _registry.SetCurrentNode(SelectedNode.FullPath);
        }

        base.OnPropertyChanged(e);
    }

    private void OnWorkspaceInitialized(FileSystemRegistry registry)
    {
        _registry = registry;
        _registry.CurrentNodeChanged += OnCurrentNodeChanged;
        _registry.FileSystemChanged += OnFileSystemChanged;

        OnPropertyChanged(nameof(FileSystemRoot));
    }

    private void OnFileSystemChanged()
    {
        OnPropertyChanged(nameof(FileSystemRoot));
    }

    private void OnCurrentNodeChanged(FileSystemNode node)
    {
        SelectedNode = node;
    }

    [RelayCommand]
    private async Task CopyFolder(FileSystemNode? node)
    {
        if (node?.IsFolder != true)
        {
            return;
        }

        await _clipboardActionService.CopyPathsAsync([node.FullPath]);
    }

    [RelayCommand]
    private async Task PasteIntoFolder(FileSystemNode? node)
    {
        if (node?.IsFolder != true)
        {
            return;
        }

        await _clipboardActionService.PasteIntoAsync(node.FullPath);
    }
}
