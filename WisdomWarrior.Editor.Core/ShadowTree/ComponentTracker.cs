using WisdomWarrior.Editor.Core.Helpers;

namespace WisdomWarrior.Editor.Core.ShadowTree;

public class ComponentTracker
{
    private readonly List<PropertyTracker> _properties = new();
    public object EngineComponent { get; }
    public bool IsDirty => _properties.Any(p => p.IsDirty);

    public List<PropertyTracker> Properties => _properties;

    public ComponentTracker(object component)
    {
        EngineComponent = component;

        var cachedProperties = ReflectionCache.GetTrackableProperties(component.GetType());
        _properties.Capacity = cachedProperties.Length;

        foreach (var prop in cachedProperties)
        {
            _properties.Add(new PropertyTracker(prop, component));
        }
    }

    public void Update()
    {
        foreach (var p in _properties)
        {
            p.CheckForChanges();
        }
    }
}