using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WisdomWarrior.Engine.Core;
using WisdomWarrior.Engine.Core.Components;
using Color = Microsoft.Xna.Framework.Color;

namespace WisdomWarrior.Editor.MonoGame.EditorUI;

public class EditorViewportRenderer
{
    private Texture2D? _gizmoTexture;
    private Texture2D? _cursorTexture;
    private Texture2D? _handTexture;
    private Vector2 _origin;

    public EditorViewportRenderer(GraphicsDevice graphicsDevice)
    {
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Gizmos/point-gizmo.png");
        if (File.Exists(path))
        {
            using var stream = File.OpenRead(path);
            _gizmoTexture = Texture2D.FromStream(graphicsDevice, stream);
            _origin = new Vector2(_gizmoTexture.Width / 2f, _gizmoTexture.Height / 2f);
        }

        var cursorPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Gizmos/cursor.png");
        if (File.Exists(cursorPath))
        {
            using var stream = File.OpenRead(cursorPath);
            _cursorTexture = Texture2D.FromStream(graphicsDevice, stream);
        }

        var handPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Gizmos/hand.png");
        if (File.Exists(handPath))
        {
            using var stream = File.OpenRead(handPath);
            _handTexture = Texture2D.FromStream(graphicsDevice, stream);
        }
    }

    public bool IsMouseOver(Vector2 mousePos, GameEntity? entity, float totalSeconds)
    {
        if (entity == null) return false;

        var transform = entity.Components.OfType<Transform>().FirstOrDefault();
        if (transform == null) return false;

        var orbPos = new Vector2(transform.Position.X, transform.Position.Y);

        return Vector2.Distance(mousePos, orbPos) < 15f;
    }

    public void Draw(SpriteBatch spriteBatch, Vector2? localMousePos, bool isHovering, float scale)
    {
        if (localMousePos == null) return;

        var activeCursor = isHovering ? _handTexture : _cursorTexture;
        var position = isHovering ? localMousePos.Value - new Vector2(_handTexture.Width / 2, 0) : localMousePos.Value;
        
        spriteBatch.Draw(
            activeCursor,
            position,
            null,
            Color.White,
            0f,
            Vector2.Zero,
            scale / 4, // <-- The magic happens here!
            SpriteEffects.None,
            0f
        );
    }

    public void Draw(SpriteBatch spriteBatch, GameEntity? selectedEntity)
    {
        if (_gizmoTexture == null) return;
        if (selectedEntity == null) return;

        var transform = selectedEntity.Components.OfType<Transform>().FirstOrDefault();
        if (transform == null) return;

        var position = new Vector2(transform.Position.X, transform.Position.Y);

        spriteBatch.Draw(
            _gizmoTexture,
            position,
            null,
            Color.White,
            0f,
            _origin,
            0.3f, // Drawn at full 32x32 size
            SpriteEffects.None,
            0f
        );
    }
}