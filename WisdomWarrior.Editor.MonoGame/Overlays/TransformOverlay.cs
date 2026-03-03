using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WisdomWarrior.Editor.MonoGame.Helpers;
using WisdomWarrior.Editor.MonoGame.Tools;
using WisdomWarrior.Engine.Core.Components;
using Vector2 = System.Numerics.Vector2;

namespace WisdomWarrior.Editor.MonoGame.Overlays;

public class TransformOverlay(ToolContext context) : IEditorOverlay
{
    private Texture2D? _pointGizmo;
    private Vector2 _origin;

    public void Load(GraphicsDevice graphicsDevice)
    {
        _pointGizmo = graphicsDevice.LoadTexture("Gizmos/point-gizmo.png");
        _origin = new Vector2(_pointGizmo.Width / 2f, _pointGizmo.Height / 2f);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (context.SelectedEntity == null) return;
        if (_pointGizmo == null) return;

        var transform = context.SelectedEntity.Components.OfType<Transform>().FirstOrDefault();
        if (transform == null) return;

        var position = new Vector2(transform.Position.X, transform.Position.Y);

        spriteBatch.Draw(
            _pointGizmo,
            position,
            null,
            Color.White,
            0f,
            _origin,
            0.3f,
            SpriteEffects.None,
            0f
        );
    }
}