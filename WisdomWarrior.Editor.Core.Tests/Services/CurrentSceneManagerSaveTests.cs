using System.Numerics;
using System.Text.Json;
using WisdomWarrior.Editor.Core.Services;
using WisdomWarrior.Editor.Core.Tests.TestInfrastructure;
using WisdomWarrior.Engine.Core;
using WisdomWarrior.Engine.Core.Components;

namespace WisdomWarrior.Editor.Core.Tests.Services;

public class CurrentSceneManagerSaveTests
{
    [Fact]
    public void SaveScene_OnSuccess_ResetsSceneAndPropertyDirtyState()
    {
        TestComponentRegistry.EnsureBootstrapped();

        var sceneName = $"Scene_{Guid.NewGuid():N}";
        var tempDir = Path.Combine(Path.GetTempPath(), "WisdomWarrior.Editor.Tests", Guid.NewGuid().ToString("N"));
        var scenePath = Path.Combine(tempDir, "CurrentScene.scene.json");

        Directory.CreateDirectory(tempDir);

        var scene = new Scene
        {
            Name = sceneName,
            Entities = [TestSceneFactory.CreateEntityWithTransform("Root", new Vector2(1f, 2f))]
        };

        var json = JsonSerializer.Serialize(scene, TestJsonOptions.Create(writeIndented: true));
        File.WriteAllText(scenePath, json);

        var manager = new CurrentSceneManager();

        try
        {
            manager.LoadScene(scenePath);

            var transform = manager.ActiveScene.Entities[0].Components.OfType<Transform>().Single();
            transform.Position = new Vector2(5f, 6f);

            manager.Tracker.Update();
            Assert.True(manager.Tracker.IsDirty);

            var trackedPosition = manager.Tracker.TrackedRoots[0]
                .TrackedComponents.Single(c => c.EngineComponent is Transform)
                .Properties.Single(p => p.Name == nameof(Transform.Position));

            Assert.True(trackedPosition.IsDirty);

            var saveSucceeded = manager.SaveScene();
            Assert.True(saveSucceeded);
            Assert.False(manager.Tracker.IsDirty);
            Assert.False(trackedPosition.IsDirty);

            manager.Tracker.Update();
            Assert.False(manager.Tracker.IsDirty);
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
}
