using System.Numerics;
using CommunityToolkit.Mvvm.ComponentModel;
using WisdomWarrior.Engine.Core.DataTypes;

namespace WisdomWarrior.Editor.Inspector.Models;

public class SizeViewModel : ObservableObject
{
    private readonly Action<Size> _onChanged;
    private Size _value;

    public SizeViewModel(Size value, Action<Size> onChanged)
    {
        _value = value;
        _onChanged = onChanged;
    }

    public void UpdateFromEngine(Size newValue)
    {
        if (_value == newValue) return;

        _value = newValue;

        OnPropertyChanged(nameof(W));
        OnPropertyChanged(nameof(H));
    }

    public float W
    {
        get => _value.W;
        set
        {
            if (_value.W == value) return;
            _value.W = value;
            OnPropertyChanged();
            _onChanged.Invoke(_value);
        }
    }

    public float H
    {
        get => _value.H;
        set
        {
            if (_value.H == value) return;
            _value.H = value;
            OnPropertyChanged();
            _onChanged.Invoke(_value);
        }
    }
}