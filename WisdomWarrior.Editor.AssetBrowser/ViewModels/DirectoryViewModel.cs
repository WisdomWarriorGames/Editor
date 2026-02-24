using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using WisdomWarrior.Editor.FileSystem;

namespace WisdomWarrior.Editor.AssetBrowser.ViewModels;

public partial class DirectoryViewModel : ObservableObject
{
    private FileSystemRegistry _registry;

    [ObservableProperty] private string _rootDir;

    public ObservableCollection<AssetViewModel> Assets { get; } = [];

    public DirectoryViewModel()
    {
        RootDir = "Assets";
    }

    public void Initialize(FileSystemRegistry registry)
    {
        _registry = registry;
        RootDir = registry.RootName;

        _registry.RegistryUpdated += OnRegistryUpdated;
        Refresh();
    }

    private void OnRegistryUpdated()
    {
        Dispatcher.UIThread.InvokeAsync(Refresh);
    }

    private void Refresh()
    {
        var activeNode = _registry.CurrentNode;
        if (activeNode == null) return;

        Assets.Clear();
        foreach (var children in _registry.CurrentNode.Children)
        {
            Assets.Add(new AssetViewModel(children));
        }
    }
}