using WisdomWarrior.Editor.FileSystem;

namespace WisdomWarrior.Editor.Core.Tests.Services;

public class WorkspaceLoaderTests
{
    [Fact]
    public void Load_WhenManifestMissing_CreatesManifestNextToSlnx_UsingFirstProjectAsDefault()
    {
        var tempRoot = CreateTempRoot();
        FileSystemRegistry? registry = null;

        try
        {
            var solutionPath = CreateSolutionWithProjects(
                tempRoot,
                "OpenGame.slnx",
                "ProjectA/ProjectA.csproj",
                "ProjectB/ProjectB.csproj");

            var manifestService = new EditorManifestService();
            var loader = new WorkspaceLoader(manifestService);
            var workspace = loader.Load(solutionPath);

            var manifestPath = manifestService.GetManifestPath(tempRoot);
            Assert.True(File.Exists(manifestPath));
            Assert.Equal(Path.GetDirectoryName(solutionPath), Path.GetDirectoryName(manifestPath));

            var manifest = manifestService.TryLoad(tempRoot);
            Assert.NotNull(manifest);
            Assert.Equal("ProjectA", manifest!.DefaultProject);
            Assert.True(string.IsNullOrWhiteSpace(manifest.LastActiveScene));

            registry = new FileSystemRegistry(new FileSystemService());
            var workspaceService = new WorkspaceService(registry, manifestService);
            workspaceService.Load(workspace);

            Assert.Equal(Path.Combine(tempRoot, "ProjectA"), workspaceService.CurrentProjectRoot);
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
    public void Load_WhenManifestExists_DoesNotOverwriteExistingDefaultProject()
    {
        var tempRoot = CreateTempRoot();

        try
        {
            var solutionPath = CreateSolutionWithProjects(
                tempRoot,
                "OpenGame.slnx",
                "ProjectA/ProjectA.csproj",
                "ProjectB/ProjectB.csproj");

            var manifestService = new EditorManifestService();
            manifestService.Save(tempRoot, new EditorManifest
            {
                DefaultProject = "ProjectB",
                LastActiveScene = null
            });

            var loader = new WorkspaceLoader(manifestService);
            var workspace = loader.Load(solutionPath);

            Assert.Equal("ProjectA", workspace.DefaultProjectPath);

            var manifest = manifestService.TryLoad(tempRoot);
            Assert.NotNull(manifest);
            Assert.Equal("ProjectB", manifest!.DefaultProject);
        }
        finally
        {
            if (Directory.Exists(tempRoot))
            {
                Directory.Delete(tempRoot, true);
            }
        }
    }

    private static string CreateSolutionWithProjects(string root, string solutionName, params string[] projectPaths)
    {
        foreach (var projectPath in projectPaths)
        {
            var relativePath = projectPath.Replace('/', Path.DirectorySeparatorChar);
            var fullPath = Path.Combine(root, relativePath);
            var directory = Path.GetDirectoryName(fullPath)!;
            Directory.CreateDirectory(directory);
            File.WriteAllText(fullPath, "<Project Sdk=\"Microsoft.NET.Sdk\" />");
        }

        var projectLines = string.Join(
            Environment.NewLine,
            projectPaths.Select(path => $"  <Project Path=\"{path.Replace('\\', '/')}\" />"));

        var solutionContent = $"""
                               <Solution>
                               {projectLines}
                               </Solution>
                               """;

        var solutionPath = Path.Combine(root, solutionName);
        File.WriteAllText(solutionPath, solutionContent);
        return solutionPath;
    }

    private static string CreateTempRoot()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "WisdomWarrior.Editor.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempRoot);
        return tempRoot;
    }
}
