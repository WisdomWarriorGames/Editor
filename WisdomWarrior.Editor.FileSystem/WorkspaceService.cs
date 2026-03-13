using System.Text.Json;
using WisdomWarrior.Editor.Core;
using WisdomWarrior.Editor.Core.Helpers;

namespace WisdomWarrior.Editor.FileSystem;

public class WorkspaceService(FileSystemRegistry registry, EditorManifestService manifestService)
{
    private readonly FileSystemRegistry _currentRegistry = registry;
    private readonly EditorManifestService _manifestService = manifestService;
    private Manifest? _project;
    private string _activeScenePath = string.Empty;

    public string ProjectRoot { get; private set; } = string.Empty;
    public string CurrentModuleRoot { get; private set; } = string.Empty;
    public string ActiveScene => string.IsNullOrWhiteSpace(_activeScenePath)
        ? string.Empty
        : Path.Combine(
            ProjectRoot,
            _activeScenePath
                .Replace('/', Path.DirectorySeparatorChar)
                .Replace('\\', Path.DirectorySeparatorChar));

    public event Action<FileSystemRegistry>? WorkspaceInitialized;

    public void Load(string pathToManifest)
    {
        var json = File.ReadAllText(pathToManifest);
        _project = JsonSerializer.Deserialize<Manifest>(json);

        var baseDirectory = Path.GetDirectoryName(pathToManifest)!;
        Load(_project, baseDirectory);
    }

    public void Load(Manifest? manifest, string rootPath)
    {
        _project = manifest;

        if (_project == null)
        {
            return;
        }

        ProjectRoot = rootPath;
        _activeScenePath = _project.ActiveScene;
        AssetPathContext.Configure(ProjectRoot, null);

        if (_project.Modules.Count > 0)
        {
            var module = _project.Modules[0];
            var path = Path.Combine(rootPath, module.Path);
            ChangeModule(path);
        }
    }

    public void Load(SlnxWorkspaceDescriptor? workspace)
    {
        if (workspace == null)
        {
            return;
        }

        _project = null;
        ProjectRoot = workspace.RootPath;
        AssetPathContext.Configure(ProjectRoot, null);

        var manifest = _manifestService.TryLoad(ProjectRoot);

        var defaultProjectRelativePath = ResolveDefaultProjectPath(manifest, workspace);
        var defaultProjectPath = Path.Combine(ProjectRoot, defaultProjectRelativePath);

        if (!IsValidProjectDirectory(defaultProjectPath))
        {
            defaultProjectPath = Path.Combine(ProjectRoot, workspace.DefaultProjectPath);
        }

        _activeScenePath = ResolveActiveScenePath(manifest, workspace);

        if (!IsValidProjectDirectory(defaultProjectPath))
        {
            _activeScenePath = string.Empty;
            return;
        }

        ChangeModule(defaultProjectPath);
    }

    private static string ResolveDefaultProjectPath(EditorManifest? manifest, SlnxWorkspaceDescriptor workspace)
    {
        var candidate = manifest?.DefaultProject;
        if (string.IsNullOrWhiteSpace(candidate) || Path.IsPathRooted(candidate))
        {
            return workspace.DefaultProjectPath;
        }

        return candidate.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
    }

    private static string ResolveActiveScenePath(EditorManifest? manifest, SlnxWorkspaceDescriptor workspace)
    {
        var candidate = manifest?.LastActiveScene;
        if (string.IsNullOrWhiteSpace(candidate) || Path.IsPathRooted(candidate))
        {
            candidate = workspace.ActiveScenePath;
        }

        if (string.IsNullOrWhiteSpace(candidate))
        {
            return string.Empty;
        }

        var normalized = candidate.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
        return File.Exists(Path.Combine(workspace.RootPath, normalized)) ? normalized : string.Empty;
    }

    private static bool IsValidProjectDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            return false;
        }

        return Directory.EnumerateFiles(path, "*.csproj", SearchOption.TopDirectoryOnly).Any();
    }

    public void ChangeModule(string path)
    {
        _currentRegistry.Initialize(path);
        CurrentModuleRoot = path;
        AssetPathContext.Configure(ProjectRoot, CurrentModuleRoot);
        WorkspaceInitialized?.Invoke(_currentRegistry);
    }

    public string GetPreferredSceneSaveDirectory()
    {
        var currentNodePath = _currentRegistry.Nodes.Count > 0 ? _currentRegistry.CurrentNode.FullPath : string.Empty;

        if (!string.IsNullOrWhiteSpace(currentNodePath))
        {
            if (Directory.Exists(currentNodePath))
            {
                return currentNodePath;
            }

            if (File.Exists(currentNodePath))
            {
                var parent = Path.GetDirectoryName(currentNodePath);
                if (!string.IsNullOrWhiteSpace(parent))
                {
                    return parent;
                }
            }
        }

        if (Directory.Exists(CurrentModuleRoot))
        {
            return CurrentModuleRoot;
        }

        return Directory.Exists(ProjectRoot) ? ProjectRoot : string.Empty;
    }

    public bool IsValidSceneSaveDirectory(string directoryPath)
    {
        if (string.IsNullOrWhiteSpace(directoryPath))
            return false;

        if (!Directory.Exists(directoryPath))
            return false;

        var fullDirectoryPath = Path.GetFullPath(directoryPath);
        if (!TryGetProjectRelativePath(fullDirectoryPath, out _))
            return false;

        var projectDirectories = GetProjectDirectories();
        if (projectDirectories.Count == 0)
            return false;

        return projectDirectories.Any(projectDirectory => IsUnderPath(fullDirectoryPath, projectDirectory));
    }

    public bool TryGetProjectRelativePath(string absolutePath, out string relativePath)
    {
        relativePath = string.Empty;

        if (string.IsNullOrWhiteSpace(ProjectRoot) || string.IsNullOrWhiteSpace(absolutePath))
            return false;

        var fullProjectRoot = Path.GetFullPath(ProjectRoot);
        var fullPath = Path.GetFullPath(absolutePath);

        if (!IsUnderPath(fullPath, fullProjectRoot))
            return false;

        var relative = Path.GetRelativePath(fullProjectRoot, fullPath);
        relativePath = relative.Replace('\\', '/');
        return true;
    }

    public string GetCurrentModuleRelativePath()
    {
        if (string.IsNullOrWhiteSpace(CurrentModuleRoot))
            return string.Empty;

        return TryGetProjectRelativePath(CurrentModuleRoot, out var relativePath)
            ? relativePath
            : string.Empty;
    }

    private IReadOnlyList<string> GetProjectDirectories()
    {
        if (string.IsNullOrWhiteSpace(ProjectRoot) || !Directory.Exists(ProjectRoot))
            return [];

        return Directory.EnumerateFiles(ProjectRoot, "*.csproj", SearchOption.AllDirectories)
            .Select(Path.GetDirectoryName)
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Select(Path.GetFullPath)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    private static bool IsUnderPath(string childPath, string parentPath)
    {
        var relative = Path.GetRelativePath(parentPath, childPath);
        return !relative.StartsWith("..", StringComparison.Ordinal)
               && !Path.IsPathRooted(relative);
    }
}
