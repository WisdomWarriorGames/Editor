using System.Numerics;
using WisdomWarrior.Engine.Core;
using WisdomWarrior.Engine.Core.Assets;
using WisdomWarrior.Engine.Core.Components;
using EngineSize = WisdomWarrior.Engine.Core.DataTypes.Size;

namespace WisdomWarrior.Editor.Core.Tests.Engine;

public class EntityReparentingTests
{
    [Fact]
    public void SetParent_WithParentInSameScene_PreservesSceneMembershipAndCache()
    {
        var scene = new Scene { Name = "SameSceneReparent" };
        var rootA = new GameEntity { Name = "RootA" };
        var rootB = new GameEntity { Name = "RootB" };
        var child = CreateEntityWithSprite("Child", "Assets/child.png");

        rootA.AddEntity(child);
        scene.AddEntity(rootA);
        scene.AddEntity(rootB);

        Assert.Single(scene.GetEntitiesWith<Sprite>());
        Assert.Same(scene, child.CurrentScene);
        Assert.Same(rootA, child.Parent);

        child.SetParent(rootB);

        var sprites = scene.GetEntitiesWith<Sprite>().ToList();
        Assert.Single(sprites);
        Assert.Contains(child, sprites);
        Assert.DoesNotContain(child, rootA.Children);
        Assert.Contains(child, rootB.Children);
        Assert.Same(rootB, child.Parent);
        Assert.Same(scene, child.CurrentScene);
    }

    [Fact]
    public void SetParent_WithParentInDifferentScene_MovesSubtreeBetweenSceneCaches()
    {
        var sceneA = new Scene { Name = "SceneA" };
        var sceneB = new Scene { Name = "SceneB" };

        var rootA = new GameEntity { Name = "RootA" };
        var rootB = new GameEntity { Name = "RootB" };
        var child = CreateEntityWithSprite("Child", "Assets/child.png");
        var grandchild = CreateEntityWithSprite("Grandchild", "Assets/grandchild.png");

        child.AddEntity(grandchild);
        rootA.AddEntity(child);
        sceneA.AddEntity(rootA);
        sceneB.AddEntity(rootB);

        Assert.Equal(2, sceneA.GetEntitiesWith<Sprite>().Count());
        Assert.Empty(sceneB.GetEntitiesWith<Sprite>());
        Assert.Same(sceneA, child.CurrentScene);
        Assert.Same(sceneA, grandchild.CurrentScene);

        child.SetParent(rootB);

        Assert.Empty(sceneA.GetEntitiesWith<Sprite>());
        var sceneBSprites = sceneB.GetEntitiesWith<Sprite>().ToList();
        Assert.Equal(2, sceneBSprites.Count);
        Assert.Contains(child, sceneBSprites);
        Assert.Contains(grandchild, sceneBSprites);
        Assert.Same(sceneB, child.CurrentScene);
        Assert.Same(sceneB, grandchild.CurrentScene);
        Assert.DoesNotContain(child, rootA.Children);
        Assert.Contains(child, rootB.Children);
    }

    [Fact]
    public void SetParent_Null_DetachesSubtreeFromSceneAndCache()
    {
        var scene = new Scene { Name = "DetachViaSetParent" };
        var root = new GameEntity { Name = "Root" };
        var child = CreateEntityWithSprite("Child", "Assets/child.png");
        var grandchild = CreateEntityWithSprite("Grandchild", "Assets/grandchild.png");

        child.AddEntity(grandchild);
        root.AddEntity(child);
        scene.AddEntity(root);

        Assert.Equal(2, scene.GetEntitiesWith<Sprite>().Count());

        child.SetParent(null);

        Assert.Empty(scene.GetEntitiesWith<Sprite>());
        Assert.Empty(root.Children);
        Assert.Null(child.Parent);
        Assert.Null(child.CurrentScene);
        Assert.Null(grandchild.CurrentScene);
    }

    [Fact]
    public void SetParent_FromDetachedSubtree_ToSceneEntity_AttachesSubtreeAndRegistersCache()
    {
        var scene = new Scene { Name = "AttachDetachedSubtree" };
        var root = new GameEntity { Name = "Root" };
        scene.AddEntity(root);

        var child = CreateEntityWithSprite("Child", "Assets/child.png");
        var grandchild = CreateEntityWithSprite("Grandchild", "Assets/grandchild.png");
        child.AddEntity(grandchild);

        Assert.Null(child.CurrentScene);
        Assert.Null(grandchild.CurrentScene);
        Assert.Empty(scene.GetEntitiesWith<Sprite>());

        child.SetParent(root);

        var sprites = scene.GetEntitiesWith<Sprite>().ToList();
        Assert.Equal(2, sprites.Count);
        Assert.Contains(child, sprites);
        Assert.Contains(grandchild, sprites);
        Assert.Same(scene, child.CurrentScene);
        Assert.Same(scene, grandchild.CurrentScene);
        Assert.Same(root, child.Parent);
        Assert.Contains(child, root.Children);
    }

    private static GameEntity CreateEntityWithSprite(string name, string path)
    {
        var entity = new GameEntity { Name = name };
        entity.AddComponent(new Transform { Position = new(1f, 2f) });
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
