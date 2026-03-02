using System.Collections;
using WisdomWarrior.Editor.Core.ShadowTree;
using WisdomWarrior.Editor.SceneList.ViewModels;
using WisdomWarrior.Engine.Core;

namespace WisdomWarrior.Editor.SceneList.Helpers;

public static class HierarchyHelpers
{
    public static void ProcessEntityDrop(this object? droppedItem, object? target, SceneTracker sceneTracker)
    {
        if (droppedItem == null) return;

        var draggedEntities = new List<EntityViewModel>();

        if (droppedItem is EntityViewModel single)
            draggedEntities.Add(single);
        else if (droppedItem is IEnumerable list)
            draggedEntities.AddRange(list.OfType<EntityViewModel>());

        if (draggedEntities.Count == 0) return;

        GameEntity? newParent = null;
        var dropInScene = target is SceneNodeViewModel || target is SceneHierarchyViewModel;

        if (target is EntityViewModel targetEntity)
        {
            newParent = targetEntity.Tracker.EngineEntity;
        }

        foreach (var entityVm in draggedEntities)
        {
            var engineEntity = entityVm.Tracker.EngineEntity;

            if (newParent != null && IsDescendantOf(newParent, engineEntity))
                continue;

            if (engineEntity.Parent == null)
            {
                sceneTracker.RemoveEntity(engineEntity);
            }

            engineEntity.SetParent(newParent);

            if (dropInScene)
            {
                sceneTracker.AddEntity(engineEntity);
            }
        }
    }


    private static bool IsDescendantOf(GameEntity potentialChild, GameEntity parent)
    {
        var current = potentialChild.Parent;
        while (current != null)
        {
            if (current == parent) return true;
            current = current.Parent;
        }

        return false;
    }
}