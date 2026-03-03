using Microsoft.Xna.Framework.Graphics;

namespace WisdomWarrior.Editor.MonoGame.Overlays;

public interface IEditorOverlay
{
    void Load(GraphicsDevice graphicsDevice);
    void Draw(SpriteBatch spriteBatch);
}