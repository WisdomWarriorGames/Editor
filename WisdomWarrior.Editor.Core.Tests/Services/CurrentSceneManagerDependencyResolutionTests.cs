using WisdomWarrior.Editor.Core.Services;
using WisdomWarrior.Engine.Core;
using WisdomWarrior.Engine.Core.Attributes;
using WisdomWarrior.Engine.Core.Systems;

namespace WisdomWarrior.Editor.Core.Tests.Services;

public class CurrentSceneManagerDependencyResolutionTests
{
    [Fact]
    public async Task EditorTick_ResolvesDependencies_AndUpdatesTrackerState()
    {
        var sceneName = $"TickScene_{Guid.NewGuid():N}";
        var manager = new CurrentSceneManager
        {
            TicksPerSecond = 60
        };

        try
        {
            manager.CreateInMemoryScene(sceneName);

            var entity = new GameEntity { Name = "Player" };
            manager.Tracker.AddEntity(entity);
            manager.Tracker.AcknowledgeSaved();

            entity.Components.Add(new TickRequiresSystemComponent());

            await WaitForConditionAsync(() => manager.ActiveScene!.Systems.Any(system => system is TickResolvedSystem));
            await WaitForConditionAsync(() => manager.Tracker.TrackedSystems.Any(system => system.EngineSystem is TickResolvedSystem));

            Assert.True(manager.IsDirty);
        }
        finally
        {
            manager.StopTicking();
            SceneManager.RemoveScene(sceneName);
        }
    }

    [Fact]
    public async Task EditorTick_LimitToOneCleanup_RemovesDuplicatesAndMarksSceneDirty()
    {
        var sceneName = $"LimitTickScene_{Guid.NewGuid():N}";
        var manager = new CurrentSceneManager
        {
            TicksPerSecond = 60
        };

        try
        {
            manager.CreateInMemoryScene(sceneName);

            var entity = new GameEntity { Name = "Player" };
            manager.Tracker.AddEntity(entity);
            manager.Tracker.AcknowledgeSaved();

            entity.Components.Add(new TickLimitedComponent());
            entity.Components.Add(new TickLimitedComponent());
            manager.ActiveScene!.Systems.Add(new TickLimitedSystem());
            manager.ActiveScene.Systems.Add(new TickLimitedSystem());

            await WaitForConditionAsync(() => entity.Components.OfType<TickLimitedComponent>().Count() == 1);
            await WaitForConditionAsync(() => manager.ActiveScene!.Systems.OfType<TickLimitedSystem>().Count() == 1);
            await WaitForConditionAsync(() => manager.Tracker.TrackedRoots.Single().TrackedComponents.Count(tracker => tracker.EngineComponent is TickLimitedComponent) == 1);
            await WaitForConditionAsync(() => manager.Tracker.TrackedSystems.Count(tracker => tracker.EngineSystem is TickLimitedSystem) == 1);

            Assert.True(manager.IsDirty);
        }
        finally
        {
            manager.StopTicking();
            SceneManager.RemoveScene(sceneName);
        }
    }

    private static async Task WaitForConditionAsync(Func<bool> condition)
    {
        var deadline = DateTime.UtcNow.AddSeconds(2);

        while (DateTime.UtcNow < deadline)
        {
            if (condition())
            {
                return;
            }

            await Task.Delay(25);
        }

        Assert.True(condition());
    }

    [RequireSystem(typeof(TickResolvedSystem))]
    public sealed class TickRequiresSystemComponent : Component
    {
    }

    [LimitToOne]
    public sealed class TickLimitedComponent : Component
    {
    }

    [LimitToOne]
    public sealed class TickLimitedSystem : BehaviourSystem
    {
        public override void OnStart()
        {
        }

        public override void Update()
        {
        }
    }

    public sealed class TickResolvedSystem : BehaviourSystem
    {
        public override void OnStart()
        {
        }

        public override void Update()
        {
        }
    }
}
