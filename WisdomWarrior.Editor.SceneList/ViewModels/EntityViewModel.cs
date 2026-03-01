using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WisdomWarrior.Editor.Core.Models;
using WisdomWarrior.Editor.Core.ShadowTree;
using WisdomWarrior.Engine.Core;

namespace WisdomWarrior.Editor.SceneList.ViewModels;

public partial class EntityViewModel : ObservableObject
{
    private readonly SceneTracker _sceneTracker;

    public EntityTracker Tracker { get; }

    [ObservableProperty] private string _name;
    [ObservableProperty] private bool _isExpanded = true;
    [ObservableProperty] private bool _isSelected;

    [ObservableProperty] private string _tempName;
    [ObservableProperty] private bool _isEditing = false;

    public ObservableCollection<EntityViewModel> Children { get; } = new();

    public EntityViewModel(EntityTracker tracker, SceneTracker sceneTracker)
    {
        _sceneTracker = sceneTracker;
        Tracker = tracker;
        Name = tracker.EngineEntity.Name;

        if (string.IsNullOrEmpty(Name))
        {
            Name = "GameEntity";
            IsSelected = true;
            BeginEdit();
        }

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
                Children.Insert(i, new EntityViewModel(childTracker, _sceneTracker));
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

    [RelayCommand]
    public void Delete()
    {
        _sceneTracker.RemoveEntity(Tracker.EngineEntity);
    }

    [RelayCommand]
    public void AddEntity()
    {
        Tracker.AddEntity(new GameEntity());
    }

    private bool CanAcceptDrop(object? droppedItem)
    {
        if (droppedItem is not EntityViewModel draggedVm) return false;
        if (draggedVm == this) return false;
        if (IsDescendantOf(draggedVm)) return false;

        return true;
    }

    [RelayCommand(CanExecute = nameof(CanAcceptDrop))]
    public void AcceptDrop(object? droppedObject)
    {
        if (droppedObject is not EntityViewModel draggedVm) return;

        if (draggedVm.Tracker.EngineEntity.Parent == null)
        {
            _sceneTracker.RemoveEntity(draggedVm.Tracker.EngineEntity);
        }

        draggedVm.Tracker.EngineEntity.SetParent(Tracker.EngineEntity);

        IsExpanded = true;
    }

    private bool IsDescendantOf(EntityViewModel potentialParent)
    {
        var current = Tracker.EngineEntity.Parent;
        while (current != null)
        {
            if (current == potentialParent.Tracker.EngineEntity) return true;
            current = current.Parent;
        }

        return false;
    }
}