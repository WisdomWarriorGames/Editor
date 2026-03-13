using WisdomWarrior.Editor.Core.Helpers;

namespace WisdomWarrior.Editor.FileSystem;

public class WorkspaceService(FileSystemRegistry registry, EditorManifestService manifestService)
{
    private readonly FileSystemRegistry _currentRegistry = registry;
    private readonly EditorManifestService _manifestService = manifestService;
    private string _activeScenePath = string.Empty;

    public string WorkspaceRoot { get; private set; } = string.Empty;
    public string CurrentProjectRoot { get; private set; } = string.Empty;
    public string ActiveScene => string.IsNullOrWhiteSpace(_activeScenePath)
        ? string.Empty
        : Path.Combine(
            WorkspaceRoot,
            _activeScenePath
                .Replace('/', Path.DirectorySeparatorChar)
                .Replace('\\', Path.DirectorySeparatorChar));

    public event Action<FileSystemRegistry>? WorkspaceInitialized;

    public void Load(WorkspaceDescriptor? workspace)
    {
        if (workspace == null)
        {
            return;
        }

        WorkspaceRoot = workspace.RootPath;
        AssetPathContext.Configure(WorkspaceRoot, null);

        var manifest = _manifestService.TryLoad(WorkspaceRoot);

        var defaultProjectRelativePath = ResolveDefaultProjectPath(manifest, workspace);
        var defaultProjectPath = Path.Combine(WorkspaceRoot, defaultProjectRelativePath);

        if (!IsValidProjectDirectory(defaultProjectPath))
        {
            defaultProjectPath = Path.Combine(WorkspaceRoot, workspace.DefaultProjectPath);
        }

        _activeScenePath = ResolveActiveScenePath(manifest, workspace);

        if (!IsValidProjectDirectory(defaultProjectPath))
        {
            _activeScenePath = string.Empty;
            return;
        }

        ChangeProject(defaultProjectPath);
    }

    private static string ResolveDefaultProjectPath(EditorManifest? manifest, WorkspaceDescriptor workspace)
    {
        var candidate = manifest?.DefaultProject;
        if (string.IsNullOrWhiteSpace(candidate) || Path.IsPathRooted(candidate))
        {
            return workspace.DefaultProjectPath;
        }

        return candidate.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
    }

    private static string ResolveActiveScenePath(EditorManifest? manifest, WorkspaceDescriptor workspace)
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

    public void ChangeProject(string path)
    {
        _currentRegistry.Initialize(path);
        CurrentProjectRoot = path;
        AssetPathContext.Configure(WorkspaceRoot, CurrentProjectRoot);
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

        if (Directory.Exists(CurrentProjectRoot))
        {
            return CurrentProjectRoot;
        }

        return Directory.Exists(WorkspaceRoot) ? WorkspaceRoot : string.Empty;
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

        if (string.IsNullOrWhiteSpace(WorkspaceRoot) || string.IsNullOrWhiteSpace(absolutePath))
            return false;

        var fullProjectRoot = Path.GetFullPath(WorkspaceRoot);
        var fullPath = Path.GetFullPath(absolutePath);

        if (!IsUnderPath(fullPath, fullProjectRoot))
            return false;

        var relative = Path.GetRelativePath(fullProjectRoot, fullPath);
        relativePath = relative.Replace('\\', '/');
        return true;
    }

    public string GetCurrentProjectRelativePath()
    {
        if (string.IsNullOrWhiteSpace(CurrentProjectRoot))
            return string.Empty;

        return TryGetProjectRelativePath(CurrentProjectRoot, out var relativePath)
            ? relativePath
            : string.Empty;
    }

    private IReadOnlyList<string> GetProjectDirectories()
    {
        if (string.IsNullOrWhiteSpace(WorkspaceRoot) || !Directory.Exists(WorkspaceRoot))
            return [];

        return Directory.EnumerateFiles(WorkspaceRoot, "*.csproj", SearchOption.AllDirectories)
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
