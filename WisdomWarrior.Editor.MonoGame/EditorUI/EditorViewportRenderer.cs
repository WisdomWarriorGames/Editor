using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WisdomWarrior.Engine.Core;
using WisdomWarrior.Engine.Core.Components;
using Color = Microsoft.Xna.Framework.Color;

namespace WisdomWarrior.Editor.MonoGame.EditorUI;

public class EditorViewportRenderer
{
    private readonly Texture2D _circleTexture;

    public EditorViewportRenderer(GraphicsDevice graphicsDevice)
    {
        int size = 64;
        _circleTexture = new Texture2D(graphicsDevice, size, size);
        Color[] data = new Color[size * size];
        float center = size / 2f;
        float radius = size / 2f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                // Anti-aliasing math: make the edges soft
                float alpha = MathHelper.Clamp((radius - distance) / 2f, 0, 1);
                data[y * size + x] = Color.White * alpha;
            }
        }

        _circleTexture.SetData(data);
    }

    public bool IsMouseOver(Vector2 mousePos, GameEntity? entity, float totalSeconds)
    {
        if (entity == null) return false;

        var transform = entity.Components.OfType<Transform>().FirstOrDefault();
        if (transform == null) return false;

        // Calculate the exact same bobbing position we use in Draw
        var bob = MathF.Sin(totalSeconds * 3f) * 5f;
        var orbPos = new Vector2(transform.Position.X, transform.Position.Y + bob);

        // Check distance (15 pixels is a good 'hitbox' for a 16px circle)
        return Vector2.Distance(mousePos, orbPos) < 15f;
    }

    public void Draw(SpriteBatch spriteBatch, GameEntity? selectedEntity, GameTime gameTime)
    {
        if (selectedEntity == null) return;

        var transform = selectedEntity.Components.OfType<Transform>().FirstOrDefault();
        if (transform == null) return;

        var position = new Vector2(transform.Position.X, transform.Position.Y);

        spriteBatch.Draw(
            _circleTexture,
            position,
            null,
            Color.Yellow,
            0f, // Rotation
            new Vector2(32, 32),
            0.25f,
            SpriteEffects.None,
            0f
        );
    }
}