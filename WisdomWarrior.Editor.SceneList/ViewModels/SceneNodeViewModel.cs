using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WisdomWarrior.Editor.Core.ShadowTree;
using WisdomWarrior.Engine.Core;

namespace WisdomWarrior.Editor.SceneList.ViewModels;

public partial class SceneNodeViewModel : ObservableObject
{
    private readonly SceneTracker _tracker;

    [ObservableProperty] private string _name = "Loading...";
    [ObservableProperty] private bool _isExpanded = true;
    [ObservableProperty] private bool _isSelected;

    [ObservableProperty] private string _tempName;
    [ObservableProperty] private bool _isEditing = false;

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

    partial void OnNameChanged(string value)
    {
        if (_tracker.ActiveScene != null && _tracker.ActiveScene.Name != value)
        {
            _tracker.ActiveScene.Name = value;
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
    public void AddEntity()
    {
        _tracker.AddEntity(new GameEntity());
    }

    private bool CanAcceptDrop(object? droppedItem)
    {
        if (droppedItem is not EntityViewModel) return false;

        return true;
    }

    [RelayCommand(CanExecute = nameof(CanAcceptDrop))]
    public void AcceptDrop(object? droppedObject)
    {
        if (droppedObject is not EntityViewModel draggedVm) return;

        draggedVm.Tracker.EngineEntity.RemoveFromParent();
        _tracker.AddEntity(draggedVm.Tracker.EngineEntity);

        IsExpanded = true;
    }
}