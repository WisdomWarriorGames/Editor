using WisdomWarrior.Editor.Core.ShadowTree;
using WisdomWarrior.Editor.Core.Tests.TestInfrastructure;
using WisdomWarrior.Engine.Core;
using WisdomWarrior.Engine.Core.Components;

namespace WisdomWarrior.Editor.Core.Tests.Trackers;

public class TrackerEdgeCaseTests
{
    [Fact]
    public void ComponentTracker_MultipleUpdatesAfterChange_SettlesBackToClean()
    {
        var component = new Transform();
        var tracker = new ComponentTracker(component);

        tracker.Update();
        Assert.False(tracker.IsDirty);

        component.Rotation = 90f;
        tracker.Update();
        Assert.True(tracker.IsDirty);

        tracker.Update();
        Assert.False(tracker.IsDirty);
    }

    [Fact]
    public void ComponentTracker_NullTransitions_AreMarkedDirty()
    {
        var component = new NullableNameComponent();
        var tracker = new ComponentTracker(component);

        tracker.Update();
        Assert.False(tracker.IsDirty);

        component.Nickname = "Hero";
        tracker.Update();
        Assert.True(tracker.IsDirty);

        tracker.Update();
        Assert.False(tracker.IsDirty);

        component.Nickname = null;
        tracker.Update();
        Assert.True(tracker.IsDirty);
    }

    [Fact]
    public void SceneTracker_ReorderingRoots_DoesNotRaiseSceneModified()
    {
        var scene = TestSceneFactory.CreateSceneWithRoot();
        scene.AddEntity(TestSceneFactory.CreateEntityWithTransform("Root2", new(1f, 1f)));

        var tracker = new SceneTracker();
        tracker.TrackScene(scene);

        var modifiedCount = 0;
        tracker.OnSceneModified += () => modifiedCount++;

        scene.Entities.Move(0, 1);
        tracker.Update();

        Assert.Equal(0, modifiedCount);
    }

    private sealed class NullableNameComponent : Component
    {
        public string? Nickname { get; set; }
    }
}
