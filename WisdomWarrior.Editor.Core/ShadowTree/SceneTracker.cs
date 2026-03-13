using WisdomWarrior.Engine.Core;

namespace WisdomWarrior.Editor.Core.ShadowTree;

public class SceneTracker
{
    private readonly object _syncRoot = new();
    private Scene? _activeScene;
    private readonly List<EntityTracker> _rootEntities = new();
    private int _lastRootCount;
    private string? _lastName;
    private bool _isDirty;

    public event Action? OnSceneModified;

    public IReadOnlyList<EntityTracker> TrackedRoots
    {
        get
        {
            lock (_syncRoot)
            {
                return _rootEntities.ToArray();
            }
        }
    }

    public Scene? ActiveScene
    {
        get
        {
            lock (_syncRoot)
            {
                return _activeScene;
            }
        }
    }

    public bool IsDirty
    {
        get
        {
            lock (_syncRoot)
            {
                return _isDirty;
            }
        }
    }

    public void TrackScene(Scene scene)
    {
        lock (_syncRoot)
        {
            _activeScene = scene;
            _lastName = scene.Name;
            SyncRoots();
            _isDirty = false;
        }
    }

    public void AddEntity(GameEntity entity)
    {
        lock (_syncRoot)
        {
            if (_activeScene == null) return;
            if (_activeScene.Entities.Contains(entity)) return;
            _activeScene.AddEntity(entity);
        }

        Update();
    }

    public void RemoveEntity(GameEntity entity)
    {
        lock (_syncRoot)
        {
            if (_activeScene == null) return;

            if (entity.Parent != null)
            {
                entity.Parent.RemoveEntity(entity);
            }
            else
            {
                _activeScene.RemoveEntity(entity);
            }
        }

        Update();
    }

    public void Update()
    {
        EntityTracker[] rootTrackers;
        var detectedChanges = false;
        Action? onSceneModified = null;

        lock (_syncRoot)
        {
            if (_activeScene == null) return;

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

            rootTrackers = _rootEntities.ToArray();
        }

        foreach (var rootTracker in rootTrackers)
        {
            if (rootTracker.Update())
            {
                detectedChanges = true;
            }
        }

        lock (_syncRoot)
        {
            if (!detectedChanges) return;
            if (_isDirty) return;

            _isDirty = true;
            onSceneModified = OnSceneModified;
        }

        onSceneModified?.Invoke();
    }

    public void AcknowledgeSaved()
    {
        EntityTracker[] rootTrackers;

        lock (_syncRoot)
        {
            if (_activeScene == null)
            {
                _isDirty = false;
                return;
            }

            _lastName = _activeScene.Name;
            SyncRoots();
            rootTrackers = _rootEntities.ToArray();
            _isDirty = false;
        }

        foreach (var rootTracker in rootTrackers)
        {
            rootTracker.AcknowledgeSaved();
        }
    }

    public EntityTracker? FindTrackerByEntity(GameEntity entity)
    {
        EntityTracker[] rootTrackers;
        lock (_syncRoot)
        {
            rootTrackers = _rootEntities.ToArray();
        }

        foreach (var rootTracker in rootTrackers)
        {
            var match = FindTrackerByEntity(rootTracker, entity);
            if (match != null)
            {
                return match;
            }
        }

        return null;
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

    private static EntityTracker? FindTrackerByEntity(EntityTracker tracker, GameEntity entity)
    {
        if (ReferenceEquals(tracker.EngineEntity, entity))
        {
            return tracker;
        }

        foreach (var childTracker in tracker.TrackedChildren)
        {
            var match = FindTrackerByEntity(childTracker, entity);
            if (match != null)
            {
                return match;
            }
        }

        return null;
    }
}
