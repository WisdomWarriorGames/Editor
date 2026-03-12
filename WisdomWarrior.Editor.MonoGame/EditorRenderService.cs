using System.Drawing;
using System.Numerics;
using Microsoft.Xna.Framework.Graphics;
using WisdomWarrior.Editor.Core.Helpers;
using WisdomWarrior.Engine.Core.Interfaces;
using WisdomWarrior.Engine.MonoGame;

namespace WisdomWarrior.Editor.MonoGame;

public class EditorRenderService : IRenderService
{
    private SpriteBatch? _spriteBatch;
    private TextureManager? _textureManager;

    public void LoadContent(SpriteBatch spriteBatch, TextureManager textureManager)
    {
        _spriteBatch = spriteBatch;
        _textureManager = textureManager;
    }

    public void Begin()
    {
        _spriteBatch?.Begin();
    }

    public void End()
    {
        _spriteBatch?.End();
    }

    public void Draw(string texturePath, Vector2 position, int width, int height, Color color, float rotation, Vector2 scale)
    {
        if (_spriteBatch == null || _textureManager == null) return;

        var resolvedPath = AssetHelpers.ResolveAbsoluteAssetPath(texturePath);
        var texture = _textureManager.GetTexture(resolvedPath);
        if (texture == null) return;

        var mgColor = new Microsoft.Xna.Framework.Color(color.R, color.G, color.B, color.A);
        var mgPos = new Microsoft.Xna.Framework.Vector2(position.X, position.Y);
        var scaleX = width / (float)texture.Width * scale.X;
        var scaleY = height / (float)texture.Height * scale.Y;
        var mgScale = new Microsoft.Xna.Framework.Vector2(scaleX, scaleY);
        var origin = new Microsoft.Xna.Framework.Vector2(texture.Width / 2f, texture.Height / 2f);

        _spriteBatch.Draw(
            texture,
            mgPos,
            null,
            mgColor,
            rotation,
            origin,
            mgScale,
            SpriteEffects.None,
            0.0f);
    }
}
