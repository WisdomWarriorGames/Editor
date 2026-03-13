using WisdomWarrior.Editor.Core.Services;
using WisdomWarrior.Editor.Core.Tests.TestInfrastructure;
using WisdomWarrior.Editor.FileSystem;
using WisdomWarrior.Editor.SceneList.ViewModels;
using WisdomWarrior.Engine.Core;

namespace WisdomWarrior.Editor.Core.Tests.SceneList;

public class SceneHierarchyViewModelTests
{
    [Fact]
    public void CreateInMemoryScene_WhenCalledMultipleTimes_OnlyKeepsSingleSceneNode()
    {
        TestComponentRegistry.EnsureBootstrapped();

        var currentSceneManager = new CurrentSceneManager();
        var selectionManager = new SelectionManager();
        var fileSystemRegistry = new FileSystemRegistry(new FileSystemService());
        var manifestService = new EditorManifestService();
        var workspaceService = new WorkspaceService(fileSystemRegistry, manifestService);
        var scenePersistenceService = new ScenePersistenceService(currentSceneManager, workspaceService, manifestService);
        var fileSystemService = new FileSystemService();
        var scenePathSynchronizationService = new ScenePathSynchronizationService(currentSceneManager, workspaceService, manifestService, fileSystemService);
        var sceneHierarchyViewModel = new SceneHierarchyViewModel(
            currentSceneManager,
            selectionManager,
            scenePersistenceService,
            scenePathSynchronizationService);

        try
        {
            currentSceneManager.CreateInMemoryScene("Scene1");
            currentSceneManager.CreateInMemoryScene("Scene1");

            SpinWait.SpinUntil(() => sceneHierarchyViewModel.Scenes.Count == 1, TimeSpan.FromSeconds(1));

            Assert.Single(sceneHierarchyViewModel.Scenes);
            Assert.Equal("Scene1", sceneHierarchyViewModel.Scenes[0].Name);
        }
        finally
        {
            currentSceneManager.StopTicking();
            fileSystemRegistry.Dispose();
            SceneManager.RemoveScene("Scene1");
        }
    }
}
