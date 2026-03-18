using WisdomWarrior.Editor.Core.ShadowTree;
using DependencyTypes = WisdomWarrior.Editor.Core.Tests.Engine.AttachmentDependencyTests;
using WisdomWarrior.Engine.Core;

namespace WisdomWarrior.Editor.Core.Tests.Trackers;

public class SceneTrackerDependencyResolutionTests
{
    [Fact]
    public void TrackScene_ResolvesExistingDependencies_WithoutMarkingSceneDirty()
    {
        var entity = new GameEntity
        {
            Name = "Player",
            Components = [new DependencyTypes.RequiresComponentAndSystemComponent()]
        };

        var scene = new Scene
        {
            Name = "TrackedScene",
            Entities = [entity]
        };

        scene.Initialize();

        var tracker = new SceneTracker();
        tracker.TrackScene(scene);

        Assert.Single(entity.Components.OfType<DependencyTypes.RequiresComponentAndSystemComponent>());
        var requiredComponent = Assert.Single(entity.Components.OfType<DependencyTypes.SiblingDependencyComponent>());
        Assert.Same(entity, requiredComponent.Parent);
        Assert.Contains(scene.Systems, system => system is DependencyTypes.FirstRequiredSystem);
        Assert.Single(scene.GetEntitiesWith<DependencyTypes.SiblingDependencyComponent>());
        Assert.Single(tracker.TrackedRoots);
        Assert.Contains(tracker.TrackedSystems, system => system.EngineSystem is DependencyTypes.FirstRequiredSystem);
        Assert.Contains(tracker.TrackedRoots[0].TrackedComponents, component => component.EngineComponent is DependencyTypes.SiblingDependencyComponent);
        Assert.False(tracker.IsDirty);
    }

    [Fact]
    public void Update_ResolvesDependenciesAddedOutsideAttachApi_AndMarksSceneDirty()
    {
        var scene = new Scene { Name = "UpdateScene" };
        var entity = new GameEntity { Name = "Player" };
        scene.AddEntity(entity);
        scene.Initialize();

        var tracker = new SceneTracker();
        tracker.TrackScene(scene);
        tracker.AcknowledgeSaved();

        entity.Components.Add(new DependencyTypes.RequiresComponentAndSystemComponent());
        tracker.Update();

        Assert.Single(entity.Components.OfType<DependencyTypes.RequiresComponentAndSystemComponent>());
        Assert.Single(entity.Components.OfType<DependencyTypes.SiblingDependencyComponent>());
        Assert.Contains(scene.Systems, system => system is DependencyTypes.FirstRequiredSystem);
        Assert.Single(scene.GetEntitiesWith<DependencyTypes.SiblingDependencyComponent>());
        Assert.Contains(tracker.TrackedSystems, system => system.EngineSystem is DependencyTypes.FirstRequiredSystem);
        Assert.Contains(tracker.TrackedRoots[0].TrackedComponents, component => component.EngineComponent is DependencyTypes.SiblingDependencyComponent);
        Assert.True(tracker.IsDirty);
    }

    [Fact]
    public void Update_ResolvesNestedAndCyclicDependencies_ToAStableGraph()
    {
        var scene = new Scene { Name = "DependencyGraphScene" };
        var entity = new GameEntity { Name = "Player" };
        scene.AddEntity(entity);
        scene.Initialize();

        var tracker = new SceneTracker();
        tracker.TrackScene(scene);
        tracker.AcknowledgeSaved();

        entity.Components.Add(new DependencyTypes.RootChainComponent());
        entity.Components.Add(new DependencyTypes.ComponentCycleA());
        scene.Systems.Add(new DependencyTypes.RootChainSystem());
        scene.Systems.Add(new DependencyTypes.SystemCycleA());

        tracker.Update();

        Assert.Single(entity.Components.OfType<DependencyTypes.RootChainComponent>());
        Assert.Single(entity.Components.OfType<DependencyTypes.MidChainComponent>());
        Assert.Single(entity.Components.OfType<DependencyTypes.LeafChainComponent>());
        Assert.Single(entity.Components.OfType<DependencyTypes.ComponentCycleA>());
        Assert.Single(entity.Components.OfType<DependencyTypes.ComponentCycleB>());
        Assert.Single(scene.Systems.OfType<DependencyTypes.RootChainSystem>());
        Assert.Single(scene.Systems.OfType<DependencyTypes.MidChainSystem>());
        Assert.Single(scene.Systems.OfType<DependencyTypes.LeafChainSystem>());
        Assert.Single(scene.Systems.OfType<DependencyTypes.SystemCycleA>());
        Assert.Single(scene.Systems.OfType<DependencyTypes.SystemCycleB>());
    }

