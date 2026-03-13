using WisdomWarrior.Editor.Core.Services;

namespace WisdomWarrior.Editor.FileSystem;

public sealed class ScenePathSynchronizationService
{
    private readonly CurrentSceneManager _currentSceneManager;
    private readonly WorkspaceService _workspaceService;
    private readonly EditorManifestService _manifestService;
    private readonly FileSystemService _fileSystemService;
    private FileSystemRegistry? _registry;

    public ScenePathSynchronizationService(
        CurrentSceneManager currentSceneManager,
        WorkspaceService workspaceService,
        EditorManifestService manifestService,
        FileSystemService fileSystemService)
    {
        _currentSceneManager = currentSceneManager;
        _workspaceService = workspaceService;
        _manifestService = manifestService;
        _fileSystemService = fileSystemService;

        _workspaceService.WorkspaceInitialized += OnWorkspaceInitialized;
        _fileSystemService.PathRenamed += HandlePathRenamed;
    }

    public SceneRenameResult RenameActiveScene(string newSceneName)
    {
        var candidateName = newSceneName?.Trim();
        if (string.IsNullOrWhiteSpace(candidateName))
        {
            return new SceneRenameResult(false, "Scene name cannot be empty.");
        }

        if (_currentSceneManager.ActiveScene == null)
        {
            return new SceneRenameResult(false, "There is no active scene to rename.");
        }

        if (!_currentSceneManager.HasPersistedScene)
        {
            _currentSceneManager.RenameActiveScene(candidateName);
            return new SceneRenameResult(true, null);
        }

        var currentPath = _currentSceneManager.ActiveScenePath;
        var directory = Path.GetDirectoryName(currentPath);
        if (string.IsNullOrWhiteSpace(directory))
        {
            return new SceneRenameResult(false, "The current scene path is invalid.");
        }

        var targetFileName = $"{ScenePersistenceService.SanitizeSceneFileName(candidateName)}.scene.json";
        var targetPath = Path.Combine(directory, targetFileName);

        if (!string.Equals(currentPath, targetPath, StringComparison.OrdinalIgnoreCase)
            && File.Exists(targetPath))
        {
            return new SceneRenameResult(false, "A scene file with that name already exists.");
        }

        try
        {
            if (!string.Equals(currentPath, targetPath, StringComparison.OrdinalIgnoreCase))
            {
                File.Move(currentPath, targetPath);
                _currentSceneManager.UpdateActiveScenePath(targetPath);
            }

            _currentSceneManager.RenameActiveScene(candidateName);
            if (!_currentSceneManager.SaveScene())
            {
                return new SceneRenameResult(false, "The scene was renamed but could not be saved.");
            }

            PersistLastActiveScene(_currentSceneManager.ActiveScenePath);
            return new SceneRenameResult(true, null);
        }
        catch (IOException)
        {
            return new SceneRenameResult(false, "The scene file could not be renamed.");
        }
        catch (UnauthorizedAccessException)
        {
            return new SceneRenameResult(false, "The scene file could not be renamed.");
        }
    }

    public void HandlePathRenamed(FileSystemPathRenamedEvent pathRenamed)
    {
        if (!_currentSceneManager.HasPersistedScene)
        {
            return;
        }

        var activeScenePath = _currentSceneManager.ActiveScenePath;
        if (string.IsNullOrWhiteSpace(activeScenePath))
        {
            return;
        }

        var fullActiveScenePath = Path.GetFullPath(activeScenePath);
        var fullOldPath = Path.GetFullPath(pathRenamed.OldPath);
        var fullNewPath = Path.GetFullPath(pathRenamed.NewPath);

        string? updatedScenePath = null;
        if (!pathRenamed.IsDirectory)
        {
            if (string.Equals(fullActiveScenePath, fullOldPath, StringComparison.OrdinalIgnoreCase))
            {
                updatedScenePath = fullNewPath;
            }
        }
        else if (IsUnderPath(fullActiveScenePath, fullOldPath))
        {
            var relative = Path.GetRelativePath(fullOldPath, fullActiveScenePath);
            updatedScenePath = relative == "."
                ? fullNewPath
                : Path.Combine(fullNewPath, relative);
        }

        if (string.IsNullOrWhiteSpace(updatedScenePath))
        {
            return;
        }

        _currentSceneManager.UpdateActiveScenePath(updatedScenePath);
        PersistLastActiveScene(updatedScenePath);
    }

    private void OnWorkspaceInitialized(FileSystemRegistry registry)
    {
        if (_registry != null)
        {
            _registry.PathRenamed -= HandlePathRenamed;
        }

        _registry = registry;
        _registry.PathRenamed += HandlePathRenamed;
    }

    private void PersistLastActiveScene(string scenePath)
    {
        var workspaceRoot = _workspaceService.WorkspaceRoot;
        if (string.IsNullOrWhiteSpace(workspaceRoot))
        {
            return;
        }

        if (!_workspaceService.TryGetProjectRelativePath(scenePath, out var relativeScenePath))
        {
            return;
        }

        var manifest = _manifestService.TryLoad(workspaceRoot)
                       ?? _manifestService.CreateDefault(_workspaceService.GetCurrentProjectRelativePath());

        if (string.IsNullOrWhiteSpace(manifest.DefaultProject))
        {
            manifest.DefaultProject = _workspaceService.GetCurrentProjectRelativePath();
        }

        manifest.LastActiveScene = relativeScenePath;
        _manifestService.Save(workspaceRoot, manifest);
    }

    private static bool IsUnderPath(string childPath, string parentPath)
    {
        var relative = Path.GetRelativePath(parentPath, childPath);
        return !relative.StartsWith("..", StringComparison.Ordinal)
               && !Path.IsPathRooted(relative);
    }
}

public readonly record struct SceneRenameResult(bool Success, string? ErrorMessage);
