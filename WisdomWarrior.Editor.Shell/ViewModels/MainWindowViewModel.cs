using CommunityToolkit.Mvvm.ComponentModel;
using WisdomWarrior.Editor.AssetBrowser.ViewModels;

namespace WisdomWarrior.Editor.Shell.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    private const string TEST_PROJECT_PATH = @"C:\Personal\GameDev\TestGame\TestGame";

    [ObservableProperty]
    private AssetBrowserViewModel _assetBrowser;

    public MainWindowViewModel()
    {
        _assetBrowser = new AssetBrowserViewModel(TEST_PROJECT_PATH);
    }
}