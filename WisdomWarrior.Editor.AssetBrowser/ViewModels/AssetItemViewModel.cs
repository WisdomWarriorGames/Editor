using CommunityToolkit.Mvvm.ComponentModel;

namespace WisdomWarrior.Editor.AssetBrowser.ViewModels;

public partial class AssetItemViewModel : ObservableObject
{
    [ObservableProperty] private string _name = "";
    [ObservableProperty] private string _path = "";
    [ObservableProperty] private bool _isFolder;
    
    // Helper to pick an icon (Folder vs File)
    public string Icon => IsFolder ? "📁" : "📄"; 
}