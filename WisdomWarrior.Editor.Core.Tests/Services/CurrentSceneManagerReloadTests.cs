using System.Numerics;
using System.Text.Json;
using WisdomWarrior.Editor.Core.Services;
using WisdomWarrior.Editor.Core.Tests.TestInfrastructure;
using WisdomWarrior.Engine.Core;
using WisdomWarrior.Engine.Core.Components;

namespace WisdomWarrior.Editor.Core.Tests.Services;

public class CurrentSceneManagerReloadTests
{
    [Fact]
    public void LoadScene_ReloadSameSceneName_ReplacesSceneAndRebuildsTracker()
    {
        TestComponentRegistry.EnsureBootstrapped();

        var sceneName = $"ReloadScene_{Guid.NewGuid():N}";
        var tempDir = Path.Combine(Path.GetTempPath(), "WisdomWarrior.Editor.Tests", Guid.NewGuid().ToString("N"));
        var scenePath = Path.Combine(tempDir, "CurrentScene.scene.json");

        Directory.CreateDirectory(tempDir);

        var manager = new CurrentSceneManager();

        try
        {
            WriteScene(scenePath, CreateScene(sceneName, 1));

            var firstLoadError = Record.Exception(() => manager.LoadScene(scenePath));
            Assert.Null(firstLoadError);

            var firstScene = manager.ActiveScene;
            Assert.NotNull(firstScene);
            Assert.Single(manager.Tracker.TrackedRoots);

            WriteScene(scenePath, CreateScene(sceneName, 2));

            var secondLoadError = Record.Exception(() => manager.LoadScene(scenePath));
            Assert.Null(secondLoadError);

            var reloadedScene = manager.ActiveScene;
            Assert.NotNull(reloadedScene);
            Assert.NotSame(firstScene, reloadedScene);
            Assert.Same(reloadedScene, SceneManager.GetCurrentScene());
            Assert.Equal(2, manager.Tracker.TrackedRoots.Count);
            Assert.False(manager.Tracker.IsDirty);

            for (var i = 0; i < 5; i++)
            {
                var entityCount = (i % 3) + 1;
                WriteScene(scenePath, CreateScene(sceneName, entityCount));
                var reloadError = Record.Exception(() => manager.LoadScene(scenePath));
                Assert.Null(reloadError);
                Assert.Equal(entityCount, manager.Tracker.TrackedRoots.Count);
                Assert.Same(manager.ActiveScene, SceneManager.GetCurrentScene());
            }
        }
        finally
        {
            manager.StopTicking();
            SceneManager.RemoveScene(sceneName);

            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }

    [Fact]
    public void LoadScene_WhenSceneNameChanges_RemovesPreviousSceneRegistration()
    {
        TestComponentRegistry.EnsureBootstrapped();

        var originalSceneName = $"Original_{Guid.NewGuid():N}";
        var renamedSceneName = $"Renamed_{Guid.NewGuid():N}";
        var tempDir = Path.Combine(Path.GetTempPath(), "WisdomWarrior.Editor.Tests", Guid.NewGuid().ToString("N"));
        var scenePath = Path.Combine(tempDir, "CurrentScene.scene.json");
        Directory.CreateDirectory(tempDir);

        var manager = new CurrentSceneManager();

        try
        {
            WriteScene(scenePath, CreateScene(originalSceneName, 1));
            manager.LoadScene(scenePath);
            Assert.Equal(originalSceneName, SceneManager.GetCurrentScene()!.Name);

            WriteScene(scenePath, CreateScene(renamedSceneName, 1));
            manager.LoadScene(scenePath);
            Assert.Equal(renamedSceneName, SceneManager.GetCurrentScene()!.Name);

            Assert.Throws<Exception>(() => SceneManager.SetCurrentScene(originalSceneName));
        }
        finally
        {
            manager.StopTicking();
            SceneManager.RemoveScene(originalSceneName);
            SceneManager.RemoveScene(renamedSceneName);

            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, recursive: true);
            }
        }
    }

    private static Scene CreateScene(string sceneName, int rootCount)
    {
        var scene = new Scene { Name = sceneName };

        for (var i = 0; i < rootCount; i++)
        {
            var entity = new GameEntity { Name = $"Entity_{i}" };
            entity.AddComponent(new Transform { Position = new Vector2(i, i + 1) });
            scene.AddEntity(entity);
        }

        return scene;
    }

    private static void WriteScene(string path, Scene scene)
    {
        var json = JsonSerializer.Serialize(scene, TestJsonOptions.Create(writeIndented: true));
        File.WriteAllText(path, json);
    }
}
