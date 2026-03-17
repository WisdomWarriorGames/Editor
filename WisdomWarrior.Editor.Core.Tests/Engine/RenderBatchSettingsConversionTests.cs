using System.Numerics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using WisdomWarrior.Engine.Core.Rendering;
using WisdomWarrior.Engine.MonoGame;

namespace WisdomWarrior.Editor.Core.Tests.Engine;

public class RenderBatchSettingsConversionTests
{
    [Fact]
    public void ToMonoGame_DefaultSettings_MapToCurrentSpriteBatchDefaults()
    {
        var converted = SpriteBatchBeginSettingsConverter.ToMonoGame(new RenderBatchSettings());

        Assert.Equal(SpriteSortMode.Deferred, converted.SortMode);
        Assert.Same(BlendState.AlphaBlend, converted.BlendState);
        Assert.Same(SamplerState.LinearClamp, converted.SamplerState);
        Assert.Same(DepthStencilState.None, converted.DepthStencilState);
        Assert.Same(RasterizerState.CullCounterClockwise, converted.RasterizerState);
        Assert.Null(converted.TransformMatrix);
    }

    [Fact]
    public void ToMonoGame_CustomSettings_MapAllConfiguredValues()
    {
        var settings = new RenderBatchSettings
        {
            SortMode = RenderBatchSortMode.Texture,
            BlendMode = RenderBlendMode.Additive,
            SamplerMode = RenderSamplerMode.PointWrap,
            DepthStencilMode = RenderDepthStencilMode.DepthRead,
            RasterizerMode = RenderRasterizerMode.CullNone,
            UseTransformMatrix = true,
            TransformMatrix = new Matrix4x4(
                1, 2, 3, 4,
                5, 6, 7, 8,
                9, 10, 11, 12,
                13, 14, 15, 16)
        };

        var converted = SpriteBatchBeginSettingsConverter.ToMonoGame(settings);

        Assert.Equal(SpriteSortMode.Texture, converted.SortMode);
        Assert.Same(BlendState.Additive, converted.BlendState);
        Assert.Same(SamplerState.PointWrap, converted.SamplerState);
        Assert.Same(DepthStencilState.DepthRead, converted.DepthStencilState);
        Assert.Same(RasterizerState.CullNone, converted.RasterizerState);
        Assert.True(converted.TransformMatrix.HasValue);

        var matrix = converted.TransformMatrix!.Value;
        Assert.Equal(1f, matrix.M11);
        Assert.Equal(6f, matrix.M22);
        Assert.Equal(11f, matrix.M33);
        Assert.Equal(16f, matrix.M44);
        Assert.Equal(13f, matrix.M41);
        Assert.Equal(14f, matrix.M42);
        Assert.Equal(15f, matrix.M43);
    }
}
