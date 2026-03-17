using WisdomWarrior.Editor.Core.ShadowTree;
using WisdomWarrior.Engine.Core;

namespace WisdomWarrior.Editor.Core.Tests.Trackers;

public class SceneTrackerLookupTests
{
    [Fact]
    public void FindTrackerByEntity_ReturnsTrackerForNestedEntity()
    {
        var scene = new Scene { Name = "LookupScene" };
        var root = new GameEntity { Name = "Root" };
        var child = new GameEntity { Name = "Child" };
        var grandchild = new GameEntity { Name = "Grandchild" };

        child.AddEntity(grandchild);
        root.AddEntity(child);
        scene.AddEntity(root);

        var tracker = new SceneTracker();
        tracker.TrackScene(scene);

        var match = tracker.FindTrackerByEntity(grandchild);

        Assert.NotNull(match);
        Assert.Same(grandchild, match!.EngineEntity);
    }

    [Fact]
    public void FindTrackerByEntity_ReturnsNullWhenEntityIsNotTracked()
    {
        var scene = new Scene { Name = "LookupScene" };
        var trackedEntity = new GameEntity { Name = "Tracked" };
        scene.AddEntity(trackedEntity);

        var untrackedEntity = new GameEntity { Name = "Untracked" };

        var tracker = new SceneTracker();
        tracker.TrackScene(scene);

        var match = tracker.FindTrackerByEntity(untrackedEntity);

        Assert.Null(match);
    }
}
