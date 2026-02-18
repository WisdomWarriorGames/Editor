using CommunityToolkit.Mvvm.ComponentModel;
using WisdomWarrior.Editor.AssetBrowser.ViewModels;
using WisdomWarrior.Editor.Inspector.ViewModels;
using WisdomWarrior.Editor.Menus.ViewModels;
using WisdomWarrior.Editor.MonoGame.ViewModels;
using WisdomWarrior.Editor.SceneList.ViewModels;

namespace WisdomWarrior.Editor.Shell.ViewModels;

public partial class MainWindowViewModel(
    AssetBrowserViewModel assetBrowser,
    MonoGameViewModel monoGameViewModel,
    SceneListViewModel sceneListViewModel,
    FileMenuViewModel fileMenuViewModel,
    InspectorViewModel inspectorViewModel
) : ObservableObject
{
    [ObservableProperty]
    private AssetBrowserViewModel _assetBrowser = assetBrowser;

    [ObservableProperty]
    private MonoGameViewModel _monoGameWindow = monoGameViewModel;

    [ObservableProperty]
    private SceneListViewModel _sceneListViewModel = sceneListViewModel;

    [ObservableProperty]
    private FileMenuViewModel _fileMenuViewModel = fileMenuViewModel;

    [ObservableProperty]
    private InspectorViewModel _inspectorViewModel = inspectorViewModel;
}