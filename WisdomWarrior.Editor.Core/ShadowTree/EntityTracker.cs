using WisdomWarrior.Engine.Core;

namespace WisdomWarrior.Editor.Core.ShadowTree;

public class EntityTracker
{
    private readonly GameEntity _entity;

    private readonly List<ComponentTracker> _components = new();
    private readonly List<EntityTracker> _children = new();

    private int _lastComponentCount;
    private int _lastChildCount;

    public event Action? OnStructureChanged;
    public GameEntity EngineEntity => _entity;
    public IReadOnlyList<EntityTracker> TrackedChildren => _children;
    public IReadOnlyList<ComponentTracker> TrackedComponents => _components;

    public EntityTracker(GameEntity entity)
    {
        _entity = entity;
        SyncComponents();
        SyncChildren();
    }

    public void Update()
    {
        bool structureChanged = false;

        if (_entity.Components.Count != _lastComponentCount)
        {
            SyncComponents();
            structureChanged = true;
        }

        if (_entity.Children.Count != _lastChildCount)
        {
            SyncChildren();
            structureChanged = true;
        }

        if (structureChanged)
        {
            OnStructureChanged?.Invoke();
        }

        foreach (var comp in _components)
        {
            comp.Update();
        }

        foreach (var child in _children)
        {
            child.Update();
        }
    }

    private void SyncComponents()
    {
        var currentEngineComponents = _entity.Components;

        var syncedComponents = new List<ComponentTracker>(currentEngineComponents.Count);

        foreach (var engineComponent in currentEngineComponents)
        {
            var existingTracker = _components.FirstOrDefault(t => t.EngineComponent == engineComponent);

            if (existingTracker != null)
            {
                syncedComponents.Add(existingTracker);
            }
            else
            {
                syncedComponents.Add(new ComponentTracker(engineComponent));
            }
        }

        _components.Clear();
        _components.AddRange(syncedComponents);

        _lastComponentCount = currentEngineComponents.Count;
    }

    private void SyncChildren()
    {
        var currentEngineChildren = _entity.Children;
        var syncedChildren = new List<EntityTracker>(currentEngineChildren.Count);

        foreach (var engineChild in currentEngineChildren)
        {
            var existingTracker = _children.FirstOrDefault(t => t.EngineEntity == engineChild);

            if (existingTracker != null)
            {
                syncedChildren.Add(existingTracker);
            }
            else
            {
                syncedChildren.Add(new EntityTracker(engineChild));
            }
        }

        _children.Clear();
        _children.AddRange(syncedChildren);

        _lastChildCount = currentEngineChildren.Count;
    }
}