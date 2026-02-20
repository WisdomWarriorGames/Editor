using System.Numerics;
using CommunityToolkit.Mvvm.ComponentModel;

namespace WisdomWarrior.Editor.Inspector.Models;

public class FloatViewModel : ObservableObject
{
    private readonly Action<float> _onChanged;
    private float _value;

    public FloatViewModel(float value, Action<float> onChanged)
    {
        _value = value;
        _onChanged = onChanged;
    }

    public void UpdateFromEngine(float newValue)
    {
        if (Math.Abs(_value - newValue) < 0.01) return;

        _value = newValue;

        OnPropertyChanged(nameof(Value));
    }

    public float Value
    {
        get => _value;
        set
        {
            if (Math.Abs(_value - value) < 0.01) return;
            _value = value;
            OnPropertyChanged();
            _onChanged.Invoke(_value);
        }
    }
}