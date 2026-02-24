using CommunityToolkit.Mvvm.ComponentModel;
using WisdomWarrior.Editor.FileSystem.Models;

namespace WisdomWarrior.Editor.AssetBrowser.ViewModels;

public partial class AssetViewModel : ObservableObject
{
    [ObservableProperty] private string _name;
    [ObservableProperty] private bool _isFolder;

    public string Icon => IsFolder ? "📁" : "📄";

    public AssetViewModel(FileSystemNode node)
    {
        Name = node.Name;
        IsFolder = node.IsFolder;
    }
}