using System.Collections.ObjectModel;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WisdomWarrior.Editor.Core.Helpers;
using WisdomWarrior.Editor.Core.Models;
using WisdomWarrior.Editor.Core.Services;
using WisdomWarrior.Editor.Core.ShadowTree;
using WisdomWarrior.Editor.SceneList.Helpers;
using WisdomWarrior.Engine.Core;

namespace WisdomWarrior.Editor.SceneList.ViewModels;

public partial class EntityViewModel : ObservableObject
{
    private readonly SceneTracker _sceneTracker;
    private readonly SelectionManager _selectionManager;

    public EntityTracker Tracker { get; }

    [ObservableProperty] private string _name;
    [ObservableProperty] private bool _isExpanded = true;
    [ObservableProperty] private bool _isSelected;

    [ObservableProperty] private string _tempName;
    [ObservableProperty] private bool _isEditing = false;

    public ObservableCollection<EntityViewModel> Children { get; } = new();

    public EntityViewModel(EntityTracker tracker, SceneTracker sceneTracker, SelectionManager selectionManager)
    {
        _sceneTracker = sceneTracker;
        _selectionManager = selectionManager;
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
                Children.Insert(i, new EntityViewModel(childTracker, _sceneTracker, _selectionManager));
            }
        }

        while (Children.Count > currentTrackers.Count)
        {
            Children.RemoveAt(Children.Count - 1);
        }
    }

    partial void OnIsSelectedChanged(bool value)
    {
        if (value)
        {
            _selectionManager.SetSelection(Tracker);
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
        if (droppedItem.CanAccept<EntityViewModel>()) return true;
        if (droppedItem.CanAccept(this)) return true;
        if (!IsDescendantOf(droppedItem)) return true;

        return false;
    }

    [RelayCommand(CanExecute = nameof(CanAcceptDrop))]
    public void AcceptDrop(object? droppedObject)
    {
        droppedObject.ProcessEntityDrop(this, _sceneTracker);

        IsExpanded = true;
    }

    private bool IsDescendantOf(object? droppedItem)
    {
        if (droppedItem == null) return false;
        if (droppedItem is not EntityViewModel draggedVm) return false;

        var current = Tracker.EngineEntity.Parent;
        while (current != null)
        {
            if (current == draggedVm.Tracker.EngineEntity) return true;
            current = current.Parent;
        }

        return false;
    }
}