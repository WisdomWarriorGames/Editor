using System.Numerics;
using System.Text.Json;
using WisdomWarrior.Editor.Core.Helpers;
using WisdomWarrior.Editor.Core.Services;
using WisdomWarrior.Editor.Core.Tests.TestInfrastructure;
using WisdomWarrior.Engine.Core;
using WisdomWarrior.Engine.Core.Assets;
using WisdomWarrior.Engine.Core.Components;

namespace WisdomWarrior.Editor.Core.Tests.Assets;

public class AssetPathPortabilityTests
{
    [Fact]
    public void CreateImageAsset_AbsolutePathInsideProject_StoresRelativePath()
    {
        var projectRoot = Path.Combine(Path.GetTempPath(), "WisdomWarrior.Editor.Tests", Guid.NewGuid().ToString("N"));
        var moduleRoot = Path.Combine(projectRoot, "GameAssets");
        var nestedDirectory = Path.Combine(moduleRoot, "Sprites", "Characters");
        var imagePath = Path.Combine(nestedDirectory, "hero.png");

        Directory.CreateDirectory(nestedDirectory);
        File.WriteAllBytes(imagePath, [1, 2, 3, 4]);

        AssetPathContext.Configure(projectRoot, moduleRoot);

        try
        {
            var asset = AssetHelpers.CreateImageAsset(imagePath);

            Assert.False(Path.IsPathRooted(asset.AssetPath));
            Assert.Equal("GameAssets/Sprites/Characters/hero.png", asset.AssetPath);
        }
        finally
        {
            if (Directory.Exists(projectRoot))
            {
                Directory.Delete(projectRoot, recursive: true);
            }
        }
    }

    [Fact]
    public void CreateImageAsset_AbsoluteExternalPath_ImportsIntoProjectAndStoresRelativePath()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "WisdomWarrior.Editor.Tests", Guid.NewGuid().ToString("N"));
        var projectRoot = Path.Combine(tempRoot, "Project");
        var moduleRoot = Path.Combine(projectRoot, "GameAssets");
        var externalRoot = Path.Combine(tempRoot, "External");
        var externalFile = Path.Combine(externalRoot, "enemy.png");

        Directory.CreateDirectory(moduleRoot);
        Directory.CreateDirectory(externalRoot);
        File.WriteAllBytes(externalFile, [10, 20, 30, 40]);

        AssetPathContext.Configure(projectRoot, moduleRoot);

        try
        {
            var asset = AssetHelpers.CreateImageAsset(externalFile);

            Assert.False(Path.IsPathRooted(asset.AssetPath));
            Assert.Contains("GameAssets/ImportedAssets/Images/", asset.AssetPath);

            var resolved = AssetHelpers.ResolveAbsoluteAssetPath(asset.AssetPath);
            Assert.True(File.Exists(resolved));
        }
        finally
        {
            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, recursive: true);
            }
        }
    }

    [Fact]
    public void ResolveAbsoluteAssetPath_RelativePath_ResolvesFromProjectRoot()
    {
        var projectRoot = Path.Combine(Path.GetTempPath(), "WisdomWarrior.Editor.Tests", Guid.NewGuid().ToString("N"));
        AssetPathContext.Configure(projectRoot, Path.Combine(projectRoot, "GameAssets"));

        try
        {
            var resolved = AssetHelpers.ResolveAbsoluteAssetPath("GameAssets/Textures/UI/icon.png");
            var expected = Path.GetFullPath(Path.Combine(projectRoot, "GameAssets", "Textures", "UI", "icon.png"));

            Assert.Equal(expected, resolved);
        }
        finally
        {
            if (Directory.Exists(projectRoot))
            {
                Directory.Delete(projectRoot, recursive: true);
            }
        }
    }

    [Fact]
    public void SaveScene_LegacyAbsoluteExternalPath_IsImportedAndSerializedAsRelative()
    {
        TestComponentRegistry.EnsureBootstrapped();

        var tempRoot = Path.Combine(Path.GetTempPath(), "WisdomWarrior.Editor.Tests", Guid.NewGuid().ToString("N"));
        var projectRoot = Path.Combine(tempRoot, "Project");
        var moduleRoot = Path.Combine(projectRoot, "GameAssets");
        var sceneRoot = Path.Combine(projectRoot, "GameProject");
        var scenePath = Path.Combine(sceneRoot, "CurrentScene.scene.json");
        var externalRoot = Path.Combine(tempRoot, "External");
        var externalFile = Path.Combine(externalRoot, "legacy.png");
        var sceneName = $"Scene_{Guid.NewGuid():N}";

        Directory.CreateDirectory(moduleRoot);
        Directory.CreateDirectory(sceneRoot);
        Directory.CreateDirectory(externalRoot);
        File.WriteAllBytes(externalFile, [99, 88, 77, 66]);

        AssetPathContext.Configure(projectRoot, moduleRoot);

        var scene = new Scene
        {
            Name = sceneName,
            Entities =
            [
                CreateSpriteEntity("Player", externalFile)
            ]
        };

        File.WriteAllText(scenePath, JsonSerializer.Serialize(scene, TestJsonOptions.Create(writeIndented: true)));

        var manager = new CurrentSceneManager();

        try
        {
            manager.LoadScene(scenePath);

            var saveSucceeded = manager.SaveScene();
            Assert.True(saveSucceeded);

            var savedJson = File.ReadAllText(scenePath);
            Assert.DoesNotContain(externalFile, savedJson, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("GameAssets/ImportedAssets/Images/", savedJson, StringComparison.OrdinalIgnoreCase);

            var loadedSprite = manager.ActiveScene!.Entities[0].Components.OfType<Sprite>().Single();
            Assert.False(Path.IsPathRooted(loadedSprite.Image.AssetPath));

            var resolvedImportedPath = AssetHelpers.ResolveAbsoluteAssetPath(loadedSprite.Image.AssetPath);
            Assert.True(File.Exists(resolvedImportedPath));
        }
        finally
        {
            manager.StopTicking();
            SceneManager.RemoveScene(sceneName);

            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, recursive: true);
            }
        }
    }

    [Fact]
    public void CreateImageAsset_MissingExternalPath_DoesNotCrashAndReturnsEmptyPath()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "WisdomWarrior.Editor.Tests", Guid.NewGuid().ToString("N"));
        var projectRoot = Path.Combine(tempRoot, "Project");
        var moduleRoot = Path.Combine(projectRoot, "GameAssets");
        var missingExternalPath = Path.Combine(tempRoot, "External", "missing.png");

        Directory.CreateDirectory(moduleRoot);
        AssetPathContext.Configure(projectRoot, moduleRoot);

        try
        {
            var asset = AssetHelpers.CreateImageAsset(missingExternalPath);
            Assert.Equal(string.Empty, asset.AssetPath);
        }
        finally
        {
            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, recursive: true);
            }
        }
    }

    private static GameEntity CreateSpriteEntity(string name, string assetPath)
    {
        var entity = new GameEntity { Name = name };
        entity.AddComponent(new Transform { Position = new Vector2(10f, 20f) });
        entity.AddComponent(new Sprite
        {
            Image = new ImageAsset { AssetPath = assetPath }
        });
        return entity;
    }
}
