using WisdomWarrior.Editor.Core.Services;

namespace WisdomWarrior.Editor.FileSystem;

public class ScenePersistenceService(
    CurrentSceneManager currentSceneManager,
    WorkspaceService workspaceService,
    EditorManifestService manifestService)
{
    private readonly CurrentSceneManager _currentSceneManager = currentSceneManager;
    private readonly WorkspaceService _workspaceService = workspaceService;
    private readonly EditorManifestService _manifestService = manifestService;

    public bool HasUnsavedInMemoryDirtyScene
    {
        get
        {
            _currentSceneManager.Tracker.Update();
            return _currentSceneManager.IsDirty && !_currentSceneManager.HasPersistedScene;
        }
    }

    public bool HasDirtyPersistedScene
    {
        get
        {
            _currentSceneManager.Tracker.Update();
            return _currentSceneManager.IsDirty && _currentSceneManager.HasPersistedScene;
        }
    }

    public bool TrySaveSceneToPreferredDirectory()
    {
        var directory = _workspaceService.GetPreferredSceneSaveDirectory();
        return TrySaveSceneToDirectory(directory);
    }

    public bool TrySaveSceneToDirectory(string directoryPath)
    {
        if (!_workspaceService.IsValidSceneSaveDirectory(directoryPath))
            return false;

        var sceneName = SanitizeSceneFileName(_currentSceneManager.ActiveScene?.Name);
        var targetScenePath = Path.Combine(directoryPath, $"{sceneName}.scene.json");

        return TrySaveSceneToPath(targetScenePath);
    }

    public bool TryAutoSavePersistedScene()
    {
        _currentSceneManager.Tracker.Update();

        if (!_currentSceneManager.IsDirty)
            return true;

        if (!_currentSceneManager.HasPersistedScene)
            return false;

        if (!_currentSceneManager.SaveScene())
            return false;

        PersistLastActiveScene(_currentSceneManager.ActiveScenePath);
        return true;
    }

    private bool TrySaveSceneToPath(string scenePath)
    {
        var directory = Path.GetDirectoryName(scenePath);
        if (string.IsNullOrWhiteSpace(directory))
            return false;

        if (!_workspaceService.IsValidSceneSaveDirectory(directory))
            return false;

        if (!_currentSceneManager.SaveSceneAs(scenePath))
            return false;

        PersistLastActiveScene(scenePath);
        return true;
    }

    private void PersistLastActiveScene(string scenePath)
    {
        var root = _workspaceService.WorkspaceRoot;
        if (string.IsNullOrWhiteSpace(root))
            return;

        if (!_workspaceService.TryGetProjectRelativePath(scenePath, out var relativeScenePath))
            return;

        var manifest = _manifestService.TryLoad(root) ?? _manifestService.CreateDefault(_workspaceService.GetCurrentProjectRelativePath());
        if (string.IsNullOrWhiteSpace(manifest.DefaultProject))
        {
            manifest.DefaultProject = _workspaceService.GetCurrentProjectRelativePath();
        }

        manifest.LastActiveScene = relativeScenePath;
        _manifestService.Save(root, manifest);
    }

    private static string SanitizeSceneFileName(string? sceneName)
    {
        var candidate = string.IsNullOrWhiteSpace(sceneName) ? "Scene1" : sceneName.Trim();
        var invalidChars = Path.GetInvalidFileNameChars();

        var sanitized = new string(candidate.Select(ch => invalidChars.Contains(ch) ? '_' : ch).ToArray()).Trim();
        return string.IsNullOrWhiteSpace(sanitized) ? "Scene1" : sanitized;
    }
}
