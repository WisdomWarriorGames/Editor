using System.Reflection;
using WisdomWarrior.Editor.Core.Helpers;
using WisdomWarrior.Editor.Core.ShadowTree;
using WisdomWarrior.Engine.Core.Attributes;

namespace WisdomWarrior.Editor.Inspector.Helpers;

public interface IInspectorProperty
{
    string Name { get; }
    Type PropertyType { get; }
    object? GetValue();
    void SetValue(object? value);
    event Action<object?>? OnValueChanged;
    T? GetCustomAttribute<T>() where T : Attribute;
}

public sealed record VisibleInspectorProperty(IInspectorProperty Property, bool DisposeWhenDetached);

public sealed class TrackerInspectorProperty : IInspectorProperty, IDisposable
{
    private readonly PropertyTracker _propertyTracker;
    private object? _lastObservedValue;
    private bool _disposed;

    public TrackerInspectorProperty(PropertyTracker propertyTracker)
    {
        _propertyTracker = propertyTracker;
        _lastObservedValue = propertyTracker.GetValue();
        _propertyTracker.OnValueChanged += HandleTrackerValueChanged;
    }

    public string Name => _propertyTracker.Name;
    public Type PropertyType => _propertyTracker.PropertyType;

    public event Action<object?>? OnValueChanged;

    public object? GetValue() => _propertyTracker.GetValue();

    public void SetValue(object? value)
    {
        if (Equals(_propertyTracker.GetValue(), value))
        {
            return;
        }

        _propertyTracker.SetValue(value);
        _lastObservedValue = value;
        OnValueChanged?.Invoke(value);
    }

    public T? GetCustomAttribute<T>() where T : Attribute
    {
        return _propertyTracker.GetCustomAttribute<T>();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _propertyTracker.OnValueChanged -= HandleTrackerValueChanged;
        _disposed = true;
    }

    private void HandleTrackerValueChanged(object? newValue)
    {
        if (Equals(_lastObservedValue, newValue))
        {
            return;
        }

        _lastObservedValue = newValue;
        OnValueChanged?.Invoke(newValue);
    }
}

public sealed class NestedInspectorProperty : IInspectorProperty, IDisposable
{
    private readonly IInspectorProperty _parent;
    private readonly PropertyInfo _propertyInfo;
    private object? _lastObservedValue;
    private bool _disposed;

    public NestedInspectorProperty(IInspectorProperty parent, PropertyInfo propertyInfo)
    {
        _parent = parent;
        _propertyInfo = propertyInfo;
        _lastObservedValue = GetValue();
        _parent.OnValueChanged += HandleParentValueChanged;
    }

    public string Name => _propertyInfo.Name;
    public Type PropertyType => _propertyInfo.PropertyType;

    public event Action<object?>? OnValueChanged;

    public object? GetValue()
    {
        var parentValue = _parent.GetValue();
        return parentValue is null ? null : _propertyInfo.GetValue(parentValue);
    }

    public void SetValue(object? value)
    {
        if (Equals(GetValue(), value))
        {
            return;
        }

        var parentValue = _parent.GetValue();
        if (parentValue is null)
        {
            return;
        }

        var updatedParent = parentValue;
        _propertyInfo.SetValue(updatedParent, value);
        _lastObservedValue = _propertyInfo.GetValue(updatedParent);
        _parent.SetValue(updatedParent);
        OnValueChanged?.Invoke(_lastObservedValue);
    }

    public T? GetCustomAttribute<T>() where T : Attribute
    {
        return _propertyInfo.GetCustomAttribute<T>();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _parent.OnValueChanged -= HandleParentValueChanged;
        _disposed = true;
    }

    private void HandleParentValueChanged(object? _)
    {
        var currentValue = GetValue();
        if (Equals(_lastObservedValue, currentValue))
        {
            return;
        }

        _lastObservedValue = currentValue;
        OnValueChanged?.Invoke(currentValue);
    }
}

public static class InspectorPropertyExpander
{
    public static IReadOnlyList<VisibleInspectorProperty> ExpandVisibleProperties(IEnumerable<IInspectorProperty> rootProperties)
    {
        ArgumentNullException.ThrowIfNull(rootProperties);

        var visibleProperties = new List<VisibleInspectorProperty>();
        foreach (var property in rootProperties)
        {
            ExpandProperty(property, disposeWhenDetached: false, visibleProperties);
        }

        return visibleProperties;
    }

    private static void ExpandProperty(
        IInspectorProperty property,
        bool disposeWhenDetached,
        ICollection<VisibleInspectorProperty> visibleProperties)
    {
        if (property.GetCustomAttribute<HideInInspectorAttribute>() is not null)
        {
            DisposeIfOwned(property, disposeWhenDetached);
            return;
        }

        if (!ShouldFlatten(property))
        {
            visibleProperties.Add(new VisibleInspectorProperty(property, disposeWhenDetached));
            return;
        }

        var childProperties = ReflectionCache.GetTrackableProperties(property.PropertyType)
            .Where(childProperty => ShouldIncludeChildProperty(property, childProperty))
            .Select(childProperty => new NestedInspectorProperty(property, childProperty))
            .ToList();

        if (childProperties.Count == 0)
        {
            visibleProperties.Add(new VisibleInspectorProperty(property, disposeWhenDetached));
            return;
        }

        foreach (var childProperty in childProperties)
        {
            ExpandProperty(childProperty, disposeWhenDetached: true, visibleProperties);
        }
    }

    private static bool ShouldFlatten(IInspectorProperty property)
    {
        return !PropertyEditors.SupportsDirectEditor(property.PropertyType)
               && property.PropertyType != typeof(string)
               && ReflectionCache.GetTrackableProperties(property.PropertyType).Length > 0;
    }

    private static bool ShouldIncludeChildProperty(IInspectorProperty parent, PropertyInfo childProperty)
    {
        if (childProperty.GetCustomAttribute<HideInInspectorAttribute>() is not null)
        {
            return false;
        }

        var gatingProperty = parent.PropertyType.GetProperty($"Use{childProperty.Name}");
        if (gatingProperty?.PropertyType != typeof(bool))
        {
            return true;
        }

        var parentValue = parent.GetValue();
        return parentValue is not null && (bool)(gatingProperty.GetValue(parentValue) ?? false);
    }

    private static void DisposeIfOwned(IInspectorProperty property, bool disposeWhenDetached)
    {
        if (!disposeWhenDetached || property is not IDisposable disposable)
        {
            return;
        }

        disposable.Dispose();
    }
}
