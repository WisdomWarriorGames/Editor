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
