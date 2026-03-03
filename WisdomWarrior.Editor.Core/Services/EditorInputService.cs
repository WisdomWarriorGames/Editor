using System.Numerics;

namespace WisdomWarrior.Editor.Core.Services;

public class EditorInputService
{
    public Vector2 MousePosition { get; set; }
    public bool IsLeftMouseDown { get; set; }
    public bool IsRightMouseDown { get; set; }

    public bool IsPointerOver(Vector2 targetPosition, float radius)
    {
        return Vector2.Distance(MousePosition, targetPosition) <= radius;
    }
}