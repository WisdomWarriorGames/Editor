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

    public float X
    {
        get => _value.X;
        set
        {
            _value.X = value;
            OnPropertyChanged(nameof(X));
            _onChanged(_value);
        }
    }

    public float Y
    {
        get => _value.Y;
        set
        {
            _value.Y = value;
            OnPropertyChanged(nameof(Y));
            _onChanged(_value);
        }
    }
}