using WisdomWarrior.Editor.FileSystem;

namespace WisdomWarrior.Editor.Core.Tests.Services;

public class ProjectCreationFlowTests
{
    [Fact]
    public void CreateSolution_CreatesWorkspaceShape()
    {
        var tempRoot = CreateTempRoot();
        FileSystemRegistry? registry = null;

        try
        {
            var workspaceCreationService = new WorkspaceCreationService();
            var workspace = workspaceCreationService.CreateSolution(tempRoot, "Workspace Game");

            Assert.EndsWith(".slnx", workspace.SolutionFilePath, StringComparison.OrdinalIgnoreCase);
            Assert.True(File.Exists(workspace.SolutionFilePath));
            Assert.True(Directory.Exists(Path.Combine(workspace.RootPath, workspace.DefaultProjectPath)));
            Assert.False(Directory.Exists(Path.Combine(workspace.RootPath, "WorkspaceGame.Game")));
            Assert.False(Directory.EnumerateFiles(workspace.RootPath, "*.manifest.json", SearchOption.TopDirectoryOnly).Any());

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

            Assert.Equal(workspace.RootPath, workspaceService.WorkspaceRoot);
            Assert.Equal(Path.Combine(workspace.RootPath, workspace.DefaultProjectPath), workspaceService.CurrentProjectRoot);
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
    public void WorkspaceLoad_WhenManifestMissing_FallsBackToDescriptorDefaults()
    {
        var tempRoot = CreateTempRoot();
        FileSystemRegistry? registry = null;

        try
        {
            var manifestService = new EditorManifestService();
            var workspaceCreationService = new WorkspaceCreationService(manifestService);
            var workspace = workspaceCreationService.CreateSolution(tempRoot, "Missing Manifest Game");

            File.Delete(manifestService.GetManifestPath(workspace.RootPath));

            registry = new FileSystemRegistry(new FileSystemService());
            var workspaceService = new WorkspaceService(registry, manifestService);
            workspaceService.Load(workspace);

            Assert.Equal(Path.Combine(workspace.RootPath, workspace.DefaultProjectPath), workspaceService.CurrentProjectRoot);
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
    public void WorkspaceLoad_WhenManifestInvalidJson_FallsBackToDescriptorDefaults()
    {
        var tempRoot = CreateTempRoot();
        FileSystemRegistry? registry = null;

        try
        {
            var manifestService = new EditorManifestService();
            var workspaceCreationService = new WorkspaceCreationService(manifestService);
            var workspace = workspaceCreationService.CreateSolution(tempRoot, "Invalid Manifest Game");

            File.WriteAllText(manifestService.GetManifestPath(workspace.RootPath), "{ invalid json");

            registry = new FileSystemRegistry(new FileSystemService());
            var workspaceService = new WorkspaceService(registry, manifestService);
            workspaceService.Load(workspace);

            Assert.Equal(Path.Combine(workspace.RootPath, workspace.DefaultProjectPath), workspaceService.CurrentProjectRoot);
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
    public void WorkspaceLoad_WhenManifestDefaultProjectInvalid_FallsBackToDescriptorDefaults()
    {
        var tempRoot = CreateTempRoot();
        FileSystemRegistry? registry = null;

        try
        {
            var manifestService = new EditorManifestService();
            var workspaceCreationService = new WorkspaceCreationService(manifestService);
            var workspace = workspaceCreationService.CreateSolution(tempRoot, "Invalid Default Project Game");

            manifestService.Save(workspace.RootPath, new EditorManifest
            {
                DefaultProject = "DoesNotExist",
                LastActiveScene = "Nope.scene.json"
            });

            registry = new FileSystemRegistry(new FileSystemService());
            var workspaceService = new WorkspaceService(registry, manifestService);
            workspaceService.Load(workspace);

            Assert.Equal(Path.Combine(workspace.RootPath, workspace.DefaultProjectPath), workspaceService.CurrentProjectRoot);
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
    public void WorkspaceLoad_UsesManifestDefaultProject_WhenValid()
    {
        var tempRoot = CreateTempRoot();
        FileSystemRegistry? registry = null;

        try
        {
            var manifestService = new EditorManifestService();
            var workspaceCreationService = new WorkspaceCreationService(manifestService);
            var workspace = workspaceCreationService.CreateSolution(tempRoot, "Manifest Default Project Game");

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

            Assert.Equal(Path.Combine(workspace.RootPath, alternateProjectRelativePath), workspaceService.CurrentProjectRoot);
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
