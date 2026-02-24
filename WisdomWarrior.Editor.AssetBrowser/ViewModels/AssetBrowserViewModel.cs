using CommunityToolkit.Mvvm.ComponentModel;
using WisdomWarrior.Editor.FileSystem;

namespace WisdomWarrior.Editor.AssetBrowser.ViewModels;

public partial class AssetBrowserViewModel : ObservableObject
{
    [ObservableProperty] private SolutionViewModel _solutionViewModel;
    [ObservableProperty] private DirectoryViewModel _directoryViewModel;

    private readonly WorkspaceService _workspaceService;

    public AssetBrowserViewModel(
        SolutionViewModel solutionViewModel,
        DirectoryViewModel directoryViewModel,
        WorkspaceService workspaceService)
    {
        _solutionViewModel = solutionViewModel;
        _directoryViewModel = directoryViewModel;
        _workspaceService = workspaceService;

        _workspaceService.WorkspaceInitialized += OnWorkspaceInitialized;
    }

    private void OnWorkspaceInitialized(FileSystemRegistry registry)
    {
        _directoryViewModel.Initialize(registry);
    }
}