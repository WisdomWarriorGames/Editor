using System.Drawing;
using System.Numerics;
using System.Text.Json;
using WisdomWarrior.Editor.Core.Tests.TestInfrastructure;
using WisdomWarrior.Engine.Core;
using WisdomWarrior.Engine.Core.Assets;
using WisdomWarrior.Engine.Core.Components;
using WisdomWarrior.Engine.Core.Rendering;
using WisdomWarrior.Engine.Core.Systems;
using EngineSize = WisdomWarrior.Engine.Core.DataTypes.Size;

namespace WisdomWarrior.Editor.Core.Tests.Serialization;

public class SceneSerializationTests
{
    public SceneSerializationTests()
    {
        TestComponentRegistry.EnsureBootstrapped();
    }

    [Fact]
    public void Scene_RoundTrip_PreservesHierarchyAndComponentData()
    {
        var scene = new Scene { Name = "SceneAlpha" };

        var root = new GameEntity { Name = "Player" };
        root.AddComponent(new Transform
        {
            Position = new Vector2(10f, 20f),
            Scale = new Vector2(2f, 3f),
            Rotation = 45f
        });
        root.AddComponent(new Sprite
        {
            Colour = Color.FromArgb(128, 10, 20, 30),
            Image = new ImageAsset
            {
                AssetPath = "Assets/player.png",
                Dimensions = new EngineSize(64, 32)
            }
        });

        var child = new GameEntity { Name = "Weapon" };
        child.AddComponent(new Transform { Position = new Vector2(1f, 2f) });
        root.AddEntity(child);

        scene.AddEntity(root);
        scene.AddSystem(new SpriteRenderSystem());

        var options = TestJsonOptions.Create(writeIndented: true);
        var json = JsonSerializer.Serialize(scene, options);

        Assert.Contains("\"$type\"", json);
        Assert.Contains("\"Transform\"", json);
        Assert.Contains("\"Sprite\"", json);
        Assert.Contains("\"SpriteRenderSystem\"", json);
        Assert.DoesNotContain("\"RenderSystems\"", json);
        Assert.DoesNotContain("\"BehaviourSystems\"", json);
        Assert.Contains("#800A141E", json);

        var deserialized = JsonSerializer.Deserialize<Scene>(json, options);

        Assert.NotNull(deserialized);
        deserialized!.Initialize();

        Assert.Equal("SceneAlpha", deserialized.Name);
        Assert.Single(deserialized.Entities);
        Assert.Single(deserialized.Systems);
        Assert.Single(deserialized.RenderSystems);
        Assert.IsType<SpriteRenderSystem>(deserialized.Systems[0]);
        Assert.Same(deserialized, deserialized.Systems[0].Scene);

        var loadedRoot = deserialized.Entities[0];
        Assert.Equal("Player", loadedRoot.Name);
        Assert.Single(loadedRoot.Children);

        var loadedChild = loadedRoot.Children[0];
        Assert.Same(loadedRoot, loadedChild.Parent);

        var loadedTransform = loadedRoot.Components.OfType<Transform>().Single();
        Assert.Equal(new Vector2(10f, 20f), loadedTransform.Position);
        Assert.Equal(new Vector2(2f, 3f), loadedTransform.Scale);
        Assert.Equal(45f, loadedTransform.Rotation);

        var loadedSprite = loadedRoot.Components.OfType<Sprite>().Single();
        Assert.Equal(Color.FromArgb(128, 10, 20, 30), loadedSprite.Colour);
        Assert.Equal(new EngineSize(64, 32), loadedSprite.Size);
    }

    [Fact]
    public void Scene_RoundTrip_PreservesRenderBatchSettings()
    {
        var scene = new Scene { Name = "RenderSettingsScene" };
        var system = new SpriteRenderSystem
        {
            BatchSettings = new RenderBatchSettings
            {
                SortMode = RenderBatchSortMode.FrontToBack,
                BlendMode = RenderBlendMode.NonPremultiplied,
                SamplerMode = RenderSamplerMode.AnisotropicWrap,
                DepthStencilMode = RenderDepthStencilMode.Default,
                RasterizerMode = RenderRasterizerMode.WireFrame,
                UseTransformMatrix = true,
                TransformMatrix = Matrix4x4.CreateScale(2f) * Matrix4x4.CreateTranslation(4f, 5f, 6f)
            }
        };

        scene.AddSystem(system);

        var options = TestJsonOptions.Create(writeIndented: true);
        var json = JsonSerializer.Serialize(scene, options);
        var deserialized = JsonSerializer.Deserialize<Scene>(json, options);

        Assert.NotNull(deserialized);
        deserialized!.Initialize();

        var loadedSystem = Assert.IsType<SpriteRenderSystem>(Assert.Single(deserialized.Systems));
        Assert.Equal(system.BatchSettings, loadedSystem.BatchSettings);
    }

    [Fact]
    public void Scene_Deserialize_WithUnknownComponentType_ThrowsJsonException()
    {
        var json = """
                   {
                     "Name": "BrokenScene",
                     "Entities": [
                       {
                         "Name": "Entity1",
                         "Components": [
                           {
                             "$type": "MissingComponent"
                           }
                         ],
                         "Children": []
                       }
                     ]
                   }
                   """;

        var options = TestJsonOptions.Create();

        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Scene>(json, options));
    }

    [Fact]
    public void Scene_Deserialize_WithUnknownSystemType_ThrowsJsonException()
    {
        var json = """
                   {
                     "Name": "BrokenScene",
                     "Systems": [
                       {
                         "$type": "MissingSystem"
                       }
                     ],
                     "Entities": []
                   }
                   """;

        var options = TestJsonOptions.Create();

        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<Scene>(json, options));
    }

    [Fact]
    public void Scene_Initialize_AfterDeserialization_SetsChildParentLinks()
    {
        var scene = new Scene { Name = "ParentingScene" };
        var root = new GameEntity { Name = "Root" };
        var child = new GameEntity { Name = "Child" };

        root.AddEntity(child);
        scene.AddEntity(root);

        var options = TestJsonOptions.Create();
        var json = JsonSerializer.Serialize(scene, options);
        var deserialized = JsonSerializer.Deserialize<Scene>(json, options);

        Assert.NotNull(deserialized);

        deserialized!.Initialize();
        var loadedRoot = deserialized.Entities[0];
        var loadedChild = loadedRoot.Children[0];

        Assert.Same(loadedRoot, loadedChild.Parent);
    }
}
