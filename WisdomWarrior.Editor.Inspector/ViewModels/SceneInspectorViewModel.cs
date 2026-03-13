using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WisdomWarrior.Editor.Core.ShadowTree;
using WisdomWarrior.Engine.Core.Systems;

namespace WisdomWarrior.Editor.Inspector.ViewModels;

public partial class SceneInspectorViewModel : ObservableObject
{
    [ObservableProperty] private SceneTracker? _scene;

    public IEnumerable<SystemTracker> Systems => Scene?.TrackedSystems.ToList() ?? Enumerable.Empty<SystemTracker>();
    public IEnumerable<string> AvailableSystemNames => SystemRegistry.GetRegisteredKeys();

    public SceneInspectorViewModel(SceneTracker scene)
    {
        _scene = scene;
        _scene.OnStructureChanged += () => OnPropertyChanged(nameof(Systems));
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
}
