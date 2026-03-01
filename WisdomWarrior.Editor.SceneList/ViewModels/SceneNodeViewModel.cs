using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using WisdomWarrior.Editor.Core.ShadowTree;

namespace WisdomWarrior.Editor.SceneList.ViewModels;

public partial class SceneNodeViewModel : ObservableObject
{
    private readonly SceneTracker _tracker;

    [ObservableProperty] private string _name = "Loading...";
    [ObservableProperty] private bool _isExpanded = true;
    [ObservableProperty] private bool _isSelected;

    public ObservableCollection<EntityViewModel> Children { get; } = new();

    public SceneNodeViewModel(SceneTracker tracker)
    {
        _tracker = tracker;
        SyncName();
    }

    public void SyncName()
    {
        Name = _tracker.ActiveScene?.Name ?? "Untitled Scene";
    }
}