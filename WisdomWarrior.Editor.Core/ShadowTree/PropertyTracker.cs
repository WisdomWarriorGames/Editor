using System.Reflection;

namespace WisdomWarrior.Editor.Core.ShadowTree;

public class PropertyTracker
{
    private readonly PropertyInfo _propertyInfo;
    private readonly object _target;
    private object? _lastSavedValue;
    private object? _lastObservedValue;

    public string Name => _propertyInfo.Name;
    public bool IsDirty { get; private set; }

    public Type PropertyType => _propertyInfo.PropertyType;

    public event Action<object?>? OnValueChanged;

    public PropertyTracker(PropertyInfo prop, object target)
    {
        _propertyInfo = prop;
        _target = target;
        _lastSavedValue = _propertyInfo.GetValue(_target);
        _lastObservedValue = _lastSavedValue;
    }

    public object? GetValue() => _propertyInfo.GetValue(_target);

    public void SetValue(object? value)
    {
        _propertyInfo.SetValue(_target, value);
    }

    public void CheckForChanges()
    {
        var currentValue = _propertyInfo.GetValue(_target);

        if (!ValuesEqual(currentValue, _lastObservedValue))
        {
            _lastObservedValue = currentValue;
            OnValueChanged?.Invoke(currentValue);
        }

        if (!ValuesEqual(currentValue, _lastSavedValue))
        {
            IsDirty = true;
        }
    }

    public void AcknowledgeSaved()
    {
        _lastSavedValue = _propertyInfo.GetValue(_target);
        _lastObservedValue = _lastSavedValue;
        IsDirty = false;
    }

    public T? GetCustomAttribute<T>() where T : Attribute
    {
        return _propertyInfo.GetCustomAttribute<T>();
    }

    private static bool ValuesEqual(object? left, object? right)
    {
        if (left == null && right == null) return true;
        if (left == null || right == null) return false;

        return left.Equals(right);
    }
}
