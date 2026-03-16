using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WisdomWarrior.Editor.Core.ShadowTree;
using WisdomWarrior.Engine.Core.Systems;

namespace WisdomWarrior.Editor.Inspector.ViewModels;

public partial class SceneInspectorViewModel : ObservableObject
{
    [ObservableProperty] private SceneTracker? _scene;

    public IEnumerable<SystemTracker> Systems => Scene?.TrackedSystems.ToList() ?? Enumerable.Empty<SystemTracker>();
    public IEnumerable<string> AvailableSystemNames => SystemRegistry.GetRegisteredKeys().Where(CanAddSystemType);

    public SceneInspectorViewModel(SceneTracker scene)
    {
        _scene = scene;
        _scene.OnStructureChanged += () =>
        {
            OnPropertyChanged(nameof(Systems));
            OnPropertyChanged(nameof(AvailableSystemNames));
        };
    }

    [RelayCommand]
    private void AddSystem(string systemName)
    {
        if (string.IsNullOrWhiteSpace(systemName)) return;

        var newSystem = SystemRegistry.CreateSystem(systemName);
        if (newSystem == null) return;

        Scene?.AddSystem(newSystem);
    }

    [RelayCommand]
    private void RemoveSystem(SystemTracker tracker)
    {
        if (tracker == null || Scene == null) return;

        Scene.RemoveSystem(tracker.EngineSystem);
    }

    private bool CanAddSystemType(string systemName)
    {
        var systemType = SystemRegistry.GetType(systemName);
        if (systemType == null || Scene?.ActiveScene == null)
        {
            return false;
        }

        var limitToOne = systemType.GetCustomAttributes(typeof(WisdomWarrior.Engine.Core.Attributes.LimitToOneAttribute), inherit: true).Length > 0;
        if (!limitToOne)
        {
            return true;
        }

        return !Scene.ActiveScene.Systems.Any(system => system.GetType() == systemType);
    }
}
