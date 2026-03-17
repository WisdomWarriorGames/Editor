using CommunityToolkit.Mvvm.ComponentModel;

namespace WisdomWarrior.Editor.Inspector.Models;

public class BoolPropertyViewModel : ObservableObject
{
    private readonly Action<bool> _onChanged;
    private bool _value;

    public BoolPropertyViewModel(bool value, Action<bool> onChanged)
    {
        _value = value;
        _onChanged = onChanged;
    }

    public void UpdateFromEngine(bool newValue)
    {
        if (_value == newValue)
        {
            return;
        }

        _value = newValue;
        OnPropertyChanged(nameof(Value));
    }

    public bool Value
    {
        get => _value;
        set
        {
            if (_value == value)
            {
                return;
            }

            _value = value;
            OnPropertyChanged();
            _onChanged.Invoke(_value);
        }
    }
}
