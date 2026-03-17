using System.Numerics;
using WisdomWarrior.Engine.Core.Components;

namespace WisdomWarrior.Editor.MonoGame.Tools;

public class TranslateTool : IEditorTool
{
    private bool _isDragging;
    private Vector2 _grabOffset;

    public void Update(ToolContext context)
    {
        var input = context.Input;
        var entity = context.SelectedEntity;

        if (entity == null)
        {
            _isDragging = false;
            return;
        }

        var transform = entity.Components.OfType<Transform>().FirstOrDefault();
        if (transform == null)
        {
            _isDragging = false;
            return;
        }

        var sprite = entity.GetComponent<Sprite>();
        var isHoveringSprite = IsPointerOverSprite(input.MousePosition, transform, sprite);
        var isHoveringGizmo = input.IsPointerOver(transform.Position, radius: 15f);

        if (input.LeftPressedThisFrame && (isHoveringSprite || isHoveringGizmo))
        {
            _isDragging = true;
            _grabOffset = transform.Position - input.MousePosition;
        }

        if (_isDragging && input.IsLeftMouseDown)
        {
            transform.Position = input.MousePosition + _grabOffset;
        }

        if (input.LeftReleasedThisFrame || !input.IsLeftMouseDown)
        {
            _isDragging = false;
        }
    }

    private static bool IsPointerOverSprite(Vector2 pointerPosition, Transform transform, Sprite? sprite)
    {
        if (sprite == null)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(sprite.Image?.AssetPath))
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

        var width = sprite.Size.W * Math.Abs(transform.Scale.X);
        var height = sprite.Size.H * Math.Abs(transform.Scale.Y);

        var left = transform.Position.X - (width / 2f);
        var right = transform.Position.X + (width / 2f);
        var top = transform.Position.Y - (height / 2f);
        var bottom = transform.Position.Y + (height / 2f);

        return pointerPosition.X >= left
               && pointerPosition.X <= right
               && pointerPosition.Y >= top
               && pointerPosition.Y <= bottom;
    }
}
