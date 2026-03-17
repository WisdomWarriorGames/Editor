using WisdomWarrior.Engine.Core;

namespace WisdomWarrior.Editor.Core.ShadowTree;

public class EntityTracker
{
    private readonly object _syncRoot = new();
    private readonly GameEntity _entity;

    private readonly List<ComponentTracker> _components = new();
    private readonly List<EntityTracker> _children = new();

    private int _lastComponentCount;
    private int _lastChildCount;
    private string? _lastName;

    public event Action? OnStructureChanged;
    public GameEntity EngineEntity => _entity;
    public IReadOnlyList<EntityTracker> TrackedChildren
    {
        get
        {
            lock (_syncRoot)
            {
                return _children.ToArray();
            }
        }
    }

    public IReadOnlyList<ComponentTracker> TrackedComponents
    {
        get
        {
            lock (_syncRoot)
            {
                return _components.ToArray();
            }
        }
    }

    public string? Name
    {
        get
        {
            lock (_syncRoot)
            {
                return _lastName;
            }
        }
    }

    public EntityTracker(GameEntity entity)
    {
        _entity = entity;
        _lastName = entity.Name;
        SyncComponents();
        SyncChildren();
    }

    public void AddEntity(GameEntity entity)
    {
        lock (_syncRoot)
        {
            _entity.AddEntity(entity);
        }
    }

    public bool Update()
    {
        var isDirty = false;
        var structureChanged = false;
        Action? onStructureChanged = null;
        ComponentTracker[] components;
        EntityTracker[] children;

        lock (_syncRoot)
        {
            if (_entity.Name != _lastName)
            {
                _lastName = _entity.Name;
                isDirty = true;
            }

            if (_entity.Components.Count != _lastComponentCount)
            {
                SyncComponents();
                structureChanged = true;
                isDirty = true;
            }

            if (_entity.Children.Count != _lastChildCount)
            {
                SyncChildren();
                structureChanged = true;
                isDirty = true;
            }

            if (structureChanged)
            {
                onStructureChanged = OnStructureChanged;
            }

            components = _components.ToArray();
            children = _children.ToArray();
        }

        onStructureChanged?.Invoke();

        foreach (var comp in components)
        {
            comp.Update();
            if (comp.IsDirty) isDirty = true;
        }

        foreach (var child in children)
        {
            if (child.Update()) isDirty = true;
        }

        return isDirty;
    }

    public void AcknowledgeSaved()
    {
        ComponentTracker[] components;
        EntityTracker[] children;

        lock (_syncRoot)
        {
            _lastName = _entity.Name;

            if (_entity.Components.Count != _lastComponentCount)
            {
                SyncComponents();
            }

            if (_entity.Children.Count != _lastChildCount)
            {
                SyncChildren();
            }

            components = _components.ToArray();
            children = _children.ToArray();
        }

        foreach (var component in components)
        {
            component.AcknowledgeSaved();
        }

        foreach (var child in children)
        {
            child.AcknowledgeSaved();
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
