using CommunityToolkit.Mvvm.ComponentModel;
using WisdomWarrior.Editor.AssetBrowser.ViewModels;
using WisdomWarrior.Editor.MonoGame.ViewModels;

namespace WisdomWarrior.Editor.Shell.ViewModels;

public partial class MainWindowViewModel(AssetBrowserViewModel assetBrowser, MonoGameViewModel monoGameViewModel) : ObservableObject
{
    [ObservableProperty]
    private AssetBrowserViewModel _assetBrowser = assetBrowser;

    [ObservableProperty]
    private MonoGameViewModel _monoGameWindow = monoGameViewModel;
}