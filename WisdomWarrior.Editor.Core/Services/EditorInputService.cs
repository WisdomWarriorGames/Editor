using System.Numerics;

namespace WisdomWarrior.Editor.Core.Services;

public class EditorInputService
{
    public Vector2 MousePosition { get; set; }
    public bool IsLeftMouseDown { get; private set; }
    public bool IsRightMouseDown { get; set; }

    public bool LeftPressedThisFrame { get; private set; }
    public bool LeftReleasedThisFrame { get; private set; }

    public void SetLeftMouseDown(bool isDown)
    {
        if (isDown && !IsLeftMouseDown)
        {
            LeftPressedThisFrame = true;
        }
        else if (!isDown && IsLeftMouseDown)
        {
            LeftReleasedThisFrame = true;
        }

        IsLeftMouseDown = isDown;
    }

    public void AdvanceFrame()
    {
        LeftPressedThisFrame = false;
        LeftReleasedThisFrame = false;
    }

    public bool IsPointerOver(Vector2 targetPosition, float radius)
    {
        var delta = MousePosition - targetPosition;
        return delta.LengthSquared() <= (radius * radius);
    }
}
