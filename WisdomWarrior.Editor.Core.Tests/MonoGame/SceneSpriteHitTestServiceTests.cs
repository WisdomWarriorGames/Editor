using System.Numerics;
using WisdomWarrior.Editor.MonoGame.Selection;
using WisdomWarrior.Engine.Core;
using WisdomWarrior.Engine.Core.Assets;
using WisdomWarrior.Engine.Core.Components;
using EngineSize = WisdomWarrior.Engine.Core.DataTypes.Size;

namespace WisdomWarrior.Editor.Core.Tests.MonoGame;

public class SceneSpriteHitTestServiceTests
{
    private readonly SceneSpriteHitTestService _hitTestService = new();

    [Fact]
    public void HitTest_ClickInsideVisibleSprite_ReturnsOwningEntity()
    {
        var scene = new Scene { Name = "PickScene" };
        var entity = CreateSpriteEntity("Entity", "Assets/entity.png", new Vector2(100, 100), 50, 40);
        scene.AddEntity(entity);

        var picked = _hitTestService.HitTest(scene, new Vector2(100, 100), _ => true);

        Assert.Same(entity, picked);
    }

    [Fact]
    public void HitTest_ClickInsideNestedEntitySprite_ReturnsNestedOwner()
    {
        var scene = new Scene { Name = "NestedPickScene" };
        var root = new GameEntity { Name = "Root" };
        var child = CreateSpriteEntity("Child", "Assets/child.png", new Vector2(200, 200), 30, 30);
        root.AddEntity(child);
        scene.AddEntity(root);

        var picked = _hitTestService.HitTest(scene, new Vector2(200, 200), _ => true);

        Assert.Same(child, picked);
    }

    [Fact]
    public void HitTest_ClickInEmptySpace_ReturnsNull()
    {
        var scene = new Scene { Name = "EmptyPickScene" };
        var entity = CreateSpriteEntity("Entity", "Assets/entity.png", new Vector2(50, 50), 20, 20);
        scene.AddEntity(entity);

        var picked = _hitTestService.HitTest(scene, new Vector2(500, 500), _ => true);

        Assert.Null(picked);
    }

    [Fact]
    public void HitTest_OverlappingSprites_ReturnsLastRenderableHitInRenderIteration()
    {
        var scene = new Scene { Name = "OverlapPickScene" };
        var entityA = CreateSpriteEntity("A", "Assets/a.png", new Vector2(120, 120), 64, 64);
        var entityB = CreateSpriteEntity("B", "Assets/b.png", new Vector2(120, 120), 64, 64);
        scene.AddEntity(entityA);
        scene.AddEntity(entityB);

        var expected = scene.GetEntitiesWith<Sprite>()
            .Where(entity => entity.GetComponent<Sprite>() != null && entity.GetComponent<Transform>() != null)
            .Last();

        var picked = _hitTestService.HitTest(scene, new Vector2(120, 120), _ => true);

        Assert.Same(expected, picked);
    }

    [Fact]
    public void HitTest_NonRenderableTexture_IsIgnored()
    {
        var scene = new Scene { Name = "RenderableFilterScene" };
        var visible = CreateSpriteEntity("Visible", "Assets/visible.png", new Vector2(80, 80), 40, 40);
        var missing = CreateSpriteEntity("Missing", "Assets/missing.png", new Vector2(80, 80), 40, 40);
        scene.AddEntity(visible);
        scene.AddEntity(missing);

        var picked = _hitTestService.HitTest(scene, new Vector2(80, 80), path => path.Contains("visible", StringComparison.OrdinalIgnoreCase));

        Assert.Same(visible, picked);
    }

    private static GameEntity CreateSpriteEntity(string name, string path, Vector2 position, int width, int height)
    {
        var entity = new GameEntity { Name = name };
        entity.AddComponent(new Transform
        {
            Position = position,
            Scale = Vector2.One
        });
        entity.AddComponent(new Sprite
        {
            Image = new ImageAsset
            {
                AssetPath = path,
                Dimensions = new EngineSize(width, height)
            }
        });
        return entity;
    }
}
