using System.Numerics;
using WisdomWarrior.Engine.Core;
using WisdomWarrior.Engine.Core.Components;

namespace WisdomWarrior.Editor.MonoGame.Selection;

public class SceneSpriteHitTestService
{
    public GameEntity? HitTest(Scene? scene, Vector2 point, Func<string, bool>? isTextureRenderable = null)
    {
        if (scene == null)
        {
            return null;
        }

        GameEntity? lastHit = null;

        foreach (var entity in scene.GetEntitiesWith<Sprite>())
        {
            var sprite = entity.GetComponent<Sprite>();
            var transform = entity.GetComponent<Transform>();

            if (sprite == null || transform == null)
            {
                continue;
            }

            if (!IsSpriteRenderable(sprite, transform, isTextureRenderable))
            {
                continue;
            }

            if (ContainsPoint(point, sprite, transform))
            {
                // Keep the last match so overlap behavior follows render iteration order.
                lastHit = entity;
            }
        }

        return lastHit;
    }

    private static bool IsSpriteRenderable(
        Sprite sprite,
        Transform transform,
        Func<string, bool>? isTextureRenderable)
    {
        var assetPath = sprite.Image?.AssetPath;
        if (string.IsNullOrWhiteSpace(assetPath))
        {
            return false;
        }

        if (sprite.Size.W <= 0 || sprite.Size.H <= 0)
        {
            return false;
        }

        if (Math.Abs(transform.Scale.X) <= float.Epsilon || Math.Abs(transform.Scale.Y) <= float.Epsilon)
        {
            return false;
        }

        if (isTextureRenderable != null && !isTextureRenderable(assetPath))
        {
            return false;
        }

        return true;
    }

    private static bool ContainsPoint(Vector2 point, Sprite sprite, Transform transform)
    {
        var width = sprite.Size.W * Math.Abs(transform.Scale.X);
        var height = sprite.Size.H * Math.Abs(transform.Scale.Y);

        var left = transform.Position.X - (width / 2f);
        var right = transform.Position.X + (width / 2f);
        var top = transform.Position.Y - (height / 2f);
        var bottom = transform.Position.Y + (height / 2f);

        return point.X >= left
               && point.X <= right
               && point.Y >= top
               && point.Y <= bottom;
    }
}
