using CommunityToolkit.Mvvm.ComponentModel;
using WisdomWarrior.Editor.AssetBrowser.ViewModels;
using WisdomWarrior.Editor.FileSystem;
using WisdomWarrior.Editor.Inspector.ViewModels;
using WisdomWarrior.Editor.Menus.ViewModels;
using WisdomWarrior.Editor.MonoGame.ViewModels;
using WisdomWarrior.Editor.SceneList.ViewModels;
using WisdomWarrior.Engine.Core;

namespace WisdomWarrior.Editor.Shell.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private AssetBrowserViewModel _assetBrowserViewModel;

    [ObservableProperty]
    private MonoGameViewModel _monoGameWindow;

    [ObservableProperty]
    private SceneListViewModel _sceneListViewModel;

    [ObservableProperty]
    private FileMenuViewModel _fileMenuViewModel;

    [ObservableProperty]
    private InspectorViewModel _inspectorViewModel;

    private readonly WorkspaceService _workspaceService;
    private readonly CurrentSceneManager _currentSceneManager;

    public MainWindowViewModel(
        AssetBrowserViewModel assetBrowserViewModel,
        MonoGameViewModel monoGameViewModel,
        SceneListViewModel sceneListViewModel,
        FileMenuViewModel fileMenuViewModel,
        InspectorViewModel inspectorViewModel,
        WorkspaceService workspaceService,
        CurrentSceneManager currentSceneManager)
    {
        _assetBrowserViewModel = assetBrowserViewModel;
        _monoGameWindow = monoGameViewModel;
        _sceneListViewModel = sceneListViewModel;
        _fileMenuViewModel = fileMenuViewModel;
        _inspectorViewModel = inspectorViewModel;

        _workspaceService = workspaceService;
        _currentSceneManager = currentSceneManager;

        _workspaceService.WorkspaceInitialized += OnWorkspaceInitialized;
    }

    private void OnWorkspaceInitialized(FileSystemRegistry obj)
    {
        _currentSceneManager.Initialized(_workspaceService.ActiveScene);
    }
}