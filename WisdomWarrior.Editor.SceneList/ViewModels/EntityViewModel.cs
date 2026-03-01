using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using WisdomWarrior.Editor.Core.ShadowTree;

namespace WisdomWarrior.Editor.SceneList.ViewModels;

public partial class EntityViewModel : ObservableObject
{
    public EntityTracker Tracker { get; }

    [ObservableProperty] private string _name;
    [ObservableProperty] private bool _isExpanded = true;
    [ObservableProperty] private bool _isSelected;
    
    public ObservableCollection<EntityViewModel> Children { get; } = new();

    public EntityViewModel(EntityTracker tracker)
    {
        Tracker = tracker;
        Name = tracker.EngineEntity.Name;
        
        Tracker.OnStructureChanged += OnTrackerStructureChanged;
        
        SyncUIChildren();
    }

    private void OnTrackerStructureChanged()
    {
        Dispatcher.UIThread.Post(SyncUIChildren);
    }

    private void SyncUIChildren()
    {
        var currentTrackers = Tracker.TrackedChildren;
        
        for (int i = 0; i < currentTrackers.Count; i++)
        {
            var childTracker = currentTrackers[i];
            var existingVm = Children.FirstOrDefault(vm => vm.Tracker == childTracker);

            if (existingVm != null)
            {
                int oldIndex = Children.IndexOf(existingVm);
                if (oldIndex != i)
                {
                    Children.Move(oldIndex, i);
                }
            }
            else
            {
                Children.Insert(i, new EntityViewModel(childTracker));
            }
        }
        
        while (Children.Count > currentTrackers.Count)
        {
            Children.RemoveAt(Children.Count - 1);
        }
    }
}