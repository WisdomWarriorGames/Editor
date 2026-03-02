using System.Reflection;

namespace WisdomWarrior.Editor.Core.ShadowTree;

public class PropertyTracker
{
    private readonly PropertyInfo _propertyInfo;
    private readonly object _target;
    private object? _lastValue;

    public string Name => _propertyInfo.Name;
    public bool IsDirty { get; private set; }

    public Type PropertyType => _propertyInfo.PropertyType;

    public PropertyTracker(PropertyInfo prop, object target)
    {
        _propertyInfo = prop;
        _target = target;
        _lastValue = _propertyInfo.GetValue(_target);
    }

    public object? GetValue() => _propertyInfo.GetValue(_target);

    public void SetValue(object? value)
    {
        _propertyInfo.SetValue(_target, value);
    }

    public void CheckForChanges()
    {
        var currentValue = _propertyInfo.GetValue(_target);

        if (!Equals(currentValue, _lastValue))
        {
            _lastValue = currentValue;
            IsDirty = true;
        }
        else
        {
            IsDirty = false;
        }
    }

    public T? GetCustomAttribute<T>() where T : Attribute
    {
        return _propertyInfo.GetCustomAttribute<T>();
    }
}