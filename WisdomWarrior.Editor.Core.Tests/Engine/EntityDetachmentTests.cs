using System.Drawing;
using System.Numerics;
using WisdomWarrior.Engine.Core;
using WisdomWarrior.Engine.Core.Assets;
using WisdomWarrior.Engine.Core.Components;
using WisdomWarrior.Engine.Core.Interfaces;
using EngineSize = WisdomWarrior.Engine.Core.DataTypes.Size;

namespace WisdomWarrior.Editor.Core.Tests.Engine;

public class EntityDetachmentTests
{
    [Fact]
    public void Scene_RemoveEntity_DetachesSubtreeAndClearsComponentCache()
    {
        var scene = new Scene { Name = "DetachRootScene" };
        var root = CreateEntityWithSprite("Root", "Assets/root.png");
        var child = CreateEntityWithSprite("Child", "Assets/child.png");
        root.AddEntity(child);

        scene.AddEntity(root);
        Assert.Equal(2, scene.GetEntitiesWith<Sprite>().Count());
        Assert.Same(scene, root.CurrentScene);
        Assert.Same(scene, child.CurrentScene);

        var removed = scene.RemoveEntity(root);

        Assert.True(removed);
        Assert.Empty(scene.Entities);
        Assert.Empty(scene.GetEntitiesWith<Sprite>());
        Assert.Null(root.CurrentScene);
        Assert.Null(child.CurrentScene);
        Assert.Null(root.Parent);
    }

    [Fact]
    public void GameEntity_RemoveEntity_DetachesChildSubtreeAndPreservesParentCacheEntries()
    {
        var scene = new Scene { Name = "DetachChildScene" };
        var root = CreateEntityWithSprite("Root", "Assets/root.png");
        var child = CreateEntityWithSprite("Child", "Assets/child.png");
        var grandchild = CreateEntityWithSprite("Grandchild", "Assets/grandchild.png");
        child.AddEntity(grandchild);
        root.AddEntity(child);
        scene.AddEntity(root);

        Assert.Equal(3, scene.GetEntitiesWith<Sprite>().Count());

        var removed = root.RemoveEntity(child);

        Assert.True(removed);
        var spritesAfterRemoval = scene.GetEntitiesWith<Sprite>().ToList();
        Assert.Single(spritesAfterRemoval);
        Assert.Contains(root, spritesAfterRemoval);
        Assert.DoesNotContain(child, spritesAfterRemoval);
        Assert.DoesNotContain(grandchild, spritesAfterRemoval);

        Assert.Null(child.Parent);
        Assert.Null(child.CurrentScene);
        Assert.Null(grandchild.CurrentScene);
        Assert.Empty(root.Children);
    }

    [Fact]
    public void RemovedEntities_AreNotRendered()
    {
        var scene = new Scene { Name = "RenderDetachScene" };
        var root = CreateEntityWithSprite("Root", "Assets/root.png");
        var child = CreateEntityWithSprite("Child", "Assets/child.png");
        root.AddEntity(child);
        scene.AddEntity(root);

        var renderService = new DrawCountingRenderService();
        scene.Draw(renderService);
        Assert.Equal(2, renderService.DrawCount);

        scene.RemoveEntity(root);
        scene.Draw(renderService);
        Assert.Equal(2, renderService.DrawCount);
    }

    [Fact]
    public void RemoveAndReAddEntity_KeepsCacheConsistent()
    {
        var scene = new Scene { Name = "ReAddScene" };
        var root = CreateEntityWithSprite("Root", "Assets/root.png");
        scene.AddEntity(root);

        Assert.Single(scene.GetEntitiesWith<Sprite>());

        scene.RemoveEntity(root);
        Assert.Empty(scene.GetEntitiesWith<Sprite>());
        Assert.Null(root.CurrentScene);

        scene.AddEntity(root);
        Assert.Single(scene.GetEntitiesWith<Sprite>());
        Assert.Same(scene, root.CurrentScene);
    }

    private static GameEntity CreateEntityWithSprite(string name, string path)
    {
        var entity = new GameEntity { Name = name };
        entity.AddComponent(new Transform { Position = new Vector2(4f, 8f) });
        entity.AddComponent(new Sprite
        {
            Image = new ImageAsset
            {
                AssetPath = path,
                Dimensions = new EngineSize(32, 32)
            }
        });
        return entity;
    }

    private sealed class DrawCountingRenderService : IRenderService
    {
        public int DrawCount { get; private set; }

        public void Begin()
        {
        }

        public void End()
        {
        }

        public void PreloadTextures(IEnumerable<string> texturePaths)
        {
        }

        public void Draw(string texturePath, Vector2 position, int width, int height, Color color, float rotation, Vector2 scale)
        {
            DrawCount++;
        }
    }
}
