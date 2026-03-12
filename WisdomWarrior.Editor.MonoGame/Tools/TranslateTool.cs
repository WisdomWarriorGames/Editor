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

        if (entity == null) return;

        var transform = entity.Components.OfType<Transform>().FirstOrDefault();
        if (transform == null) return;

        var entityPos = transform.Position;
        var isHoveringGizmo = input.IsPointerOver(entityPos, radius: 15f);

        if (input.IsLeftMouseDown && isHoveringGizmo && !_isDragging)
        {
            _isDragging = true;
            _grabOffset = transform.Position - input.MousePosition;
        }
        else if (input.IsLeftMouseDown && _isDragging)
        {
            var position = input.MousePosition + _grabOffset;
            transform.Position = position;
        }
        else
        {
            _isDragging = false;
        }
    }
}