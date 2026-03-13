using WisdomWarrior.Editor.Core.Services;
using WisdomWarrior.Editor.Core.ShadowTree;
using WisdomWarrior.Editor.Core.Tests.TestInfrastructure;
using WisdomWarrior.Editor.Inspector.ViewModels;
using WisdomWarrior.Engine.Core;
using WisdomWarrior.Engine.Core.Systems;

namespace WisdomWarrior.Editor.Core.Tests.Inspector;

public class InspectorViewModelTests
{
    [Fact]
    public void SelectingScene_ProducesSceneInspectorAndRegistryBackedAddOptions()
    {
        TestComponentRegistry.EnsureBootstrapped();

        var selectionManager = new SelectionManager();
        var inspector = new InspectorViewModel(selectionManager);
        var scene = new Scene { Name = "SceneAlpha" };
        var tracker = new SceneTracker();

        tracker.TrackScene(scene);
        selectionManager.SetSelection(tracker);

        var content = Assert.IsType<SceneInspectorViewModel>(inspector.CurrentContent);
        Assert.True(inspector.CanAddItems);
        Assert.Contains(nameof(SpriteRenderSystem), inspector.AvailableAddNames);
        Assert.Equal("SceneAlpha", inspector.Name);
        Assert.Empty(content.Systems);
    }

    [Fact]
    public void SceneInspector_AddSystemCommand_AttachesSystemToTrackedScene()
    {
        TestComponentRegistry.EnsureBootstrapped();

        var scene = new Scene { Name = "SceneAlpha" };
        var tracker = new SceneTracker();
        tracker.TrackScene(scene);

        var inspector = new SceneInspectorViewModel(tracker);

        inspector.AddSystemCommand.Execute(nameof(SpriteRenderSystem));

        tracker.Update();

        var system = Assert.Single(scene.Systems);
        Assert.IsType<SpriteRenderSystem>(system);
        Assert.Single(tracker.TrackedSystems);
    }
}
