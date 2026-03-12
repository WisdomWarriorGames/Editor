using WisdomWarrior.Engine.Core;

namespace WisdomWarrior.Editor.Core.ShadowTree;

public class SceneTracker
{
    private Scene? _activeScene;
    private readonly List<EntityTracker> _rootEntities = new();
    private int _lastRootCount;
    private string? _lastName;
    private bool _isDirty;

    public event Action? OnSceneModified;

    public IReadOnlyList<EntityTracker> TrackedRoots => _rootEntities;
    public Scene? ActiveScene => _activeScene;
    public bool IsDirty => _isDirty;

    public void TrackScene(Scene scene)
    {
        _activeScene = scene;
        _lastName = scene.Name;
        SyncRoots();
    }

    public void AddEntity(GameEntity entity)
    {
        if (_activeScene == null) return;
        if (_activeScene.Entities.Contains(entity)) return;
        _activeScene.AddEntity(entity);

        Update();
    }

    public void RemoveEntity(GameEntity entity)
    {
        if (entity.Parent != null)
        {
            entity.Parent.Children.Remove(entity);
        }
        else if (_activeScene != null)
        {
            _activeScene.Entities.Remove(entity);
        }

        Update();
    }

    public void Update()
    {
        if (_activeScene == null) return;

        var detectedChanges = false;

        if (_activeScene.Name != _lastName)
        {
            _lastName = _activeScene.Name;
            detectedChanges = true;
        }

        if (_activeScene.Entities.Count != _lastRootCount)
        {
            SyncRoots();
            detectedChanges = true;
        }

        foreach (var rootTracker in _rootEntities)
        {
            if (rootTracker.Update())
            {
                detectedChanges = true;
            }
        }

        if (!detectedChanges) return;

        if (_isDirty) return;

        _isDirty = true;
        OnSceneModified?.Invoke();
    }

    public void AcknowledgeSaved()
    {
        if (_activeScene == null)
        {
            _isDirty = false;
            return;
        }

        _lastName = _activeScene.Name;
        SyncRoots();

        foreach (var rootTracker in _rootEntities)
        {
            rootTracker.AcknowledgeSaved();
        }

        _isDirty = false;
    }

    private void SyncRoots()
    {
        if (_activeScene == null) return;

        var currentEngineRoots = _activeScene.Entities;
        var syncedRoots = new List<EntityTracker>(currentEngineRoots.Count);

        foreach (var engineRoot in currentEngineRoots)
        {
            var existingTracker = _rootEntities.FirstOrDefault(t => t.EngineEntity == engineRoot);

            if (existingTracker != null)
            {
                syncedRoots.Add(existingTracker);
            }
            else
            {
                syncedRoots.Add(new EntityTracker(engineRoot));
            }
        }

        _rootEntities.Clear();
        _rootEntities.AddRange(syncedRoots);

        _lastRootCount = currentEngineRoots.Count;
    }
}
