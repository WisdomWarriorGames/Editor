using System.Drawing;
using System.Numerics;
using WisdomWarrior.Engine.Core;
using WisdomWarrior.Engine.Core.Assets;
using WisdomWarrior.Engine.Core.Components;
using WisdomWarrior.Engine.Core.Interfaces;
using WisdomWarrior.Engine.Core.Rendering;
using EngineSize = WisdomWarrior.Engine.Core.DataTypes.Size;

namespace WisdomWarrior.Editor.Core.Tests.Engine;

public class EngineTexturePreloadTests
{
    [Fact]
    public void LoadContent_PreloadsUniqueSpriteTextures_AndNestedEntitiesAreIncluded()
    {
        var sceneName = $"Scene_{Guid.NewGuid():N}";
        var root = new GameEntity { Name = "Root" };
        root.AddComponent(CreateSprite("Assets/hero.png"));
        root.AddComponent(CreateSprite("Assets/hero.png"));

        var child = new GameEntity { Name = "Child" };
        child.AddComponent(CreateSprite("Assets/weapon.png"));
        root.AddEntity(child);

        var scene = new Scene { Name = sceneName };
        scene.AddEntity(root);

        SceneManager.AddScene(sceneName, scene);
        SceneManager.SetCurrentScene(sceneName);

        var renderService = new FakeRenderService();
        var engine = new WisdomWarrior.Engine.Core.Engine(renderService);

        try
        {
            engine.LoadContent();
            engine.Draw();

            Assert.Single(renderService.PreloadBatches);
            Assert.Equal(2, renderService.PreloadBatches[0].Count);
            Assert.Contains("Assets/hero.png", renderService.PreloadBatches[0]);
            Assert.Contains("Assets/weapon.png", renderService.PreloadBatches[0]);

            var firstPreload = renderService.EventOrder.IndexOf("Preload");
            var firstBegin = renderService.EventOrder.IndexOf("Begin");
            Assert.True(firstPreload >= 0);
            Assert.True(firstBegin >= 0);
            Assert.True(firstPreload < firstBegin);
        }
        finally
        {
            SceneManager.RemoveScene(sceneName);
        }
    }

    [Fact]
    public void Draw_AfterRuntimeSpriteAddition_DoesNotForceAnotherPreloadBatch()
    {
        var sceneName = $"Scene_{Guid.NewGuid():N}";
        var root = new GameEntity { Name = "Root" };
        root.AddComponent(CreateSprite("Assets/initial.png"));

        var scene = new Scene { Name = sceneName };
        scene.AddEntity(root);

        SceneManager.AddScene(sceneName, scene);
        SceneManager.SetCurrentScene(sceneName);

        var renderService = new FakeRenderService();
        var engine = new WisdomWarrior.Engine.Core.Engine(renderService);

        try
        {
            engine.LoadContent();
            Assert.Single(renderService.PreloadBatches);

            var runtimeEntity = new GameEntity { Name = "Runtime" };
            runtimeEntity.AddComponent(CreateSprite("Assets/runtime-added.png"));
            scene.AddEntity(runtimeEntity);

            engine.Draw();
            engine.Draw();

            Assert.Single(renderService.PreloadBatches);
        }
        finally
        {
            SceneManager.RemoveScene(sceneName);
        }
    }

    [Fact]
    public void Draw_AfterSceneSwitch_PreloadsNewSceneTextures()
    {
        var sceneOneName = $"SceneA_{Guid.NewGuid():N}";
        var sceneTwoName = $"SceneB_{Guid.NewGuid():N}";

        var sceneOne = new Scene { Name = sceneOneName };
        sceneOne.AddEntity(CreateEntityWithSprite("One", "Assets/one.png"));

        var sceneTwo = new Scene { Name = sceneTwoName };
        sceneTwo.AddEntity(CreateEntityWithSprite("Two", "Assets/two.png"));

        SceneManager.AddScene(sceneOneName, sceneOne);
        SceneManager.AddScene(sceneTwoName, sceneTwo);
        SceneManager.SetCurrentScene(sceneOneName);

        var renderService = new FakeRenderService();
        var engine = new WisdomWarrior.Engine.Core.Engine(renderService);

        try
        {
            engine.LoadContent();
            Assert.Single(renderService.PreloadBatches);
            Assert.Contains("Assets/one.png", renderService.PreloadBatches[0]);

            SceneManager.SetCurrentScene(sceneTwoName);
            engine.Draw();

            Assert.Equal(2, renderService.PreloadBatches.Count);
            Assert.Contains("Assets/two.png", renderService.PreloadBatches[1]);
        }
        finally
        {
            SceneManager.RemoveScene(sceneOneName);
            SceneManager.RemoveScene(sceneTwoName);
        }
    }

    private static GameEntity CreateEntityWithSprite(string name, string path)
    {
        var entity = new GameEntity { Name = name };
        entity.AddComponent(CreateSprite(path));
        return entity;
    }

    private static Sprite CreateSprite(string path)
    {
        return new Sprite
        {
            Image = new ImageAsset
            {
                AssetPath = path,
                Dimensions = new EngineSize(32, 32)
            }
        };
    }

    private sealed class FakeRenderService : IRenderService
    {
        public List<HashSet<string>> PreloadBatches { get; } = [];
        public List<string> EventOrder { get; } = [];

        public void Begin(RenderBatchSettings settings)
        {
            EventOrder.Add("Begin");
        }

        public void End()
        {
            EventOrder.Add("End");
        }

        public void PreloadTextures(IEnumerable<string> texturePaths)
        {
            EventOrder.Add("Preload");
            var batch = new HashSet<string>(texturePaths.Where(p => !string.IsNullOrWhiteSpace(p)), StringComparer.OrdinalIgnoreCase);
            PreloadBatches.Add(batch);
        }

        public void Draw(string texturePath, Vector2 position, int width, int height, Color color, float rotation, Vector2 scale)
        {
            EventOrder.Add("Draw");
        }
    }
}
