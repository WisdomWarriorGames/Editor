using WisdomWarrior.Engine.Core;

namespace WisdomWarrior.Editor.Core.ShadowTree;

public class SceneTracker
{
    private Scene? _activeScene;
    private readonly List<EntityTracker> _rootEntities = new();
    private int _lastRootCount;
    private string? _lastName;

    public event Action? OnSceneModified;

    public IReadOnlyList<EntityTracker> TrackedRoots => _rootEntities;
    public Scene? ActiveScene => _activeScene;

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

        var isSceneDirty = false;

        if (_activeScene.Name != _lastName)
        {
            _lastName = _activeScene.Name;
            isSceneDirty = true;
        }

        if (_activeScene.Entities.Count != _lastRootCount)
        {
            SyncRoots();
            isSceneDirty = true;
        }

        foreach (var rootTracker in _rootEntities)
        {
            if (rootTracker.Update())
            {
                isSceneDirty = true;
            }
        }

        if (isSceneDirty)
        {
            OnSceneModified?.Invoke();
        }
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