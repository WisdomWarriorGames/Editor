using System.Drawing;
using CommunityToolkit.Mvvm.ComponentModel;

namespace WisdomWarrior.Editor.Inspector.Models;

public class ColourViewModel : ObservableObject
{
    private readonly Action<Color> _onChanged;
    private Color _value;

    public ColourViewModel(Color value, Action<Color> onChanged)
    {
        _value = value;
        _onChanged = onChanged;
    }

    public void UpdateFromEngine(Color newValue)
    {
        if (_value == newValue) return;

        _value = newValue;

        OnPropertyChanged(nameof(Value));
    }

    public Color Value
    {
        get => _value;
        set
        {
            if (_value == value) return;
            _value = value;
            OnPropertyChanged();
            _onChanged.Invoke(_value);
        }
    }
}