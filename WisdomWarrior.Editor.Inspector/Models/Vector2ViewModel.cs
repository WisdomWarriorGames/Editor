using System.Numerics;
using CommunityToolkit.Mvvm.ComponentModel;

namespace WisdomWarrior.Editor.Inspector.Models;

public class Vector2ViewModel : ObservableObject
{
    private readonly Action<Vector2> _onChanged;
    private Vector2 _value;

    public Vector2ViewModel(Vector2 value, Action<Vector2> onChanged)
    {
        _value = value;
        _onChanged = onChanged;
    }

    public void UpdateFromEngine(Vector2 newValue)
    {
        if (_value == newValue) return;

        _value = newValue;

        OnPropertyChanged(nameof(X));
        OnPropertyChanged(nameof(Y));
    }

    public float X
    {
        get => _value.X;
        set
        {
            if (_value.X == value) return;
            _value.X = value;
            OnPropertyChanged();
            _onChanged.Invoke(_value);
        }
    }

    public float Y
    {
        get => _value.Y;
        set
        {
            if (_value.Y == value) return;
            _value.Y = value;
            OnPropertyChanged();
            _onChanged.Invoke(_value);
        }
    }
}