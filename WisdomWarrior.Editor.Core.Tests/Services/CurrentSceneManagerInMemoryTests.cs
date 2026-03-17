using WisdomWarrior.Editor.Core.Services;
using WisdomWarrior.Editor.Core.Tests.TestInfrastructure;
using WisdomWarrior.Engine.Core;

namespace WisdomWarrior.Editor.Core.Tests.Services;

public class CurrentSceneManagerInMemoryTests
{
    [Fact]
    public void CreateInMemoryScene_CreatesActiveSceneWithoutPersistedPath()
    {
        TestComponentRegistry.EnsureBootstrapped();

        var manager = new CurrentSceneManager();

        try
        {
            manager.CreateInMemoryScene("UnsavedScene");

            Assert.NotNull(manager.ActiveScene);
            Assert.Equal("UnsavedScene", manager.ActiveScene!.Name);
            Assert.Same(manager.ActiveScene, SceneManager.GetCurrentScene());
            Assert.False(manager.SaveScene());
        }
        finally
        {
            manager.StopTicking();
            SceneManager.RemoveScene("UnsavedScene");
        }
    }
}
