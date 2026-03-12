using Microsoft.Xna.Framework.Graphics;
using WisdomWarrior.Engine.MonoGame;

namespace WisdomWarrior.Editor.Core.Tests.Engine;

public class TextureManagerLifecycleTests
{
    [Fact]
    public void ReloadTexture_ReplacingCachedTexture_DisposesPreviousResource()
    {
        var createdResources = new List<FakeTextureResource>();
        var manager = new TextureManager(path =>
        {
            var resource = new FakeTextureResource();
            createdResources.Add(resource);
            return resource;
        });

        var texturePath = BuildVirtualTexturePath("replace.png");

        manager.LoadTexture(texturePath);
        manager.ReloadTexture(texturePath);

        Assert.Equal(2, createdResources.Count);
        Assert.Equal(1, createdResources[0].DisposeCount);
        Assert.Equal(0, createdResources[1].DisposeCount);
        Assert.Equal(1, manager.CachedTextureCount);

        manager.Dispose();
    }

    [Fact]
    public void ClearCache_DisposesAllCachedTextures()
    {
        var createdResources = new List<FakeTextureResource>();
        var manager = new TextureManager(path =>
        {
            var resource = new FakeTextureResource();
            createdResources.Add(resource);
            return resource;
        });

        manager.LoadTexture(BuildVirtualTexturePath("a.png"));
        manager.LoadTexture(BuildVirtualTexturePath("b.png"));
        Assert.Equal(2, manager.CachedTextureCount);

        manager.ClearCache();

        Assert.Equal(0, manager.CachedTextureCount);
        Assert.All(createdResources, r => Assert.Equal(1, r.DisposeCount));

        manager.Dispose();
    }

    [Fact]
    public void Dispose_DisposesCachedTexturesAndIsIdempotent()
    {
        var createdResources = new List<FakeTextureResource>();
        var manager = new TextureManager(path =>
        {
            var resource = new FakeTextureResource();
            createdResources.Add(resource);
            return resource;
        });

        manager.LoadTexture(BuildVirtualTexturePath("shutdown.png"));
        Assert.Equal(1, manager.CachedTextureCount);

        manager.Dispose();
        manager.Dispose();

        Assert.Equal(0, manager.CachedTextureCount);
        Assert.Single(createdResources);
        Assert.Equal(1, createdResources[0].DisposeCount);
    }

    [Fact]
    public void ReloadingSamePathRepeatedly_DoesNotCauseUnboundedLiveResources()
    {
        var createdResources = new List<FakeTextureResource>();
        var manager = new TextureManager(path =>
        {
            var resource = new FakeTextureResource();
            createdResources.Add(resource);
            return resource;
        });

        var texturePath = BuildVirtualTexturePath("loop.png");
        for (var i = 0; i < 10; i++)
        {
            manager.ReloadTexture(texturePath);
        }

        Assert.Equal(10, createdResources.Count);
        Assert.Equal(1, manager.CachedTextureCount);
        Assert.Equal(1, createdResources.Count(r => r.DisposeCount == 0));
        Assert.Equal(9, createdResources.Count(r => r.DisposeCount == 1));

        manager.Dispose();
    }

    [Fact]
    public void ReloadFailure_DoesNotCorruptExistingCacheEntry()
    {
        var createdResources = new List<FakeTextureResource>();
        var shouldFail = false;

        var manager = new TextureManager(path =>
        {
            if (shouldFail) throw new InvalidOperationException("Simulated load failure");

            var resource = new FakeTextureResource();
            createdResources.Add(resource);
            return resource;
        });

        var texturePath = BuildVirtualTexturePath("failure.png");
        manager.LoadTexture(texturePath);
        Assert.Single(createdResources);
        Assert.Equal(1, manager.CachedTextureCount);

        shouldFail = true;
        var result = manager.ReloadTexture(texturePath);

        Assert.Null(result);
        Assert.Equal(1, manager.CachedTextureCount);
        Assert.Single(createdResources);
        Assert.Equal(0, createdResources[0].DisposeCount);

        manager.Dispose();
    }

    [Fact]
    public void InitialLoadFailure_DoesNotCreateCacheEntry()
    {
        var manager = new TextureManager(path => throw new InvalidOperationException("Simulated load failure"));

        var result = manager.LoadTexture(BuildVirtualTexturePath("missing.png"));

        Assert.Null(result);
        Assert.Equal(0, manager.CachedTextureCount);

        manager.Dispose();
    }

    private static string BuildVirtualTexturePath(string fileName)
    {
        return Path.Combine(Path.GetTempPath(), "WisdomWarrior.TextureTests", fileName);
    }

    private sealed class FakeTextureResource : ITextureResource
    {
        public Texture2D? Texture => null;
        public int DisposeCount { get; private set; }

        public void Dispose()
        {
            DisposeCount++;
        }
    }
}
