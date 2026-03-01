using WisdomWarrior.Engine.Core;

namespace WisdomWarrior.Editor.Core.ShadowTree;

public class SceneTracker
{
    private Scene? _activeScene;
    private readonly List<EntityTracker> _rootEntities = new();
    private int _lastRootCount;

    public event Action? OnSceneModified;
    
    public IReadOnlyList<EntityTracker> TrackedRoots => _rootEntities;
    public Scene? ActiveScene => _activeScene;
    
    public void TrackScene(Scene scene)
    {
        _activeScene = scene;
        SyncRoots();
    }

    public void Update()
    {
        if (_activeScene == null) return;

        bool structureChanged = false;
        
        if (_activeScene.Entities.Count != _lastRootCount)
        {
            SyncRoots();
            structureChanged = true;
        }
        
        if (structureChanged)
        {
            OnSceneModified?.Invoke();
        }
        
        foreach (var rootTracker in _rootEntities)
        {
            rootTracker.Update();
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