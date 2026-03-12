using System.ComponentModel;
using System.Runtime.CompilerServices;
using WisdomWarrior.Editor.Core.Helpers;
using Component = WisdomWarrior.Engine.Core.Component; // Assuming your base Component lives here

namespace WisdomWarrior.Editor.Core.ShadowTree;

public class ComponentTracker : INotifyPropertyChanged
{
    private readonly List<PropertyTracker> _properties = new();

    public Component EngineComponent { get; }

    public bool IsDirty => _properties.Any(p => p.IsDirty);
    public List<PropertyTracker> Properties => _properties;

    public string Name => EngineComponent.GetType().Name;

    private bool _lastActiveState;

    public bool Active
    {
        get => EngineComponent.Active;
        set
        {
            if (EngineComponent.Active != value)
            {
                EngineComponent.Active = value;
                _lastActiveState = value;
                OnPropertyChanged();
            }
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ComponentTracker(Component component)
    {
        EngineComponent = component;
        _lastActiveState = component.Active;

        var cachedProperties = ReflectionCache.GetTrackableProperties(component.GetType());
        _properties.Capacity = cachedProperties.Length;

        foreach (var prop in cachedProperties)
        {
            _properties.Add(new PropertyTracker(prop, component));
        }
    }

    public void Update()
    {
        if (EngineComponent.Active != _lastActiveState)
        {
            _lastActiveState = EngineComponent.Active;
            OnPropertyChanged(nameof(Active));
        }

        foreach (var p in _properties)
        {
            p.CheckForChanges();
        }
    }

    public void AcknowledgeSaved()
    {
        foreach (var property in _properties)
        {
            property.AcknowledgeSaved();
        }
    }

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