    [Fact]
    public void TrackScene_CleansUpLimitToOneDuplicates_WithoutMarkingSceneDirty()
    {
        var firstComponent = new DependencyTypes.LimitedComponent { Name = "First" };
        var secondComponent = new DependencyTypes.LimitedComponent { Name = "Second" };
        var firstSystem = new DependencyTypes.LimitedSystem { Name = "FirstSystem" };
        var secondSystem = new DependencyTypes.LimitedSystem { Name = "SecondSystem" };

        var entity = new GameEntity
        {
            Name = "Player",
            Components = [firstComponent, secondComponent]
        };

        var scene = new Scene
        {
            Name = "LimitScene",
            Entities = [entity],
            Systems = [firstSystem, secondSystem]
        };

        scene.Initialize();

        var tracker = new SceneTracker();
        tracker.TrackScene(scene);

        var keptComponent = Assert.Single(entity.Components.OfType<DependencyTypes.LimitedComponent>());
        var keptSystem = Assert.Single(scene.Systems.OfType<DependencyTypes.LimitedSystem>());

        Assert.Same(firstComponent, keptComponent);
        Assert.Same(entity, keptComponent.Parent);
        Assert.Same(firstSystem, keptSystem);
        Assert.Same(scene, keptSystem.Scene);
        Assert.False(tracker.IsDirty);
    }

    [Fact]
    public void Update_CleansUpLimitToOneDuplicates_AndMarksSceneDirty()
    {
        var scene = new Scene { Name = "LimitUpdateScene" };
        var entity = new GameEntity { Name = "Player" };
        scene.AddEntity(entity);
        scene.Initialize();

        var tracker = new SceneTracker();
        tracker.TrackScene(scene);
        tracker.AcknowledgeSaved();

        var firstComponent = new DependencyTypes.LimitedComponent { Name = "First" };
        var secondComponent = new DependencyTypes.LimitedComponent { Name = "Second" };
        entity.Components.Add(firstComponent);
        entity.Components.Add(secondComponent);
        scene.Systems.Add(new DependencyTypes.LimitedSystem { Name = "FirstSystem" });
        scene.Systems.Add(new DependencyTypes.LimitedSystem { Name = "SecondSystem" });

        tracker.Update();

        var keptComponent = Assert.Single(entity.Components.OfType<DependencyTypes.LimitedComponent>());
        var keptSystem = Assert.Single(scene.Systems.OfType<DependencyTypes.LimitedSystem>());

        Assert.Same(firstComponent, keptComponent);
        Assert.Same(entity, keptComponent.Parent);
        Assert.Same(scene, keptSystem.Scene);
        Assert.True(tracker.IsDirty);
    }

    [Fact]
    public void Update_UsesExactTypeMatching_WhenCheckingExistingComponents()
    {
        var scene = new Scene { Name = "ExactTypeScene" };
        var entity = new GameEntity { Name = "Player" };
        scene.AddEntity(entity);
        scene.Initialize();

        var tracker = new SceneTracker();
        tracker.TrackScene(scene);
        tracker.AcknowledgeSaved();

        entity.Components.Add(new DependencyTypes.DerivedDependencyComponent());
        entity.Components.Add(new DependencyTypes.RequiresBaseDependencyComponent());

        tracker.Update();

        Assert.Single(entity.Components, component => component.GetType() == typeof(DependencyTypes.BaseDependencyComponent));
        Assert.Single(entity.Components, component => component.GetType() == typeof(DependencyTypes.DerivedDependencyComponent));
    }

    [Fact]
    public void Update_IgnoresInvalidDependencyTargets()
    {
        var scene = new Scene { Name = "InvalidDependencyScene" };
        var entity = new GameEntity { Name = "Player" };
        scene.AddEntity(entity);
        scene.Initialize();

        var tracker = new SceneTracker();
        tracker.TrackScene(scene);
        tracker.AcknowledgeSaved();

        entity.Components.Add(new DependencyTypes.ComponentWithInvalidDependencies());
        scene.Systems.Add(new DependencyTypes.SystemWithInvalidDependencies());

        tracker.Update();

        Assert.Single(entity.Components.OfType<DependencyTypes.ComponentWithInvalidDependencies>());
        Assert.Single(scene.Systems.OfType<DependencyTypes.SystemWithInvalidDependencies>());
    }
}
