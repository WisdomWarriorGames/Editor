using System.Numerics;
using WisdomWarrior.Engine.Core;
using WisdomWarrior.Engine.Core.Components;

namespace WisdomWarrior.Editor.Core.Tests.TestInfrastructure;

public static class TestSceneFactory
{
    public static Scene CreateSceneWithRoot(string sceneName = "Scene1", string rootName = "Root")
    {
        var scene = new Scene { Name = sceneName };
        scene.AddEntity(CreateEntityWithTransform(rootName, Vector2.Zero));
        return scene;
    }

    public static GameEntity CreateEntityWithTransform(string entityName, Vector2 position)
    {
        var entity = new GameEntity { Name = entityName };
        entity.AddComponent(new Transform { Position = position });
        return entity;
    }
}
