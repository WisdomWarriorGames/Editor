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

    [Fact]
    public void EntityInspector_AvailableComponentNames_HidesAndRestoresLimitToOneTypes()
    {
        TestComponentRegistry.EnsureBootstrapped();
        WisdomWarrior.Engine.Core.Components.ComponentRegistry.Register<LimitedInspectorComponent>(nameof(LimitedInspectorComponent));

        var entity = new GameEntity { Name = "Player" };
        var tracker = new EntityTracker(entity);
        var inspector = new EntityInspectorViewModel(tracker);

        Assert.Contains(nameof(LimitedInspectorComponent), inspector.AvailableComponentNames);

        entity.AddComponent(new LimitedInspectorComponent());
        tracker.Update();

        Assert.DoesNotContain(nameof(LimitedInspectorComponent), inspector.AvailableComponentNames);

        entity.RemoveComponent(entity.Components.OfType<LimitedInspectorComponent>().Single());
        tracker.Update();

        Assert.Contains(nameof(LimitedInspectorComponent), inspector.AvailableComponentNames);
    }

    [Fact]
    public void InspectorViewModel_AvailableAddNames_RefreshesWhenLimitToOneSystemIsAdded()
    {
        TestComponentRegistry.EnsureBootstrapped();
        SystemRegistry.Register<LimitedInspectorSystem>(nameof(LimitedInspectorSystem));

        var selectionManager = new SelectionManager();
        var inspector = new InspectorViewModel(selectionManager);
        var scene = new Scene { Name = "SceneAlpha" };
        var tracker = new SceneTracker();

        tracker.TrackScene(scene);
        selectionManager.SetSelection(tracker);

        Assert.Contains(nameof(LimitedInspectorSystem), inspector.AvailableAddNames);

        scene.AddSystem(new LimitedInspectorSystem());
        tracker.Update();

        Assert.DoesNotContain(nameof(LimitedInspectorSystem), inspector.AvailableAddNames);

        scene.RemoveSystem(scene.Systems.OfType<LimitedInspectorSystem>().Single());
        tracker.Update();

        Assert.Contains(nameof(LimitedInspectorSystem), inspector.AvailableAddNames);
    }

    [WisdomWarrior.Engine.Core.Attributes.LimitToOne]
    private sealed class LimitedInspectorComponent : Component
    {
    }

    [WisdomWarrior.Engine.Core.Attributes.LimitToOne]
    private sealed class LimitedInspectorSystem : BehaviourSystem
    {
        public override void OnStart()
        {
        }

        public override void Update()
        {
        }
    }
}
