using WisdomWarrior.Engine.Core;
using WisdomWarrior.Engine.Core.Attributes;
using WisdomWarrior.Engine.Core.Systems;

namespace WisdomWarrior.Editor.Core.Tests.Engine;

public class AttachmentDependencyTests
{
    [Fact]
    public void AddComponent_AutoAttachesRequiredComponent_AndMaintainsEntityCacheAndParent()
    {
        var scene = new Scene { Name = "AttachmentScene" };
        var entity = new GameEntity { Name = "Player" };
        scene.AddEntity(entity);

        entity.AddComponent(new RequiresSiblingComponent());

        var requiredComponent = Assert.Single(entity.Components.OfType<SiblingDependencyComponent>());
        Assert.Same(entity, requiredComponent.Parent);
        Assert.Single(scene.GetEntitiesWith<SiblingDependencyComponent>());
        Assert.Same(entity, scene.GetEntitiesWith<SiblingDependencyComponent>().Single());
    }

    [Fact]
    public void AddComponent_AutoAttachesRequiredSystems_AndSupportsMultipleDependencies()
    {
        var scene = new Scene { Name = "SystemDependencyScene" };
        var entity = new GameEntity { Name = "Player" };
        scene.AddEntity(entity);

        entity.AddComponent(new RequiresMultipleSystemsComponent());

        Assert.Contains(scene.Systems, system => system.GetType() == typeof(FirstRequiredSystem));
        Assert.Contains(scene.Systems, system => system.GetType() == typeof(SecondRequiredSystem));
        Assert.All(scene.Systems, system => Assert.Same(scene, system.Scene));
    }

    [Fact]
    public void AddSystem_AutoAttachesRequiredSystems_AndUpdatesTypedCaches()
    {
        var scene = new Scene { Name = "SceneSystems" };

        scene.AddSystem(new RequiresBehaviourSystem());

        Assert.Contains(scene.Systems, system => system.GetType() == typeof(RequiredBehaviourSystem));
        Assert.Contains(scene.BehaviourSystems, system => system.GetType() == typeof(RequiredBehaviourSystem));
        Assert.All(scene.Systems, system => Assert.Same(scene, system.Scene));
    }

    [Fact]
    public void ResolveDependencies_ResolvesNestedDependencyChains_ToAStableGraph()
    {
        var scene = new Scene { Name = "ChainScene" };
        var entity = new GameEntity { Name = "Player" };
        scene.AddEntity(entity);

        entity.AddComponent(new RootChainComponent());
        scene.AddSystem(new RootChainSystem());

        Assert.Single(entity.Components.OfType<MidChainComponent>());
        Assert.Single(entity.Components.OfType<LeafChainComponent>());
        Assert.Contains(scene.Systems, system => system.GetType() == typeof(MidChainSystem));
        Assert.Contains(scene.Systems, system => system.GetType() == typeof(LeafChainSystem));
    }

    [Fact]
    public void ResolveDependencies_CyclicDependencies_DoNotCreateDuplicates()
    {
        var scene = new Scene { Name = "CycleScene" };
        var entity = new GameEntity { Name = "Player" };
        scene.AddEntity(entity);

        entity.AddComponent(new ComponentCycleA());
        scene.AddSystem(new SystemCycleA());
        scene.ResolveDependencies();

        Assert.Single(entity.Components.OfType<ComponentCycleA>());
        Assert.Single(entity.Components.OfType<ComponentCycleB>());
        Assert.Single(scene.Systems.Where(system => system.GetType() == typeof(SystemCycleA)));
        Assert.Single(scene.Systems.Where(system => system.GetType() == typeof(SystemCycleB)));
    }

    [Fact]
    public void ResolveDependencies_UsesExactTypeMatching_WhenCheckingExistingComponents()
    {
        var scene = new Scene { Name = "ExactTypeScene" };
        var entity = new GameEntity { Name = "Player" };
        scene.AddEntity(entity);

        entity.AddComponent(new DerivedDependencyComponent());
        entity.AddComponent(new RequiresBaseDependencyComponent());

        Assert.Single(entity.Components.Where(component => component.GetType() == typeof(BaseDependencyComponent)));
        Assert.Single(entity.Components.Where(component => component.GetType() == typeof(DerivedDependencyComponent)));
    }

    [Fact]
    public void ResolveDependencies_IgnoresRequireComponentAppliedToSystems()
    {
        var scene = new Scene { Name = "IgnoredComponentAttributeScene" };

        scene.AddSystem(new SystemWithIgnoredComponentRequirement());

        Assert.Single(scene.Systems);
        Assert.Empty(scene.Entities);
    }

    [Fact]
    public void ResolveDependencies_IgnoresInvalidDependencyTargets()
    {
        var scene = new Scene { Name = "InvalidDependencyScene" };
        var entity = new GameEntity { Name = "Player" };
        scene.AddEntity(entity);

        var addComponentError = Record.Exception(() => entity.AddComponent(new ComponentWithInvalidDependencies()));
        var addSystemError = Record.Exception(() => scene.AddSystem(new SystemWithInvalidDependencies()));

        Assert.Null(addComponentError);
        Assert.Null(addSystemError);
        Assert.Single(entity.Components.OfType<ComponentWithInvalidDependencies>());
        Assert.Single(scene.Systems.OfType<SystemWithInvalidDependencies>());
    }

