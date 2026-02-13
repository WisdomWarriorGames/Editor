using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace WisdomWarrior.Editor.AssetBrowser.ViewModels;

public partial class AssetBrowserViewModel : ObservableObject
{
    // The list of files the UI will bind to
    public ObservableCollection<AssetItemViewModel> Items { get; } = new();

    [ObservableProperty]
    private string _currentPath;

    public AssetBrowserViewModel()
    {
        // 1. Find the "Content" folder next to the .exe
        var root = Path.Combine(AppContext.BaseDirectory, "Content");
        
        // 2. Ensure it exists
        if (!Directory.Exists(root)) Directory.CreateDirectory(root);

        CurrentPath = root;
        Refresh();
    }

    public void Refresh()
    {
        Items.Clear();

        var dirInfo = new DirectoryInfo(CurrentPath);

        // 1. Add Folders
        foreach (var dir in dirInfo.GetDirectories())
        {
            Items.Add(new AssetItemViewModel 
            { 
                Name = dir.Name, 
                Path = dir.FullName, 
                IsFolder = true 
            });
        }

        // 2. Add Files
        foreach (var file in dirInfo.GetFiles())
        {
            Items.Add(new AssetItemViewModel 
            { 
                Name = file.Name, 
                Path = file.FullName, 
                IsFolder = false 
            });
        }
    }
}