using System.Drawing;
using System.Numerics;
using WisdomWarrior.Engine.Core;
using WisdomWarrior.Engine.Core.Assets;
using WisdomWarrior.Engine.Core.Components;
using WisdomWarrior.Engine.Core.Interfaces;
using WisdomWarrior.Engine.Core.Systems;
using EngineSize = WisdomWarrior.Engine.Core.DataTypes.Size;

namespace WisdomWarrior.Editor.Core.Tests.Engine;

public class SceneSystemRuntimeTests
{
    [Fact]
    public void SystemsCollection_AddAndRemove_KeepsTypedCachesAndOwnershipInSync()
    {
        var scene = new Scene { Name = "SystemScene" };
        var renderSystem = new CountingRenderSystem();
        var behaviourSystem = new CountingBehaviourSystem();

        scene.Systems.Add(renderSystem);
        scene.Systems.Add(behaviourSystem);

        Assert.Equal(2, scene.Systems.Count);
        Assert.Single(scene.RenderSystems);
        Assert.Single(scene.BehaviourSystems);
        Assert.Same(scene, renderSystem.Scene);
        Assert.Same(scene, behaviourSystem.Scene);

        var removed = scene.RemoveSystem(renderSystem);

        Assert.True(removed);
        Assert.Single(scene.Systems);
        Assert.Empty(scene.RenderSystems);
        Assert.Single(scene.BehaviourSystems);
        Assert.Null(renderSystem.Scene);
    }

    [Fact]
    public void SceneLifecycle_UsesTypedSystemCachesDirectly()
    {
        var scene = new Scene { Name = "LifecycleScene" };
        var renderSystem = new CountingRenderSystem();
        var behaviourSystem = new CountingBehaviourSystem();
        var renderService = new SpyRenderService();

        scene.AddSystem(renderSystem);
        scene.AddSystem(behaviourSystem);

        scene.OnStart();
        scene.Update();
        scene.Draw(renderService);

        Assert.Equal(1, renderSystem.LoadCount);
        Assert.Equal(1, renderSystem.RenderCount);
        Assert.Equal(1, behaviourSystem.StartCount);
        Assert.Equal(1, behaviourSystem.UpdateCount);
    }

    [Fact]
    public void AddSystem_AfterSceneStart_InitializesImmediately()
    {
        var scene = new Scene { Name = "LiveScene" };
        scene.OnStart();

        var renderSystem = new CountingRenderSystem();
        var behaviourSystem = new CountingBehaviourSystem();

        scene.AddSystem(renderSystem);
        scene.AddSystem(behaviourSystem);

        Assert.Equal(1, renderSystem.LoadCount);
        Assert.Equal(1, behaviourSystem.StartCount);
    }

    [Fact]
    public void SpriteRenderSystem_RendersSpriteEntitiesUsingSceneCache()
    {
        var scene = new Scene { Name = "SpriteScene" };
        scene.AddSystem(new SpriteRenderSystem());

        var entity = new GameEntity { Name = "Hero" };
        entity.AddComponent(new Transform
        {
            Position = new Vector2(10f, 20f),
            Rotation = 45f,
            Scale = new Vector2(2f, 3f)
        });
        entity.AddComponent(new Sprite
        {
            Colour = Color.Red,
            Image = new ImageAsset
            {
                AssetPath = "Assets/hero.png",
                Dimensions = new EngineSize(32, 16)
            }
        });

        scene.AddEntity(entity);

        var renderService = new SpyRenderService();
        scene.Draw(renderService);

        Assert.Single(renderService.Draws);
        var draw = renderService.Draws[0];
        Assert.Equal("Assets/hero.png", draw.TexturePath);
        Assert.Equal(new Vector2(10f, 20f), draw.Position);
        Assert.Equal(32, draw.Width);
        Assert.Equal(16, draw.Height);
        Assert.Equal(Color.Red, draw.Colour);
        Assert.Equal(45f, draw.Rotation);
        Assert.Equal(new Vector2(2f, 3f), draw.Scale);
    }

    private sealed class CountingRenderSystem : RenderSystem
    {
        public int LoadCount { get; private set; }
        public int RenderCount { get; private set; }

        public override void LoadContent()
        {
            LoadCount++;
        }

        public override void Render(IRenderService renderService)
        {
            RenderCount++;
        }
    }

    private sealed class CountingBehaviourSystem : BehaviourSystem
    {
        public int StartCount { get; private set; }
        public int UpdateCount { get; private set; }
        public float TickScale { get; set; } = 1f;

        public override void OnStart()
        {
            StartCount++;
        }

        public override void Update()
        {
            UpdateCount++;
        }
    }

    private sealed class SpyRenderService : IRenderService
    {
        public List<DrawCall> Draws { get; } = [];

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
            Draws.Add(new DrawCall(texturePath, position, width, height, color, rotation, scale));
        }
    }

    private sealed record DrawCall(
        string TexturePath,
        Vector2 Position,
        int Width,
        int Height,
        Color Colour,
        float Rotation,
        Vector2 Scale);
}
