using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using WisdomWarrior.Editor.AssetBrowser.Models;
using WisdomWarrior.Editor.AssetBrowser.Services;

namespace WisdomWarrior.Editor.AssetBrowser.ViewModels;

public partial class AssetBrowserViewModel : ObservableObject
{
    private readonly ContentService _contentService;

    [ObservableProperty]
    private string _currentPath;

    public ObservableCollection<AssetItem> Items { get; } = new();

    public AssetBrowserViewModel(string projectPath)
    {
        _contentService = new ContentService(projectPath);

        _currentPath = _contentService.RootPath;
        _contentService.RefreshRequested += OnRefreshRequested;
        Refresh();
    }

    private void OnRefreshRequested()
    {
        Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(Refresh);
    }

    public void Refresh()
    {
        Items.Clear();
        var rawItems = _contentService.GetItems(CurrentPath);

        foreach (var item in rawItems)
        {
            Items.Add(item);
        }
    }
}