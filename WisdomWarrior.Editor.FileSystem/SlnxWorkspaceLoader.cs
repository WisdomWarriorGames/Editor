using System.Xml.Linq;

namespace WisdomWarrior.Editor.FileSystem;

public class SlnxWorkspaceLoader(EditorManifestService manifestService)
{
    private readonly EditorManifestService _manifestService = manifestService;

    public SlnxWorkspaceDescriptor Load(string solutionPath)
    {
        if (string.IsNullOrWhiteSpace(solutionPath))
            throw new ArgumentException("Solution path is required.", nameof(solutionPath));

        if (!File.Exists(solutionPath))
            throw new FileNotFoundException("Solution file was not found.", solutionPath);

        var rootPath = Path.GetDirectoryName(solutionPath);
        if (string.IsNullOrWhiteSpace(rootPath))
            throw new InvalidOperationException("Could not resolve solution root path.");

        var firstProject = GetFirstProjectPath(solutionPath);
        var defaultProject = ResolveProjectDirectory(firstProject);

        EnsureManifestExists(rootPath, defaultProject);

        return new SlnxWorkspaceDescriptor
        {
            RootPath = rootPath,
            SolutionFilePath = solutionPath,
            DefaultProjectPath = defaultProject,
            ActiveScenePath = string.Empty
        };
    }

    private void EnsureManifestExists(string rootPath, string defaultProject)
    {
        var manifestPath = _manifestService.GetManifestPath(rootPath);
        if (File.Exists(manifestPath))
        {
            return;
        }

        var manifest = _manifestService.CreateDefault(defaultProject);
        _manifestService.Save(rootPath, manifest);
    }

    private static string ResolveProjectDirectory(string projectPath)
    {
        var directory = Path.GetDirectoryName(projectPath);
        if (string.IsNullOrWhiteSpace(directory) || directory == ".")
            return string.Empty;

        return directory
            .Replace('\\', '/')
            .Trim('/');
    }

    private static string GetFirstProjectPath(string solutionPath)
    {
        var document = XDocument.Load(solutionPath);

        var firstProjectPath = document.Root?
            .Elements("Project")
            .Select(project => (string?)project.Attribute("Path"))
            .FirstOrDefault(path => !string.IsNullOrWhiteSpace(path));

        if (string.IsNullOrWhiteSpace(firstProjectPath))
            throw new InvalidOperationException("No projects were found in solution.");

        return firstProjectPath
            .Replace('\\', '/')
            .Trim();
    }
}
