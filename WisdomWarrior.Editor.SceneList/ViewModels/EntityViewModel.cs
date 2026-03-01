using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WisdomWarrior.Editor.Core.ShadowTree;

namespace WisdomWarrior.Editor.SceneList.ViewModels;

public partial class EntityViewModel : ObservableObject
{
    public EntityTracker Tracker { get; }

    [ObservableProperty] private string _name;
    [ObservableProperty] private bool _isExpanded = true;
    [ObservableProperty] private bool _isSelected;

    [ObservableProperty] private string _tempName;
    [ObservableProperty] private bool _isEditing = false;

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

    partial void OnNameChanged(string value)
    {
        if (Tracker.EngineEntity.Name != value)
        {
            Tracker.EngineEntity.Name = value;
        }
    }

    [RelayCommand]
    public void BeginEdit()
    {
        IsEditing = true;
        TempName = Name;
    }

    [RelayCommand]
    public void CancelEdit()
    {
        IsEditing = false;
        TempName = string.Empty;
    }

    [RelayCommand]
    public void CommitEdit()
    {
        if (string.IsNullOrEmpty(TempName)) return;
        if (string.Equals(TempName, Name, StringComparison.InvariantCultureIgnoreCase))
        {
            CancelEdit();
            return;
        }

        Name = TempName;

        CancelEdit();
    }
}