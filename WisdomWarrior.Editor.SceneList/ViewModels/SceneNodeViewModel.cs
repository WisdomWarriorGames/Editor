using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WisdomWarrior.Editor.Core.Helpers;
using WisdomWarrior.Editor.Core.Services;
using WisdomWarrior.Editor.Core.ShadowTree;
using WisdomWarrior.Editor.SceneList.Helpers;
using WisdomWarrior.Engine.Core;

namespace WisdomWarrior.Editor.SceneList.ViewModels;

public partial class SceneNodeViewModel : ObservableObject
{
    private readonly SceneTracker _tracker;
    private readonly SelectionManager _selectionManager;
    private readonly Action _saveScene;

    [ObservableProperty] private string _name = "Loading...";
    [ObservableProperty] private bool _isExpanded = true;
    [ObservableProperty] private bool _isSelected;

    [ObservableProperty] private string _tempName;
    [ObservableProperty] private bool _isEditing = false;

    public ObservableCollection<EntityViewModel> Children { get; } = new();

    public SceneNodeViewModel(SceneTracker tracker, SelectionManager selectionManager, Action saveScene)
    {
        _tracker = tracker;
        _selectionManager = selectionManager;
        _saveScene = saveScene;
        SyncName();
    }

    public void SyncName()
    {
        Name = _tracker.ActiveScene?.Name ?? "Untitled Scene";
    }

    partial void OnIsSelectedChanged(bool value)
    {
        if (value == true)
        {
            _selectionManager.SetSelection(_tracker);
        }
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

    [RelayCommand]
    public void SaveScene()
    {
        _saveScene.Invoke();
    }

    private bool CanAcceptDrop(object? droppedItem)
    {
        if (droppedItem.CanAccept<EntityViewModel>()) return true;

        return false;
    }

    [RelayCommand(CanExecute = nameof(CanAcceptDrop))]
    public void AcceptDrop(object? droppedObject)
    {
        droppedObject.ProcessEntityDrop(this, _tracker);

        IsExpanded = true;
    }
}
