using WisdomWarrior.Editor.Core.Services;
using WisdomWarrior.Editor.Core.Tests.TestInfrastructure;
using WisdomWarrior.Editor.FileSystem;
using WisdomWarrior.Engine.Core;

namespace WisdomWarrior.Editor.Core.Tests.Services;

public class ScenePersistenceServiceTests
{
    [Fact]
    public void SaveToPreferredDirectory_SavesSceneFileAndUpdatesManifest()
    {
        TestComponentRegistry.EnsureBootstrapped();

        var tempRoot = CreateTempRoot();
        FileSystemRegistry? registry = null;

        try
        {
            var projectFolder = Path.Combine(tempRoot, "Game");
            var levelsFolder = Path.Combine(projectFolder, "Levels");
            Directory.CreateDirectory(levelsFolder);
            File.WriteAllText(Path.Combine(projectFolder, "Game.csproj"), "<Project Sdk=\"Microsoft.NET.Sdk\" />");

            var manifestService = new EditorManifestService();
            registry = new FileSystemRegistry(new FileSystemService());
            var workspaceService = new WorkspaceService(registry, manifestService);
            workspaceService.Load(new WorkspaceDescriptor
            {
                RootPath = tempRoot,
                SolutionFilePath = Path.Combine(tempRoot, "Game.slnx"),
                DefaultProjectPath = "Game",
                ActiveScenePath = string.Empty
            });

            registry.SetCurrentNode(levelsFolder);

            var sceneManager = new CurrentSceneManager();
            sceneManager.CreateInMemoryScene("IntroScene");
            sceneManager.Tracker.AddEntity(new GameEntity());

            var persistenceService = new ScenePersistenceService(sceneManager, workspaceService, manifestService);

            Assert.True(persistenceService.HasUnsavedInMemoryDirtyScene);
            Assert.True(persistenceService.TrySaveSceneToPreferredDirectory());

            var expectedScenePath = Path.Combine(levelsFolder, "IntroScene.scene.json");
            Assert.True(File.Exists(expectedScenePath));
            Assert.True(sceneManager.HasPersistedScene);
            Assert.Equal(expectedScenePath, sceneManager.ActiveScenePath);
            Assert.False(persistenceService.HasUnsavedInMemoryDirtyScene);

            var manifest = manifestService.TryLoad(tempRoot);
            Assert.NotNull(manifest);
            Assert.Equal("Game", manifest!.DefaultProject);
            Assert.Equal("Game/Levels/IntroScene.scene.json", manifest.LastActiveScene);

            sceneManager.StopTicking();
            SceneManager.RemoveScene("IntroScene");
        }
        finally
        {
            registry?.Dispose();
            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, true);
            }
        }
    }

    [Fact]
    public void SaveToDirectory_RejectsInvalidTargetOutsideSolution()
    {
        TestComponentRegistry.EnsureBootstrapped();

        var tempRoot = CreateTempRoot();
        var outsideFolder = Path.Combine(Path.GetTempPath(), $"WW_Outside_{Guid.NewGuid():N}");
        FileSystemRegistry? registry = null;

        try
        {
            Directory.CreateDirectory(outsideFolder);

            var projectFolder = Path.Combine(tempRoot, "Game");
            Directory.CreateDirectory(projectFolder);
            File.WriteAllText(Path.Combine(projectFolder, "Game.csproj"), "<Project Sdk=\"Microsoft.NET.Sdk\" />");

            var manifestService = new EditorManifestService();
            registry = new FileSystemRegistry(new FileSystemService());
            var workspaceService = new WorkspaceService(registry, manifestService);
            workspaceService.Load(new WorkspaceDescriptor
            {
                RootPath = tempRoot,
                SolutionFilePath = Path.Combine(tempRoot, "Game.slnx"),
                DefaultProjectPath = "Game",
                ActiveScenePath = string.Empty
            });

            var sceneManager = new CurrentSceneManager();
            sceneManager.CreateInMemoryScene("UnsavedScene");
            sceneManager.Tracker.AddEntity(new GameEntity());

            var persistenceService = new ScenePersistenceService(sceneManager, workspaceService, manifestService);
            Assert.True(persistenceService.HasUnsavedInMemoryDirtyScene);
            Assert.False(persistenceService.TrySaveSceneToDirectory(outsideFolder));
            Assert.False(sceneManager.HasPersistedScene);

            sceneManager.StopTicking();
            SceneManager.RemoveScene("UnsavedScene");
        }
        finally
        {
            registry?.Dispose();
            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, true);
            }

            if (Directory.Exists(outsideFolder))
            {
                Directory.Delete(outsideFolder, true);
            }
        }
    }

    [Fact]
    public void AutoSavePersistedScene_WhenDirty_SavesWithoutPromptScenario()
    {
        TestComponentRegistry.EnsureBootstrapped();

        var tempRoot = CreateTempRoot();
        FileSystemRegistry? registry = null;

        try
        {
            var projectFolder = Path.Combine(tempRoot, "Game");
            var levelsFolder = Path.Combine(projectFolder, "Levels");
            Directory.CreateDirectory(levelsFolder);
            File.WriteAllText(Path.Combine(projectFolder, "Game.csproj"), "<Project Sdk=\"Microsoft.NET.Sdk\" />");

            var manifestService = new EditorManifestService();
            registry = new FileSystemRegistry(new FileSystemService());
            var workspaceService = new WorkspaceService(registry, manifestService);
            workspaceService.Load(new WorkspaceDescriptor
            {
                RootPath = tempRoot,
                SolutionFilePath = Path.Combine(tempRoot, "Game.slnx"),
                DefaultProjectPath = "Game",
                ActiveScenePath = string.Empty
            });

            registry.SetCurrentNode(levelsFolder);

            var sceneManager = new CurrentSceneManager();
            sceneManager.CreateInMemoryScene("AutoSaveScene");
            sceneManager.Tracker.AddEntity(new GameEntity());

            var persistenceService = new ScenePersistenceService(sceneManager, workspaceService, manifestService);
            Assert.True(persistenceService.TrySaveSceneToPreferredDirectory());

            sceneManager.Tracker.AddEntity(new GameEntity());
            Assert.True(persistenceService.HasDirtyPersistedScene);
            Assert.True(persistenceService.TryAutoSavePersistedScene());
            Assert.False(persistenceService.HasDirtyPersistedScene);
            Assert.False(persistenceService.HasUnsavedInMemoryDirtyScene);

            var expectedScenePath = Path.Combine(levelsFolder, "AutoSaveScene.scene.json");
            Assert.True(File.Exists(expectedScenePath));

            sceneManager.StopTicking();
            SceneManager.RemoveScene("AutoSaveScene");
        }
        finally
        {
            registry?.Dispose();
            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, true);
            }
        }
    }

    private static string CreateTempRoot()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "WisdomWarrior.Editor.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempRoot);
        return tempRoot;
    }
}
