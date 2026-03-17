using WisdomWarrior.Engine.Core;

namespace WisdomWarrior.Editor.Core.Tests.Engine;

public class SceneManagerReloadTests
{
    [Fact]
    public void AddOrReplaceScene_ReplacesExistingSceneEntrySafely()
    {
        var sceneName = $"Scene_{Guid.NewGuid():N}";
        var initial = new Scene { Name = sceneName };
        var replacement = new Scene { Name = sceneName };

        try
        {
            SceneManager.AddScene(sceneName, initial);
            SceneManager.SetCurrentScene(sceneName);
            Assert.Same(initial, SceneManager.GetCurrentScene());

            SceneManager.AddOrReplaceScene(sceneName, replacement);
            SceneManager.SetCurrentScene(sceneName);

            Assert.Same(replacement, SceneManager.GetCurrentScene());
        }
        finally
        {
            SceneManager.RemoveScene(sceneName);
        }
    }

    [Fact]
    public void RemoveScene_RemovingCurrentScene_ClearsCurrentReference()
    {
        var sceneName = $"Scene_{Guid.NewGuid():N}";
        var scene = new Scene { Name = sceneName };

        SceneManager.AddScene(sceneName, scene);
        SceneManager.SetCurrentScene(sceneName);
        Assert.Same(scene, SceneManager.GetCurrentScene());

        SceneManager.RemoveScene(sceneName);

        Assert.Null(SceneManager.GetCurrentScene());
    }
}