    [Fact]
    public void SceneInitialize_ResolvesDependenciesForExistingAttachments()
    {
        var entity = new GameEntity
        {
            Name = "Player",
            Components = [new RequiresComponentAndSystemComponent()]
        };

        var scene = new Scene
        {
            Name = "InitializeScene",
            Entities = [entity]
        };

        scene.Initialize();

        var requiredComponent = Assert.Single(entity.Components.OfType<SiblingDependencyComponent>());
        Assert.Same(entity, requiredComponent.Parent);
        Assert.Contains(scene.Systems, system => system.GetType() == typeof(FirstRequiredSystem));
    }

    [Fact]
    public void SceneUpdate_ResolvesDependenciesForExistingAttachmentsAddedOutsideAttachApi()
    {
        var scene = new Scene { Name = "UpdateScene" };
        var entity = new GameEntity { Name = "Player" };
        scene.AddEntity(entity);
        scene.Initialize();

        entity.Components.Add(new RequiresComponentAndSystemComponent());
        scene.Update();

        Assert.Single(entity.Components.OfType<SiblingDependencyComponent>());
        Assert.Contains(scene.Systems, system => system.GetType() == typeof(FirstRequiredSystem));
        Assert.Single(scene.GetEntitiesWith<SiblingDependencyComponent>());
    }

    [RequireComponent(typeof(SiblingDependencyComponent))]
    public sealed class RequiresSiblingComponent : Component
    {
    }

    public sealed class SiblingDependencyComponent : Component
    {
    }

    [RequireSystem(typeof(FirstRequiredSystem))]
    [RequireSystem(typeof(SecondRequiredSystem))]
    public sealed class RequiresMultipleSystemsComponent : Component
    {
    }

    [RequireComponent(typeof(SiblingDependencyComponent))]
    [RequireSystem(typeof(FirstRequiredSystem))]
    public sealed class RequiresComponentAndSystemComponent : Component
    {
    }

    public sealed class FirstRequiredSystem : BehaviourSystem
    {
        public override void OnStart()
        {
        }

        public override void Update()
        {
        }
    }

    public sealed class SecondRequiredSystem : BehaviourSystem
    {
        public override void OnStart()
        {
        }

        public override void Update()
        {
        }
    }

    [RequireSystem(typeof(RequiredBehaviourSystem))]
    public sealed class RequiresBehaviourSystem : BehaviourSystem
    {
        public override void OnStart()
        {
        }

        public override void Update()
        {
        }
    }

    public sealed class RequiredBehaviourSystem : BehaviourSystem
    {
        public override void OnStart()
        {
        }

        public override void Update()
        {
        }
    }

    [RequireComponent(typeof(MidChainComponent))]
    public sealed class RootChainComponent : Component
    {
    }

    [RequireComponent(typeof(LeafChainComponent))]
    public sealed class MidChainComponent : Component
    {
    }

    public sealed class LeafChainComponent : Component
    {
    }

    [RequireSystem(typeof(MidChainSystem))]
    public sealed class RootChainSystem : BehaviourSystem
    {
        public override void OnStart()
        {
        }

        public override void Update()
        {
        }
    }

    [RequireSystem(typeof(LeafChainSystem))]
    public sealed class MidChainSystem : BehaviourSystem
    {
        public override void OnStart()
        {
        }

        public override void Update()
        {
        }
    }

    public sealed class LeafChainSystem : BehaviourSystem
    {
        public override void OnStart()
        {
        }

        public override void Update()
        {
        }
    }

    [RequireComponent(typeof(ComponentCycleB))]
    public sealed class ComponentCycleA : Component
    {
    }

    [RequireComponent(typeof(ComponentCycleA))]
    public sealed class ComponentCycleB : Component
    {
    }

    [RequireSystem(typeof(SystemCycleB))]
    public sealed class SystemCycleA : BehaviourSystem
    {
        public override void OnStart()
        {
        }

        public override void Update()
        {
        }
    }

    [RequireSystem(typeof(SystemCycleA))]
    public sealed class SystemCycleB : BehaviourSystem
    {
        public override void OnStart()
        {
        }

        public override void Update()
        {
        }
    }

    public class BaseDependencyComponent : Component
    {
    }

    public sealed class DerivedDependencyComponent : BaseDependencyComponent
    {
    }

    [RequireComponent(typeof(BaseDependencyComponent))]
    public sealed class RequiresBaseDependencyComponent : Component
    {
    }

    [RequireComponent(typeof(SiblingDependencyComponent))]
    public sealed class SystemWithIgnoredComponentRequirement : BehaviourSystem
    {
        public override void OnStart()
        {
        }

        public override void Update()
        {
        }
    }

    [RequireComponent(typeof(string))]
    [RequireComponent(typeof(AbstractInvalidComponent))]
    [RequireComponent(typeof(NoDefaultConstructorComponent))]
    [RequireSystem(typeof(string))]
    [RequireSystem(typeof(AbstractInvalidSystem))]
    [RequireSystem(typeof(NoDefaultConstructorSystem))]
    public sealed class ComponentWithInvalidDependencies : Component
    {
    }

    [RequireSystem(typeof(string))]
    [RequireSystem(typeof(AbstractInvalidSystem))]
    [RequireSystem(typeof(NoDefaultConstructorSystem))]
    public sealed class SystemWithInvalidDependencies : BehaviourSystem
    {
        public override void OnStart()
        {
        }

        public override void Update()
        {
        }
    }

    public abstract class AbstractInvalidComponent : Component
    {
    }

    public sealed class NoDefaultConstructorComponent(string name) : Component
    {
        public string Name { get; } = name;
    }

    public abstract class AbstractInvalidSystem : BehaviourSystem
    {
    }

    public sealed class NoDefaultConstructorSystem(string name) : BehaviourSystem
    {
        public string Name { get; } = name;

        public override void OnStart()
        {
        }

        public override void Update()
        {
        }
    }
}
