using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using WisdomWarrior.Editor.FileSystem;
using WisdomWarrior.Editor.FileSystem.Models;

namespace WisdomWarrior.Editor.AssetBrowser.ViewModels;

public partial class SolutionViewModel : ObservableObject
{
    private readonly WorkspaceService _workspaceService;
    private FileSystemRegistry _registry;

    [ObservableProperty] private FileSystemNode? _selectedNode;

    public IEnumerable<FileSystemNode> FileSystemRoot => _registry?.RootNode != null
        ? new[] { _registry.RootNode }
        : Array.Empty<FileSystemNode>();

    public SolutionViewModel(WorkspaceService workspaceService)
    {
        _workspaceService = workspaceService;

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
}