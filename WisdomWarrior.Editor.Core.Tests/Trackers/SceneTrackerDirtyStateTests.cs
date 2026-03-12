using System.Numerics;
using WisdomWarrior.Editor.Core.ShadowTree;
using WisdomWarrior.Editor.Core.Tests.TestInfrastructure;
using WisdomWarrior.Engine.Core;
using WisdomWarrior.Engine.Core.Components;

namespace WisdomWarrior.Editor.Core.Tests.Trackers;

public class SceneTrackerDirtyStateTests
{
    [Fact]
    public void Update_WithNoChanges_DoesNotRaiseSceneModified()
    {
        var scene = TestSceneFactory.CreateSceneWithRoot();
        var tracker = new SceneTracker();
        tracker.TrackScene(scene);

        var modifiedCount = 0;
        tracker.OnSceneModified += () => modifiedCount++;

        tracker.Update();

        Assert.Equal(0, modifiedCount);
    }

    [Fact]
    public void Update_WhenSceneRenamed_RaisesSceneModified()
    {
        var scene = TestSceneFactory.CreateSceneWithRoot();
        var tracker = new SceneTracker();
        tracker.TrackScene(scene);

        var modifiedCount = 0;
        tracker.OnSceneModified += () => modifiedCount++;

        scene.Name = "RenamedScene";
        tracker.Update();

        Assert.Equal(1, modifiedCount);
    }

    [Fact]
    public void Update_WhenRootAddedAndRemoved_RaisesSceneModifiedAndSyncsRoots()
    {
        var scene = TestSceneFactory.CreateSceneWithRoot();
        var tracker = new SceneTracker();
        tracker.TrackScene(scene);

        var modifiedCount = 0;
        tracker.OnSceneModified += () => modifiedCount++;

        var secondRoot = TestSceneFactory.CreateEntityWithTransform("Root2", new Vector2(5f, 5f));
        scene.AddEntity(secondRoot);
        tracker.Update();

        Assert.Equal(1, modifiedCount);
        Assert.Equal(2, tracker.TrackedRoots.Count);

        scene.Entities.Remove(secondRoot);
        tracker.Update();

        Assert.Equal(2, modifiedCount);
        Assert.Single(tracker.TrackedRoots);
    }

    [Fact]
    public void Update_WhenComponentPropertyChanges_RaisesSceneModifiedThenSettles()
    {
        var scene = TestSceneFactory.CreateSceneWithRoot();
        var tracker = new SceneTracker();
        tracker.TrackScene(scene);

        var modifiedCount = 0;
        tracker.OnSceneModified += () => modifiedCount++;

        var root = scene.Entities[0];
        var transform = root.Components.OfType<Transform>().Single();
        transform.Position = new Vector2(42f, 24f);

        tracker.Update();
        tracker.Update();

        Assert.Equal(1, modifiedCount);
    }

    [Fact]
    public void Update_WhenComponentAddedOrRemoved_RaisesSceneModified()
    {
        var scene = TestSceneFactory.CreateSceneWithRoot();
        var tracker = new SceneTracker();
        tracker.TrackScene(scene);

        var modifiedCount = 0;
        tracker.OnSceneModified += () => modifiedCount++;

        var root = scene.Entities[0];
        var sprite = new Sprite();

        root.AddComponent(sprite);
        tracker.Update();

        root.RemoveComponent(sprite);
        tracker.Update();

        Assert.Equal(2, modifiedCount);
    }

    [Fact]
    public void Update_WhenChildAddedOrRemoved_RaisesSceneModified()
    {
        var scene = TestSceneFactory.CreateSceneWithRoot();
        var tracker = new SceneTracker();
        tracker.TrackScene(scene);

        var modifiedCount = 0;
        tracker.OnSceneModified += () => modifiedCount++;

        var root = scene.Entities[0];
        var child = new GameEntity { Name = "Child" };

        root.AddEntity(child);
        tracker.Update();

        root.Children.Remove(child);
        tracker.Update();

        Assert.Equal(2, modifiedCount);
    }
}
