using CommunityToolkit.Mvvm.ComponentModel;
using WisdomWarrior.Editor.AssetBrowser.ViewModels;
using WisdomWarrior.Editor.Core.Services;
using WisdomWarrior.Editor.FileSystem;
using WisdomWarrior.Editor.Inspector.ViewModels;
using WisdomWarrior.Editor.Menus.ViewModels;
using WisdomWarrior.Editor.MonoGame.ViewModels;
using WisdomWarrior.Editor.SceneList.ViewModels;

namespace WisdomWarrior.Editor.Shell.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    [ObservableProperty]
    private AssetBrowserViewModel _assetBrowserViewModel;

    [ObservableProperty]
    private MonoGameViewModel _monoGameWindow;

    [ObservableProperty]
    private SceneHierarchyViewModel _sceneListViewModel;

    [ObservableProperty]
    private FileMenuViewModel _fileMenuViewModel;

    [ObservableProperty]
    private InspectorViewModel _inspectorViewModel;

    private readonly WorkspaceService _workspaceService;
    private readonly CurrentSceneManager _currentSceneManager;
    private readonly ScenePersistenceService _scenePersistenceService;

    public MainWindowViewModel(
        AssetBrowserViewModel assetBrowserViewModel,
        MonoGameViewModel monoGameViewModel,
        SceneHierarchyViewModel sceneListViewModel,
        FileMenuViewModel fileMenuViewModel,
        InspectorViewModel inspectorViewModel,
        WorkspaceService workspaceService,
        CurrentSceneManager currentSceneManager,
        ScenePersistenceService scenePersistenceService)
    {
        _assetBrowserViewModel = assetBrowserViewModel;
        _monoGameWindow = monoGameViewModel;
        _sceneListViewModel = sceneListViewModel;
        _fileMenuViewModel = fileMenuViewModel;
        _inspectorViewModel = inspectorViewModel;

        _workspaceService = workspaceService;
        _currentSceneManager = currentSceneManager;
        _scenePersistenceService = scenePersistenceService;

        _workspaceService.WorkspaceInitialized += OnWorkspaceInitialized;
    }

    private void OnWorkspaceInitialized(FileSystemRegistry obj)
    {
        var activeScenePath = _workspaceService.ActiveScene;

        if (!string.IsNullOrWhiteSpace(activeScenePath) && System.IO.File.Exists(activeScenePath))
        {
            _currentSceneManager.LoadScene(activeScenePath);
            return;
        }

        _currentSceneManager.CreateInMemoryScene();
    }

    public bool HasUnsavedInMemoryDirtyScene => _scenePersistenceService.HasUnsavedInMemoryDirtyScene;

    public bool HasDirtyPersistedScene => _scenePersistenceService.HasDirtyPersistedScene;

    public bool TrySaveSceneToDirectory(string directoryPath)
    {
        return _scenePersistenceService.TrySaveSceneToDirectory(directoryPath);
    }

    public bool TryAutoSavePersistedScene()
    {
        return _scenePersistenceService.TryAutoSavePersistedScene();
    }
}
