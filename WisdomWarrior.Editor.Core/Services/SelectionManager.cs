namespace WisdomWarrior.Editor.Core.Services;

public class SelectionManager
{
    private object? _currentSelection;

    public object? ActiveSelection => _currentSelection;

    public event Action<object?>? OnSelectionChanged;

    public void SetSelection(object? selection)
    {
        if (ReferenceEquals(_currentSelection, selection)) return;

        _currentSelection = selection;
        OnSelectionChanged?.Invoke(_currentSelection);
    }

    public void Clear() => SetSelection(null);
}