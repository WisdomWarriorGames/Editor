using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WisdomWarrior.Editor.Core.ShadowTree;
using WisdomWarrior.Engine.Core.Components;

namespace WisdomWarrior.Editor.Inspector.ViewModels;

public partial class EntityInspectorViewModel(EntityTracker entity) : ObservableObject
{
    [ObservableProperty] private EntityTracker? _entity = entity;
    public IEnumerable<ComponentTracker> Components => Entity?.TrackedComponents.ToList() ?? Enumerable.Empty<ComponentTracker>();
    public IEnumerable<string> AvailableComponentNames => ComponentRegistry.GetRegisteredKeys();

    [RelayCommand]
    private void AddComponent(string componentName)
    {
        if (string.IsNullOrWhiteSpace(componentName)) return;

        var newComponent = ComponentRegistry.CreateComponent(componentName);
        if (newComponent == null) return;

        Entity?.EngineEntity.AddComponent(newComponent);
        Entity?.Update();

        OnPropertyChanged(nameof(Components));
    }

    [RelayCommand]
    private void RemoveComponent(ComponentTracker tracker)
    {
        if (tracker == null || Entity == null) return;

        Entity.EngineEntity.RemoveComponent(tracker.EngineComponent);
        Entity.Update();

        OnPropertyChanged(nameof(Components));
    }
}