using System.Text.Json;
using WisdomWarrior.Editor.Core.Services;
using WisdomWarrior.Editor.Core.Tests.TestInfrastructure;
using WisdomWarrior.Editor.FileSystem;
using WisdomWarrior.Engine.Core;

namespace WisdomWarrior.Editor.Core.Tests.Services;

public class ScenePathSynchronizationServiceTests
{
    [Fact]
    public void RenameActiveScene_WhenPersisted_RenamesFileAndUpdatesManifest()
    {
        TestComponentRegistry.EnsureBootstrapped();

        var tempRoot = CreateTempRoot();
        FileSystemRegistry? registry = null;
        CurrentSceneManager? sceneManager = null;

        try
        {
            var context = CreateWorkspaceContext(tempRoot, out registry, out sceneManager);
            context.ScenePersistenceService.TrySaveSceneToPreferredDirectory();

            var result = context.ScenePathSynchronizationService.RenameActiveScene("Boss Intro");

            var expectedPath = Path.Combine(context.LevelsFolder, "Boss Intro.scene.json");

            Assert.True(result.Success, result.ErrorMessage);
            Assert.Equal("Boss Intro", sceneManager!.ActiveScene!.Name);
            Assert.Equal(expectedPath, sceneManager.ActiveScenePath);
            Assert.True(File.Exists(expectedPath));
            Assert.False(File.Exists(Path.Combine(context.LevelsFolder, "IntroScene.scene.json")));

            var manifest = context.ManifestService.TryLoad(tempRoot);
            Assert.NotNull(manifest);
            Assert.Equal("Game/Levels/Boss Intro.scene.json", manifest!.LastActiveScene);

            var json = File.ReadAllText(expectedPath);
            var scene = JsonSerializer.Deserialize<Scene>(json, TestJsonOptions.Create());
            Assert.NotNull(scene);
            Assert.Equal("Boss Intro", scene!.Name);
        }
        finally
        {
            sceneManager?.StopTicking();
            registry?.Dispose();
            SceneManager.RemoveScene("IntroScene");
            SceneManager.RemoveScene("Boss Intro");

            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, true);
            }
        }
    }

    [Fact]
    public void RenameActiveScene_WhenUnsaved_OnlyRenamesInMemoryScene()
    {
        TestComponentRegistry.EnsureBootstrapped();

        var tempRoot = CreateTempRoot();
        FileSystemRegistry? registry = null;
        CurrentSceneManager? sceneManager = null;

        try
        {
            var context = CreateWorkspaceContext(tempRoot, out registry, out sceneManager, seedManifest: true);

            var result = context.ScenePathSynchronizationService.RenameActiveScene("Unsaved Renamed");

            Assert.True(result.Success, result.ErrorMessage);
            Assert.Equal("Unsaved Renamed", sceneManager!.ActiveScene!.Name);
            Assert.False(sceneManager.HasPersistedScene);
            Assert.Equal(string.Empty, sceneManager.ActiveScenePath);

            var manifest = context.ManifestService.TryLoad(tempRoot);
            Assert.NotNull(manifest);
            Assert.Null(manifest!.LastActiveScene);
        }
        finally
        {
            sceneManager?.StopTicking();
            registry?.Dispose();
            SceneManager.RemoveScene("IntroScene");
            SceneManager.RemoveScene("Unsaved Renamed");

            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, true);
            }
        }
    }

    [Fact]
    public void RenameActiveScene_WhenTargetExists_BlocksRenameAndLeavesPathUnchanged()
    {
        TestComponentRegistry.EnsureBootstrapped();

        var tempRoot = CreateTempRoot();
        FileSystemRegistry? registry = null;
        CurrentSceneManager? sceneManager = null;

        try
        {
            var context = CreateWorkspaceContext(tempRoot, out registry, out sceneManager);
            context.ScenePersistenceService.TrySaveSceneToPreferredDirectory();

            var existingPath = Path.Combine(context.LevelsFolder, "Taken.scene.json");
            File.WriteAllText(existingPath, "{}");

            var originalPath = sceneManager!.ActiveScenePath;
            var result = context.ScenePathSynchronizationService.RenameActiveScene("Taken");

            Assert.False(result.Success);
            Assert.Equal("IntroScene", sceneManager.ActiveScene!.Name);
            Assert.Equal(originalPath, sceneManager.ActiveScenePath);
            Assert.True(File.Exists(originalPath));

            var manifest = context.ManifestService.TryLoad(tempRoot);
            Assert.NotNull(manifest);
            Assert.Equal("Game/Levels/IntroScene.scene.json", manifest!.LastActiveScene);
        }
        finally
        {
            sceneManager?.StopTicking();
            registry?.Dispose();
            SceneManager.RemoveScene("IntroScene");

            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, true);
            }
        }
    }

    [Fact]
    public void HandlePathRenamed_WhenSceneFileRenamed_UpdatesActivePathAndManifest()
    {
        TestComponentRegistry.EnsureBootstrapped();

        var tempRoot = CreateTempRoot();
        FileSystemRegistry? registry = null;
        CurrentSceneManager? sceneManager = null;

        try
        {
            var context = CreateWorkspaceContext(tempRoot, out registry, out sceneManager);
            context.ScenePersistenceService.TrySaveSceneToPreferredDirectory();

            var oldPath = sceneManager!.ActiveScenePath;
            var newPath = Path.Combine(context.LevelsFolder, "RenamedByAssetBrowser.scene.json");
            File.Move(oldPath, newPath);

            context.ScenePathSynchronizationService.HandlePathRenamed(new FileSystemPathRenamedEvent
            {
                OldPath = oldPath,
                NewPath = newPath,
                IsDirectory = false
            });

            Assert.Equal(newPath, sceneManager.ActiveScenePath);

            var manifest = context.ManifestService.TryLoad(tempRoot);
            Assert.NotNull(manifest);
            Assert.Equal("Game/Levels/RenamedByAssetBrowser.scene.json", manifest!.LastActiveScene);
        }
        finally
        {
            sceneManager?.StopTicking();
            registry?.Dispose();
            SceneManager.RemoveScene("IntroScene");

            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, true);
            }
        }
    }

    [Fact]
    public void HandlePathRenamed_WhenParentFolderMoved_UpdatesActivePathAndManifest()
    {
        TestComponentRegistry.EnsureBootstrapped();

        var tempRoot = CreateTempRoot();
        FileSystemRegistry? registry = null;
        CurrentSceneManager? sceneManager = null;

        try
        {
            var context = CreateWorkspaceContext(tempRoot, out registry, out sceneManager);
            context.ScenePersistenceService.TrySaveSceneToPreferredDirectory();

            var oldFolder = context.LevelsFolder;
            var newFolder = Path.Combine(context.ProjectFolder, "Acts");
            Directory.Move(oldFolder, newFolder);

            context.ScenePathSynchronizationService.HandlePathRenamed(new FileSystemPathRenamedEvent
            {
                OldPath = oldFolder,
                NewPath = newFolder,
                IsDirectory = true
            });

            var expectedScenePath = Path.Combine(newFolder, "IntroScene.scene.json");
            Assert.Equal(expectedScenePath, sceneManager!.ActiveScenePath);

            var manifest = context.ManifestService.TryLoad(tempRoot);
            Assert.NotNull(manifest);
            Assert.Equal("Game/Acts/IntroScene.scene.json", manifest!.LastActiveScene);
        }
        finally
        {
            sceneManager?.StopTicking();
            registry?.Dispose();
            SceneManager.RemoveScene("IntroScene");

            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, true);
            }
        }
    }

    [Fact]
    public void FileSystemMove_WhenSceneFileMoved_UpdatesActivePathAndManifest()
    {
        TestComponentRegistry.EnsureBootstrapped();

        var tempRoot = CreateTempRoot();
        FileSystemRegistry? registry = null;
        CurrentSceneManager? sceneManager = null;

        try
        {
            var context = CreateWorkspaceContext(tempRoot, out registry, out sceneManager);
            context.ScenePersistenceService.TrySaveSceneToPreferredDirectory();

            var destinationFolder = Path.Combine(context.ProjectFolder, "Acts");
            Directory.CreateDirectory(destinationFolder);

            var originalPath = sceneManager!.ActiveScenePath;
            context.FileSystemService.Move(destinationFolder, originalPath);

            var expectedPath = Path.Combine(destinationFolder, "IntroScene.scene.json");
            Assert.Equal(expectedPath, sceneManager.ActiveScenePath);

            var manifest = context.ManifestService.TryLoad(tempRoot);
            Assert.NotNull(manifest);
            Assert.Equal("Game/Acts/IntroScene.scene.json", manifest!.LastActiveScene);
        }
        finally
        {
            sceneManager?.StopTicking();
            registry?.Dispose();
            SceneManager.RemoveScene("IntroScene");

            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, true);
            }
        }
    }

    [Fact]
    public void HandlePathRenamed_WhenChangeIsUnrelated_DoesNotModifyActiveScenePath()
    {
        TestComponentRegistry.EnsureBootstrapped();

        var tempRoot = CreateTempRoot();
        FileSystemRegistry? registry = null;
        CurrentSceneManager? sceneManager = null;

        try
        {
            var context = CreateWorkspaceContext(tempRoot, out registry, out sceneManager);
            context.ScenePersistenceService.TrySaveSceneToPreferredDirectory();

            var originalPath = sceneManager!.ActiveScenePath;
            context.ScenePathSynchronizationService.HandlePathRenamed(new FileSystemPathRenamedEvent
            {
                OldPath = Path.Combine(context.ProjectFolder, "Textures"),
                NewPath = Path.Combine(context.ProjectFolder, "Art"),
                IsDirectory = true
            });

            Assert.Equal(originalPath, sceneManager.ActiveScenePath);

            var manifest = context.ManifestService.TryLoad(tempRoot);
            Assert.NotNull(manifest);
            Assert.Equal("Game/Levels/IntroScene.scene.json", manifest!.LastActiveScene);
        }
        finally
        {
            sceneManager?.StopTicking();
            registry?.Dispose();
            SceneManager.RemoveScene("IntroScene");

            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, true);
            }
        }
    }

    private static WorkspaceContext CreateWorkspaceContext(
        string tempRoot,
        out FileSystemRegistry registry,
        out CurrentSceneManager sceneManager,
        bool seedManifest = false)
    {
        var projectFolder = Path.Combine(tempRoot, "Game");
        var levelsFolder = Path.Combine(projectFolder, "Levels");
        Directory.CreateDirectory(levelsFolder);
        File.WriteAllText(Path.Combine(projectFolder, "Game.csproj"), "<Project Sdk=\"Microsoft.NET.Sdk\" />");

        var manifestService = new EditorManifestService();
        if (seedManifest)
        {
            manifestService.Save(tempRoot, manifestService.CreateDefault("Game"));
        }

        var fileSystemService = new FileSystemService();
        registry = new FileSystemRegistry(fileSystemService);
        var workspaceService = new WorkspaceService(registry, manifestService);
        workspaceService.Load(new WorkspaceDescriptor
        {
            RootPath = tempRoot,
            SolutionFilePath = Path.Combine(tempRoot, "Game.slnx"),
            DefaultProjectPath = "Game",
            ActiveScenePath = string.Empty
        });

        registry.SetCurrentNode(levelsFolder);

        sceneManager = new CurrentSceneManager();
        sceneManager.CreateInMemoryScene("IntroScene");
        var scenePersistenceService = new ScenePersistenceService(sceneManager, workspaceService, manifestService);
        var scenePathSynchronizationService = new ScenePathSynchronizationService(sceneManager, workspaceService, manifestService, fileSystemService);

        return new WorkspaceContext(
            fileSystemService,
            manifestService,
            scenePersistenceService,
            scenePathSynchronizationService,
            projectFolder,
            levelsFolder);
    }

    private static string CreateTempRoot()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "WisdomWarrior.Editor.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempRoot);
        return tempRoot;
    }

    private sealed record WorkspaceContext(
        FileSystemService FileSystemService,
        EditorManifestService ManifestService,
        ScenePersistenceService ScenePersistenceService,
        ScenePathSynchronizationService ScenePathSynchronizationService,
        string ProjectFolder,
        string LevelsFolder);
}
