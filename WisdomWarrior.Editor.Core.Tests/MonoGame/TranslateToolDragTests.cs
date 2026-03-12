using System.Numerics;
using WisdomWarrior.Editor.Core.Services;
using WisdomWarrior.Editor.MonoGame.Tools;
using WisdomWarrior.Engine.Core;
using WisdomWarrior.Engine.Core.Assets;
using WisdomWarrior.Engine.Core.Components;
using EngineSize = WisdomWarrior.Engine.Core.DataTypes.Size;

namespace WisdomWarrior.Editor.Core.Tests.MonoGame;

public class TranslateToolDragTests
{
    [Fact]
    public void Update_SelectedSpriteEntity_StartsDraggingFromSpriteBodyOutsideGizmo()
    {
        var input = new EditorInputService();
        var context = new ToolContext(input);
        var tool = new TranslateTool();

        var entity = CreateSpriteEntity("SpriteEntity", new Vector2(100f, 100f), 120, 80);
        var transform = entity.GetComponent<Transform>()!;
        context.SetSelectedEntity(entity);

        input.MousePosition = new Vector2(150f, 100f); // inside sprite, outside gizmo radius
        input.SetLeftMouseDown(true);
        tool.Update(context);
        input.AdvanceFrame();

        input.MousePosition = new Vector2(170f, 100f);
        tool.Update(context);

        Assert.Equal(new Vector2(120f, 100f), transform.Position);
    }

    [Fact]
    public void Update_SelectedEntityWithoutSprite_DragsFromPointGizmo()
    {
        var input = new EditorInputService();
        var context = new ToolContext(input);
        var tool = new TranslateTool();

        var entity = CreateTransformOnlyEntity("TransformOnly", new Vector2(50f, 50f));
        var transform = entity.GetComponent<Transform>()!;
        context.SetSelectedEntity(entity);

        input.MousePosition = new Vector2(50f, 50f); // directly on gizmo
        input.SetLeftMouseDown(true);
        tool.Update(context);
        input.AdvanceFrame();

        input.MousePosition = new Vector2(75f, 80f);
        tool.Update(context);

        Assert.Equal(new Vector2(75f, 80f), transform.Position);
    }

    [Fact]
    public void Update_SelectedEntityWithoutSprite_ClickAwayFromGizmo_DoesNotDrag()
    {
        var input = new EditorInputService();
        var context = new ToolContext(input);
        var tool = new TranslateTool();

        var entity = CreateTransformOnlyEntity("TransformOnly", new Vector2(10f, 10f));
        var transform = entity.GetComponent<Transform>()!;
        context.SetSelectedEntity(entity);

        input.MousePosition = new Vector2(200f, 200f); // far from gizmo
        input.SetLeftMouseDown(true);
        tool.Update(context);
        input.AdvanceFrame();

        input.MousePosition = new Vector2(220f, 230f);
        tool.Update(context);

        Assert.Equal(new Vector2(10f, 10f), transform.Position);
    }

    private static GameEntity CreateTransformOnlyEntity(string name, Vector2 position)
    {
        var entity = new GameEntity { Name = name };
        entity.AddComponent(new Transform { Position = position, Scale = Vector2.One });
        return entity;
    }

    private static GameEntity CreateSpriteEntity(string name, Vector2 position, int width, int height)
    {
        var entity = CreateTransformOnlyEntity(name, position);
        entity.AddComponent(new Sprite
        {
            Image = new ImageAsset
            {
                AssetPath = $"Assets/{name}.png",
                Dimensions = new EngineSize(width, height)
            }
        });
        return entity;
    }
}
