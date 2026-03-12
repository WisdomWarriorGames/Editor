using Microsoft.Xna.Framework.Graphics;
using WisdomWarrior.Editor.MonoGame.Tools;

namespace WisdomWarrior.Editor.MonoGame.Overlays;

public class OverlayManager(ToolContext context)
{
    private TransformOverlay _transformOverlay = new(context);
    private MouseHoldOverlay _mouseHoldOverlay = new(context);

    public void Load(GraphicsDevice graphicsDevice)
    {
        _transformOverlay.Load(graphicsDevice);
        _mouseHoldOverlay.Load(graphicsDevice);
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        spriteBatch.Begin(blendState: BlendState.NonPremultiplied);

        _transformOverlay.Draw(spriteBatch);
        
        // Last thing to draw
        _mouseHoldOverlay.Draw(spriteBatch);

        spriteBatch.End();
    }
}