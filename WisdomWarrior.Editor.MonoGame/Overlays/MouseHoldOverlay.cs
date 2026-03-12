using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WisdomWarrior.Editor.MonoGame.Helpers;
using WisdomWarrior.Editor.MonoGame.Tools;
using Vector2 = System.Numerics.Vector2;

namespace WisdomWarrior.Editor.MonoGame.Overlays;

public class MouseHoldOverlay(ToolContext context) : IEditorOverlay
{
    private Texture2D? _gizmo;
    private Vector2 _origin;

    public void Load(GraphicsDevice graphicsDevice)
    {
        _gizmo = graphicsDevice.LoadTexture("Gizmos/move-gizmo.png");
        _origin = new Vector2(_gizmo.Width / 2f, _gizmo.Height / 2f);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (context.Input.IsLeftMouseDown)
        {
            spriteBatch.Draw(
                _gizmo,
                context.Input.MousePosition,
                null,
                Color.White,
                0f,
                _origin,
                0.5f,
                SpriteEffects.None,
                0f
            );
        }
    }
}