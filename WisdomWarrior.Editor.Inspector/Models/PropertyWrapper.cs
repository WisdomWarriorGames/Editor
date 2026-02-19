using System.Reflection;
using CommunityToolkit.Mvvm.ComponentModel;

namespace WisdomWarrior.Editor.Inspector.Models;

public class PropertyWrapper : ObservableObject
{
    private readonly object _target;
    private readonly PropertyInfo _property;

    public PropertyWrapper(object target, PropertyInfo property)
    {
        _target = target;
        _property = property;
    }

    public object? Value
    {
        get => _property.GetValue(_target);
        set
        {
            _property.SetValue(_target, value);
            OnPropertyChanged(nameof(Value));
        }
    }
}