using WisdomWarrior.Editor.FileSystem;

namespace WisdomWarrior.Editor.Core.Tests.Services;

public class ProjectCreationFlowTests
{
    [Fact]
    public void LegacyCreateSolution_KeepsLegacyProjectShape()
    {
        var tempRoot = CreateTempRoot();

        try
        {
            var projectService = new ProjectService();
            var manifest = projectService.CreateSolution(tempRoot, "Legacy Game");
            var rootPath = projectService.GetRootPath(tempRoot, "Legacy Game");

            Assert.NotNull(manifest);
            Assert.True(Directory.Exists(Path.Combine(rootPath, "LegacyGame")));
            Assert.True(Directory.Exists(Path.Combine(rootPath, "LegacyGame.Game")));
            Assert.True(File.Exists(Path.Combine(rootPath, "LegacyGame.Game", "CurrentScene.scene.json")));
            Assert.True(File.Exists(Path.Combine(rootPath, "LegacyGame.manifest.json")));
            Assert.True(Directory.EnumerateFiles(rootPath, "*.sln*", SearchOption.TopDirectoryOnly).Any());
        }
        finally
        {
            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, true);
            }
        }
    }

    [Fact]
    public void SlnxCreateSolution_CreatesSeparateSlnxWorkspaceShape()
    {
        var tempRoot = CreateTempRoot();
        FileSystemRegistry? registry = null;

        try
        {
            var projectService = new SlnxProjectService();
            var workspace = projectService.CreateSolution(tempRoot, "Slnx Game");

            Assert.EndsWith(".slnx", workspace.SolutionFilePath, StringComparison.OrdinalIgnoreCase);
            Assert.True(File.Exists(workspace.SolutionFilePath));
            Assert.True(Directory.Exists(Path.Combine(workspace.RootPath, workspace.DefaultProjectPath)));
            Assert.False(Directory.Exists(Path.Combine(workspace.RootPath, "SlnxGame.Game")));

            var projectFiles = Directory.GetFiles(workspace.RootPath, "*.csproj", SearchOption.AllDirectories);
            Assert.Single(projectFiles);

            var sceneFiles = Directory.GetFiles(workspace.RootPath, "*.scene.json", SearchOption.AllDirectories);
            Assert.Empty(sceneFiles);
            Assert.True(string.IsNullOrWhiteSpace(workspace.ActiveScenePath));

            var manifestService = new EditorManifestService();
            var manifestPath = manifestService.GetManifestPath(workspace.RootPath);
            Assert.True(File.Exists(manifestPath));
            Assert.Equal(Path.GetDirectoryName(workspace.SolutionFilePath), Path.GetDirectoryName(manifestPath));

            var manifest = manifestService.TryLoad(workspace.RootPath);
            Assert.NotNull(manifest);
            Assert.Equal(workspace.DefaultProjectPath, manifest!.DefaultProject);
            Assert.True(string.IsNullOrWhiteSpace(manifest.LastActiveScene));

            var solutionText = File.ReadAllText(workspace.SolutionFilePath);
            Assert.Contains(Path.GetFileName(projectFiles[0]), solutionText, StringComparison.OrdinalIgnoreCase);

            var fileSystemService = new FileSystemService();
            registry = new FileSystemRegistry(fileSystemService);
            var workspaceService = new WorkspaceService(registry, manifestService);

            workspaceService.Load(workspace);

            Assert.Equal(workspace.RootPath, workspaceService.ProjectRoot);
            Assert.Equal(Path.Combine(workspace.RootPath, workspace.DefaultProjectPath), workspaceService.CurrentModuleRoot);
            Assert.Equal(string.Empty, workspaceService.ActiveScene);
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
    public void SlnxWorkspaceLoad_WhenManifestMissing_FallsBackToDescriptorDefaults()
    {
        var tempRoot = CreateTempRoot();
        FileSystemRegistry? registry = null;

        try
        {
            var manifestService = new EditorManifestService();
            var projectService = new SlnxProjectService(manifestService);
            var workspace = projectService.CreateSolution(tempRoot, "Missing Manifest Game");

            File.Delete(manifestService.GetManifestPath(workspace.RootPath));

            registry = new FileSystemRegistry(new FileSystemService());
            var workspaceService = new WorkspaceService(registry, manifestService);
            workspaceService.Load(workspace);

            Assert.Equal(Path.Combine(workspace.RootPath, workspace.DefaultProjectPath), workspaceService.CurrentModuleRoot);
            Assert.Equal(string.Empty, workspaceService.ActiveScene);
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
    public void SlnxWorkspaceLoad_WhenManifestInvalidJson_FallsBackToDescriptorDefaults()
    {
        var tempRoot = CreateTempRoot();
        FileSystemRegistry? registry = null;

        try
        {
            var manifestService = new EditorManifestService();
            var projectService = new SlnxProjectService(manifestService);
            var workspace = projectService.CreateSolution(tempRoot, "Invalid Manifest Game");

            File.WriteAllText(manifestService.GetManifestPath(workspace.RootPath), "{ invalid json");

            registry = new FileSystemRegistry(new FileSystemService());
            var workspaceService = new WorkspaceService(registry, manifestService);
            workspaceService.Load(workspace);

            Assert.Equal(Path.Combine(workspace.RootPath, workspace.DefaultProjectPath), workspaceService.CurrentModuleRoot);
            Assert.Equal(string.Empty, workspaceService.ActiveScene);
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
    public void SlnxWorkspaceLoad_WhenManifestDefaultProjectInvalid_FallsBackToDescriptorDefaults()
    {
        var tempRoot = CreateTempRoot();
        FileSystemRegistry? registry = null;

        try
        {
            var manifestService = new EditorManifestService();
            var projectService = new SlnxProjectService(manifestService);
            var workspace = projectService.CreateSolution(tempRoot, "Invalid Default Project Game");

            manifestService.Save(workspace.RootPath, new EditorManifest
            {
                DefaultProject = "DoesNotExist",
                LastActiveScene = "Nope.scene.json"
            });

            registry = new FileSystemRegistry(new FileSystemService());
            var workspaceService = new WorkspaceService(registry, manifestService);
            workspaceService.Load(workspace);

            Assert.Equal(Path.Combine(workspace.RootPath, workspace.DefaultProjectPath), workspaceService.CurrentModuleRoot);
            Assert.Equal(string.Empty, workspaceService.ActiveScene);
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
    public void SlnxWorkspaceLoad_UsesManifestDefaultProject_WhenValid()
    {
        var tempRoot = CreateTempRoot();
        FileSystemRegistry? registry = null;

        try
        {
            var manifestService = new EditorManifestService();
            var projectService = new SlnxProjectService(manifestService);
            var workspace = projectService.CreateSolution(tempRoot, "Manifest Default Project Game");

            var alternateProjectRelativePath = "AlternateProject";
            var alternateProjectPath = Path.Combine(workspace.RootPath, alternateProjectRelativePath);
            Directory.CreateDirectory(alternateProjectPath);
            File.WriteAllText(Path.Combine(alternateProjectPath, "AlternateProject.csproj"), "<Project Sdk=\"Microsoft.NET.Sdk\" />");

            manifestService.Save(workspace.RootPath, new EditorManifest
            {
                DefaultProject = alternateProjectRelativePath,
                LastActiveScene = null
            });

            registry = new FileSystemRegistry(new FileSystemService());
            var workspaceService = new WorkspaceService(registry, manifestService);
            workspaceService.Load(workspace);

            Assert.Equal(Path.Combine(workspace.RootPath, alternateProjectRelativePath), workspaceService.CurrentModuleRoot);
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
