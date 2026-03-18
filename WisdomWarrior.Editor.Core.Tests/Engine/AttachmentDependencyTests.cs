using WisdomWarrior.Engine.Core;
using WisdomWarrior.Engine.Core.Attributes;
using WisdomWarrior.Engine.Core.Systems;

namespace WisdomWarrior.Editor.Core.Tests.Engine;

public class AttachmentDependencyTests
{
    [Fact]
    public void AddComponent_DoesNotAutoAttachRequiredComponentsOrSystems()
    {
        var scene = new Scene { Name = "AttachmentScene" };
        var entity = new GameEntity { Name = "Player" };
        scene.AddEntity(entity);

        entity.AddComponent(new RequiresComponentAndSystemComponent());

        Assert.Single(entity.Components.OfType<RequiresComponentAndSystemComponent>());
        Assert.Empty(entity.Components.OfType<SiblingDependencyComponent>());
        Assert.Empty(scene.Systems.OfType<FirstRequiredSystem>());
        Assert.Empty(scene.GetEntitiesWith<SiblingDependencyComponent>());
    }

    [Fact]
    public void AddSystem_DoesNotAutoAttachRequiredSystems()
    {
        var scene = new Scene { Name = "SceneSystems" };

        scene.AddSystem(new RequiresBehaviourSystem());

        Assert.Single(scene.Systems.OfType<RequiresBehaviourSystem>());
        Assert.Empty(scene.Systems.OfType<RequiredBehaviourSystem>());
        Assert.Empty(scene.BehaviourSystems.OfType<RequiredBehaviourSystem>());
    }

    [Fact]
    public void SceneInitialize_DoesNotResolveDependenciesForExistingAttachments()
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

        Assert.Single(entity.Components.OfType<RequiresComponentAndSystemComponent>());
        Assert.Empty(entity.Components.OfType<SiblingDependencyComponent>());
        Assert.Empty(scene.Systems.OfType<FirstRequiredSystem>());
    }

    [Fact]
    public void SceneUpdate_DoesNotResolveDependenciesForAttachmentsAddedOutsideAttachApi()
    {
        var scene = new Scene { Name = "UpdateScene" };
        var entity = new GameEntity { Name = "Player" };
        scene.AddEntity(entity);
        scene.Initialize();

        entity.Components.Add(new RequiresComponentAndSystemComponent());
        scene.Update();

        Assert.Single(entity.Components.OfType<RequiresComponentAndSystemComponent>());
        Assert.Empty(entity.Components.OfType<SiblingDependencyComponent>());
        Assert.Empty(scene.Systems.OfType<FirstRequiredSystem>());
        Assert.Empty(scene.GetEntitiesWith<SiblingDependencyComponent>());
    }

    [Fact]
    public void AddComponent_LimitToOne_PreventsDuplicateExactTypes()
    {
        var entity = new GameEntity { Name = "Player" };

        entity.AddComponent(new LimitedComponent());
        entity.AddComponent(new LimitedComponent());

        Assert.Single(entity.Components.OfType<LimitedComponent>());
    }

    [Fact]
    public void AddSystem_LimitToOne_PreventsDuplicateExactTypes()
    {
        var scene = new Scene { Name = "LimitSystems" };

        scene.AddSystem(new LimitedSystem());
        scene.AddSystem(new LimitedSystem());

        Assert.Single(scene.Systems.OfType<LimitedSystem>());
    }

    [Fact]
    public void AddComponent_WithoutLimitToOne_StillAllowsDuplicates()
    {
        var entity = new GameEntity { Name = "Player" };

        entity.AddComponent(new MultiAllowedComponent());
        entity.AddComponent(new MultiAllowedComponent());

        Assert.Equal(2, entity.Components.OfType<MultiAllowedComponent>().Count());
    }

    [Fact]
    public void AddSystem_WithoutLimitToOne_StillAllowsDuplicates()
    {
        var scene = new Scene { Name = "MultiSystems" };

        scene.AddSystem(new MultiAllowedSystem());
        scene.AddSystem(new MultiAllowedSystem());

        Assert.Equal(2, scene.Systems.OfType<MultiAllowedSystem>().Count());
    }

    [Fact]
    public void AddingTypesWithDependencyAttributes_DoesNotThrow_AndLeavesOriginalAttachmentsOnly()
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

    [RequireComponent(typeof(SiblingDependencyComponent))]
    public sealed class RequiresSiblingComponent : Component
    {
    }

    public sealed class SiblingDependencyComponent : Component
    {
    }

    [LimitToOne]
    public sealed class LimitedComponent : Component
    {
        public string? Name { get; set; }
    }

    public sealed class MultiAllowedComponent : Component
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

    [LimitToOne]
    public sealed class LimitedSystem : BehaviourSystem
    {
        public string? Name { get; set; }

        public override void OnStart()
        {
        }

        public override void Update()
        {
        }
    }

    public sealed class MultiAllowedSystem : BehaviourSystem
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

    [LimitToOne]
    public sealed class LimitedDependencyComponent : Component
    {
    }

    [RequireComponent(typeof(LimitedDependencyComponent))]
    [RequireSystem(typeof(LimitedSystem))]
    public sealed class RequiresLimitedComponent : Component
    {
    }

    [RequireSystem(typeof(LimitedSystem))]
    public sealed class RequiresLimitedSystem : BehaviourSystem
    {
        public override void OnStart()
        {
        }

        public override void Update()
        {
        }
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
