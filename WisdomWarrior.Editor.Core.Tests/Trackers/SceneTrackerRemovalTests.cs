using System.Numerics;
using WisdomWarrior.Editor.Core.ShadowTree;
using WisdomWarrior.Engine.Core;
using WisdomWarrior.Engine.Core.Assets;
using WisdomWarrior.Engine.Core.Components;
using EngineSize = WisdomWarrior.Engine.Core.DataTypes.Size;

namespace WisdomWarrior.Editor.Core.Tests.Trackers;

public class SceneTrackerRemovalTests
{
    [Fact]
    public void RemoveEntity_RemovesRootFromSceneTrackerAndDetachesFromScene()
    {
        var scene = new Scene { Name = "TrackerRootRemoveScene" };
        var root = CreateEntityWithSprite("Root", "Assets/root.png");
        scene.AddEntity(root);

        var tracker = new SceneTracker();
        tracker.TrackScene(scene);
        Assert.Single(tracker.TrackedRoots);

        tracker.RemoveEntity(root);

        Assert.Empty(scene.Entities);
        Assert.Empty(tracker.TrackedRoots);
        Assert.Null(root.CurrentScene);
        Assert.Null(root.Parent);
        Assert.Empty(scene.GetEntitiesWith<Sprite>());
    }

    [Fact]
    public void RemoveEntity_RemovesChildSubtreeAndDetachesFromScene()
    {
        var scene = new Scene { Name = "TrackerChildRemoveScene" };
        var root = CreateEntityWithSprite("Root", "Assets/root.png");
        var child = CreateEntityWithSprite("Child", "Assets/child.png");
        var grandchild = CreateEntityWithSprite("Grandchild", "Assets/grandchild.png");
        child.AddEntity(grandchild);
        root.AddEntity(child);
        scene.AddEntity(root);

        var tracker = new SceneTracker();
        tracker.TrackScene(scene);

        Assert.Single(tracker.TrackedRoots);
        Assert.Single(tracker.TrackedRoots[0].TrackedChildren);
        Assert.Equal(3, scene.GetEntitiesWith<Sprite>().Count());

        tracker.RemoveEntity(child);

        Assert.Single(tracker.TrackedRoots);
        Assert.Empty(tracker.TrackedRoots[0].TrackedChildren);
        Assert.Single(scene.GetEntitiesWith<Sprite>());
        Assert.Null(child.Parent);
        Assert.Null(child.CurrentScene);
        Assert.Null(grandchild.CurrentScene);
    }

    private static GameEntity CreateEntityWithSprite(string name, string path)
    {
        var entity = new GameEntity { Name = name };
        entity.AddComponent(new Transform { Position = new Vector2(1f, 2f) });
        entity.AddComponent(new Sprite
        {
            Image = new ImageAsset
            {
                AssetPath = path,
                Dimensions = new EngineSize(16, 16)
            }
        });
        return entity;
    }
}
