using CommunityToolkit.Mvvm.ComponentModel;
using WisdomWarrior.Editor.Core.ShadowTree;
using WisdomWarrior.Engine.Core.Components;

namespace WisdomWarrior.Editor.Inspector.ViewModels;

public partial class EntityInspectorViewModel : ObservableObject
{
    [ObservableProperty] private EntityTracker? _entity;
    public IEnumerable<ComponentTracker> Components => Entity?.TrackedComponents ?? Enumerable.Empty<ComponentTracker>();
    public IEnumerable<string> AvailableComponentNames => ComponentRegistry.GetRegisteredKeys();

    public EntityInspectorViewModel(EntityTracker entity)
    {
        _entity = entity;
    }
}