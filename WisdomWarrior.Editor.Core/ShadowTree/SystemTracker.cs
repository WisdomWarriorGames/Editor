using WisdomWarrior.Editor.Core.Helpers;
using SceneSystem = WisdomWarrior.Engine.Core.Systems.System;

namespace WisdomWarrior.Editor.Core.ShadowTree;

public class SystemTracker : IInspectableObjectTracker
{
    private readonly List<PropertyTracker> _properties = new();

    public SceneSystem EngineSystem { get; }
    public string Name => EngineSystem.GetType().Name;
    public bool IsDirty => _properties.Any(p => p.IsDirty);
    public IReadOnlyList<PropertyTracker> Properties => _properties;

    public SystemTracker(SceneSystem system)
    {
        EngineSystem = system;

        var cachedProperties = ReflectionCache.GetTrackableProperties(system.GetType());
        _properties.Capacity = cachedProperties.Length;

        foreach (var prop in cachedProperties)
        {
            _properties.Add(new PropertyTracker(prop, system));
        }
    }

    public void Update()
    {
        foreach (var property in _properties)
        {
            property.CheckForChanges();
        }
    }

    public void AcknowledgeSaved()
    {
        foreach (var property in _properties)
        {
            property.AcknowledgeSaved();
        }
    }
}
