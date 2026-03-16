using CommunityToolkit.Mvvm.ComponentModel;

namespace WisdomWarrior.Editor.Inspector.Models;

public class EnumPropertyViewModel : ObservableObject
{
    private readonly Action<object> _onChanged;
    private object _value;

    public EnumPropertyViewModel(Type enumType, object value, Action<object> onChanged)
    {
        Values = Enum.GetValues(enumType).Cast<object>().ToArray();
        _value = value;
        _onChanged = onChanged;
    }

    public IReadOnlyList<object> Values { get; }

    public void UpdateFromEngine(object newValue)
    {
        if (Equals(_value, newValue))
        {
            return;
        }

        _value = newValue;
        OnPropertyChanged(nameof(Value));
    }

    public object Value
    {
        get => _value;
        set
        {
            if (Equals(_value, value))
            {
                return;
            }

            _value = value;
            OnPropertyChanged();
            _onChanged.Invoke(_value);
        }
    }
}
