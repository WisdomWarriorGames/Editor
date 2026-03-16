using System.Drawing;
using System.Numerics;
using Microsoft.Xna.Framework.Graphics;
using WisdomWarrior.Editor.Core.Helpers;
using WisdomWarrior.Engine.Core.Interfaces;
using WisdomWarrior.Engine.Core.Rendering;
using WisdomWarrior.Engine.MonoGame;

namespace WisdomWarrior.Editor.MonoGame;

public class EditorRenderService : IRenderService
{
    private SpriteBatch? _spriteBatch;
    private TextureManager? _textureManager;
    private readonly HashSet<string> _pendingPreloadPaths = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, bool> _texturePresenceCache = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _resolvedPathCache = new(StringComparer.OrdinalIgnoreCase);

    public void LoadContent(SpriteBatch spriteBatch, TextureManager textureManager)
    {
        _spriteBatch = spriteBatch;
        _textureManager = textureManager;
        FlushPendingPreloads();
    }

    public void Begin(RenderBatchSettings settings)
    {
        if (_spriteBatch == null)
        {
            return;
        }

        var beginArgs = SpriteBatchBeginSettingsConverter.ToMonoGame(settings);
        _spriteBatch.Begin(
            sortMode: beginArgs.SortMode,
            blendState: beginArgs.BlendState,
            samplerState: beginArgs.SamplerState,
            depthStencilState: beginArgs.DepthStencilState,
            rasterizerState: beginArgs.RasterizerState,
            transformMatrix: beginArgs.TransformMatrix);
    }

    public void End()
    {
        _spriteBatch?.End();
    }

    public void PreloadTextures(IEnumerable<string> texturePaths)
    {
        if (texturePaths == null) return;

        var resolvedPaths = ResolvePaths(texturePaths).ToList();

        if (_textureManager == null)
        {
            QueuePendingPreloads(resolvedPaths);
            PrimeTexturePresence(resolvedPaths);
            return;
        }

        _textureManager.PreloadTextures(resolvedPaths);
        PrimeTexturePresence(resolvedPaths);
    }

    public void Draw(string texturePath, Vector2 position, int width, int height, Color color, float rotation, Vector2 scale)
    {
        if (_spriteBatch == null || _textureManager == null) return;

        var resolvedPath = ResolveAndCache(texturePath);
        if (string.IsNullOrWhiteSpace(resolvedPath))
        {
            return;
        }

        var texture = _textureManager.GetTexture(resolvedPath);
        if (texture == null)
        {
            _texturePresenceCache[resolvedPath] = false;
            return;
        }

        _texturePresenceCache[resolvedPath] = true;

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

    public bool CanRenderTexture(string texturePath)
    {
        if (string.IsNullOrWhiteSpace(texturePath))
        {
            return false;
        }

        var resolvedPath = ResolveAndCache(texturePath);
        if (string.IsNullOrWhiteSpace(resolvedPath))
        {
            return false;
        }

        if (_texturePresenceCache.TryGetValue(resolvedPath, out var canRender))
        {
            return canRender;
        }

        var exists = File.Exists(resolvedPath);
        _texturePresenceCache[resolvedPath] = exists;
        return exists;
    }

    public void PrimeTexturePresence(IEnumerable<string> texturePaths)
    {
        if (texturePaths == null)
        {
            return;
        }

        foreach (var resolvedPath in ResolvePaths(texturePaths))
        {
            if (_texturePresenceCache.ContainsKey(resolvedPath))
            {
                continue;
            }

            _texturePresenceCache[resolvedPath] = File.Exists(resolvedPath);
        }
    }

    private void QueuePendingPreloads(IEnumerable<string> texturePaths)
    {
        foreach (var texturePath in texturePaths)
        {
            if (string.IsNullOrWhiteSpace(texturePath)) continue;
            _pendingPreloadPaths.Add(texturePath);
        }
    }

    private void FlushPendingPreloads()
    {
        if (_textureManager == null || _pendingPreloadPaths.Count == 0) return;

        _textureManager.PreloadTextures(ResolvePaths(_pendingPreloadPaths));
        _pendingPreloadPaths.Clear();
    }

    private IEnumerable<string> ResolvePaths(IEnumerable<string> texturePaths)
    {
        foreach (var texturePath in texturePaths)
        {
            if (string.IsNullOrWhiteSpace(texturePath)) continue;

            var resolvedPath = ResolveAndCache(texturePath);
            if (string.IsNullOrWhiteSpace(resolvedPath)) continue;

            yield return resolvedPath;
        }
    }

    private string ResolveAndCache(string texturePath)
    {
        if (string.IsNullOrWhiteSpace(texturePath))
        {
            return string.Empty;
        }

        if (_resolvedPathCache.TryGetValue(texturePath, out var resolvedPath))
        {
            return resolvedPath;
        }

        resolvedPath = AssetHelpers.ResolveAbsoluteAssetPath(texturePath);
        if (string.IsNullOrWhiteSpace(resolvedPath))
        {
            return string.Empty;
        }

        _resolvedPathCache[texturePath] = resolvedPath;
        return resolvedPath;
    }
}
