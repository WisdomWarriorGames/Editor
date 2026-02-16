using CommunityToolkit.Mvvm.ComponentModel;
using WisdomWarrior.Editor.AssetBrowser.ViewModels;

namespace WisdomWarrior.Editor.Shell.ViewModels;

public partial class MainWindowViewModel(AssetBrowserViewModel assetBrowser) : ObservableObject
{
    [ObservableProperty]
    private AssetBrowserViewModel _assetBrowser = assetBrowser;
}