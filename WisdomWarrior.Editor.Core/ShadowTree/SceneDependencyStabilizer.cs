using System.Collections.Concurrent;
using System.Reflection;
using WisdomWarrior.Engine.Core;
using WisdomWarrior.Engine.Core.Attributes;
using SceneSystem = WisdomWarrior.Engine.Core.Systems.System;

namespace WisdomWarrior.Editor.Core.ShadowTree;

internal static class SceneDependencyStabilizer
{
    private sealed record ComponentMetadata(
        bool LimitToOne,
        Type[] RequiredComponentTypes,
        Type[] RequiredSystemTypes);

    private sealed record SystemMetadata(
        bool LimitToOne,
        Type[] RequiredSystemTypes);

    private static readonly ConcurrentDictionary<Type, ComponentMetadata> ComponentMetadataCache = new();
    private static readonly ConcurrentDictionary<Type, SystemMetadata> SystemMetadataCache = new();

    public static bool Stabilize(Scene scene)
    {
        ArgumentNullException.ThrowIfNull(scene);

        var changed = false;

        for (var pass = 0; pass < 256; pass++)
        {
            var passChanged = false;

            if (RepairRootEntities(scene))
            {
                passChanged = true;
            }

            foreach (var root in scene.Entities.ToArray())
            {
                if (RepairEntityTree(scene, root))
                {
                    passChanged = true;
                }
            }

            if (RepairSystems(scene))
            {
                passChanged = true;
            }

            if (!passChanged)
            {
                return changed;
            }

            changed = true;
        }

        throw new InvalidOperationException("Scene dependency stabilization did not converge.");
    }

    private static bool RepairRootEntities(Scene scene)
    {
        var changed = false;

        foreach (var root in scene.Entities.ToArray())
        {
            if (ReferenceEquals(root.Parent, null) && ReferenceEquals(root.CurrentScene, scene))
            {
                continue;
            }

            scene.RemoveEntity(root);
            scene.AddEntity(root);
            changed = true;
        }

        return changed;
    }

    private static bool RepairEntityTree(Scene scene, GameEntity entity)
    {
        var changed = false;
        var seenLimitedComponentTypes = new HashSet<Type>();

        foreach (var component in entity.Components.ToArray())
        {
            var metadata = GetComponentMetadata(component.GetType());

            if (metadata.LimitToOne && !seenLimitedComponentTypes.Add(component.GetType()))
            {
                entity.RemoveComponent(component);
                changed = true;
                continue;
            }

            if (!ReferenceEquals(component.Parent, entity))
            {
                if (metadata.LimitToOne
                    && entity.Components.Count(existing => existing.GetType() == component.GetType()) > 1)
                {
                    continue;
                }

                entity.RemoveComponent(component);
                entity.AddComponent(component);
                changed = true;
                continue;
            }

            foreach (var requiredComponentType in metadata.RequiredComponentTypes)
            {
                if (entity.Components.Any(existing => existing.GetType() == requiredComponentType))
                {
                    continue;
                }

                if (!TryCreateComponent(requiredComponentType, out var requiredComponent))
                {
                    continue;
                }

                entity.AddComponent(requiredComponent);
                changed = true;
            }

            foreach (var requiredSystemType in metadata.RequiredSystemTypes)
            {
                if (scene.Systems.Any(existing => existing.GetType() == requiredSystemType))
                {
                    continue;
                }

                if (!TryCreateSystem(requiredSystemType, out var requiredSystem))
                {
                    continue;
                }

                scene.AddSystem(requiredSystem);
                changed = true;
            }
        }

        foreach (var child in entity.Children.ToArray())
        {
            if (!ReferenceEquals(child.Parent, entity) || !ReferenceEquals(child.CurrentScene, scene))
            {
                entity.RemoveEntity(child);
                entity.AddEntity(child);
                changed = true;
                continue;
            }

            if (RepairEntityTree(scene, child))
            {
                changed = true;
            }
        }

        return changed;
    }

    private static bool RepairSystems(Scene scene)
    {
        var changed = false;
        var seenLimitedSystemTypes = new HashSet<Type>();

        foreach (var system in scene.Systems.ToArray())
        {
            var metadata = GetSystemMetadata(system.GetType());

            if (metadata.LimitToOne && !seenLimitedSystemTypes.Add(system.GetType()))
            {
                scene.RemoveSystem(system);
                changed = true;
                continue;
            }

            foreach (var requiredSystemType in metadata.RequiredSystemTypes)
            {
                if (scene.Systems.Any(existing => existing.GetType() == requiredSystemType))
                {
                    continue;
                }

                if (!TryCreateSystem(requiredSystemType, out var requiredSystem))
                {
                    continue;
                }

                scene.AddSystem(requiredSystem);
                changed = true;
            }
        }

        return changed;
    }

    private static ComponentMetadata GetComponentMetadata(Type componentType)
    {
        return ComponentMetadataCache.GetOrAdd(componentType, BuildComponentMetadata);
    }

    private static SystemMetadata GetSystemMetadata(Type systemType)
    {
        return SystemMetadataCache.GetOrAdd(systemType, BuildSystemMetadata);
    }

    private static ComponentMetadata BuildComponentMetadata(Type componentType)
    {
        return new ComponentMetadata(
            componentType.GetCustomAttribute<LimitToOneAttribute>(inherit: true) != null,
            componentType
                .GetCustomAttributes<RequireComponentAttribute>(inherit: true)
                .Select(attribute => attribute.ComponentType)
                .Where(IsValidRequiredComponentType)
                .Distinct()
                .ToArray(),
            componentType
                .GetCustomAttributes<RequireSystemAttribute>(inherit: true)
                .Select(attribute => attribute.SystemType)
                .Where(IsValidRequiredSystemType)
                .Distinct()
                .ToArray());
    }

    private static SystemMetadata BuildSystemMetadata(Type systemType)
    {
        return new SystemMetadata(
            systemType.GetCustomAttribute<LimitToOneAttribute>(inherit: true) != null,
            systemType
                .GetCustomAttributes<RequireSystemAttribute>(inherit: true)
                .Select(attribute => attribute.SystemType)
                .Where(IsValidRequiredSystemType)
                .Distinct()
                .ToArray());
    }

    private static bool TryCreateComponent(Type componentType, out Component component)
    {
        component = null!;

        if (!IsValidRequiredComponentType(componentType))
        {
            return false;
        }

        component = (Component)Activator.CreateInstance(componentType)!;
        return true;
    }

    private static bool TryCreateSystem(Type systemType, out SceneSystem system)
    {
        system = null!;

        if (!IsValidRequiredSystemType(systemType))
        {
            return false;
        }

        system = (SceneSystem)Activator.CreateInstance(systemType)!;
        return true;
    }

    private static bool IsValidRequiredComponentType(Type? componentType)
    {
        return componentType is not null
            && componentType.IsClass
            && !componentType.IsAbstract
            && componentType.IsSubclassOf(typeof(Component))
            && HasParameterlessConstructor(componentType);
    }

    private static bool IsValidRequiredSystemType(Type? systemType)
    {
        return systemType is not null
            && systemType.IsClass
            && !systemType.IsAbstract
            && systemType.IsSubclassOf(typeof(SceneSystem))
            && HasParameterlessConstructor(systemType);
    }

    private static bool HasParameterlessConstructor(Type type)
    {
        return type.GetConstructor(BindingFlags.Public | BindingFlags.Instance, binder: null, Type.EmptyTypes, modifiers: null) != null;
    }
}
