using System.Reflection;

namespace WisdomWarrior.Editor.Core.ShadowTree;

public class PropertyTracker
{
    private readonly PropertyInfo _propertyInfo;
    private readonly object _target;
    private object? _lastValue;

    public string Name => _propertyInfo.Name;
    public bool IsDirty { get; private set; }

    public PropertyTracker(PropertyInfo prop, object target)
    {
        _propertyInfo = prop;
        _target = target;
        _lastValue = _propertyInfo.GetValue(_target);
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
}