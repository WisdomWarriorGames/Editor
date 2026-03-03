using Microsoft.Xna.Framework.Graphics;

namespace WisdomWarrior.Editor.MonoGame.Helpers;

public static class TextureHelpers
{
    public static Texture2D? LoadTexture(this GraphicsDevice graphicsDevice, string relativePath)
    {
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, relativePath);
        if (File.Exists(path))
        {
            using var stream = File.OpenRead(path);
            return Texture2D.FromStream(graphicsDevice, stream);
        }

        return null;
    }
}