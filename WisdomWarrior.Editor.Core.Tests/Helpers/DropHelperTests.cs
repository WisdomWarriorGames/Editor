using WisdomWarrior.Editor.Core.Helpers;
using WisdomWarrior.Editor.Core.Models;

namespace WisdomWarrior.Editor.Core.Tests.Helpers;

public class DropHelperTests
{
    [Fact]
    public void GetPayloadItems_SingleItem_ReturnsSingleTypedItem()
    {
        var item = new TestAssetDropData("C:/Project/Assets/hero.png", ".png", false);

        var result = item.GetPayloadItems<IAssetDropData>().ToList();

        Assert.Single(result);
        Assert.Same(item, result[0]);
    }

    [Fact]
    public void GetPayloadItems_MixedCollection_ReturnsOnlyTypedItems()
    {
        var itemA = new TestAssetDropData("A.png", ".png", false);
        var itemB = new TestAssetDropData("B.png", ".png", false);
        object payload = new object[] { itemA, "ignore", itemB };

        var result = payload.GetPayloadItems<IAssetDropData>().ToList();

        Assert.Equal(2, result.Count);
        Assert.Same(itemA, result[0]);
        Assert.Same(itemB, result[1]);
    }

    [Fact]
    public void CanAccept_AllItemsMatchType_ReturnsTrue()
    {
        object payload = new[] { "A", "B", "C" };

        var canAccept = payload.CanAccept<string>();

        Assert.True(canAccept);
    }

    [Fact]
    public void CanAccept_CollectionContainsDifferentType_ReturnsFalse()
    {
        object payload = new object[] { "A", 42 };

        var canAccept = payload.CanAccept<string>();

        Assert.False(canAccept);
    }

    [Fact]
    public void CanAccept_WithTarget_RejectsWhenTargetIncluded()
    {
        var target = new object();
        object payload = new object[] { new object(), target };

        var canAccept = payload.CanAccept(target);

        Assert.False(canAccept);
    }

    private sealed class TestAssetDropData(string fullPath, string extension, bool isFolder) : IAssetDropData
    {
        public string FullPath { get; } = fullPath;
        public string Extension { get; } = extension;
        public bool IsFolder { get; } = isFolder;
    }
}
