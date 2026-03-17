using WisdomWarrior.Editor.FileSystem;

namespace WisdomWarrior.Editor.Core.Tests.Services;

public class FileSystemServiceCopyTests
{
    [Fact]
    public async Task CopyAssetsAsync_FileIntoSameFolder_UsesNextDuplicateNumberAndPreservesContents()
    {
        var tempRoot = CreateTempRoot();

        try
        {
            var fileSystemService = new FileSystemService();
            var sourceFile = Path.Combine(tempRoot, "Script.cs");
            File.WriteAllText(sourceFile, "console.log('source');");
            File.WriteAllText(Path.Combine(tempRoot, "Script (1).cs"), "one");
            File.WriteAllText(Path.Combine(tempRoot, "Script (2).cs"), "two");

            await fileSystemService.CopyAssetsAsync([sourceFile], tempRoot, CopyAssetMode.DuplicateExisting);

            var duplicatePath = Path.Combine(tempRoot, "Script (3).cs");
            Assert.True(File.Exists(duplicatePath));
            Assert.Equal("console.log('source');", File.ReadAllText(duplicatePath));
        }
        finally
        {
            Directory.Delete(tempRoot, true);
        }
    }

    [Fact]
    public async Task CopyAssetsAsync_NumberedSource_ReusesNormalizedBaseName()
    {
        var tempRoot = CreateTempRoot();

        try
        {
            var fileSystemService = new FileSystemService();
            File.WriteAllText(Path.Combine(tempRoot, "Example.txt"), "base");
            File.WriteAllText(Path.Combine(tempRoot, "Example (1).txt"), "one");
            var numberedSource = Path.Combine(tempRoot, "Example (2).txt");
            File.WriteAllText(numberedSource, "two");

            await fileSystemService.CopyAssetsAsync([numberedSource], tempRoot, CopyAssetMode.DuplicateExisting);

            Assert.True(File.Exists(Path.Combine(tempRoot, "Example (3).txt")));
        }
        finally
        {
            Directory.Delete(tempRoot, true);
        }
    }

    [Fact]
    public async Task CopyAssetsAsync_FolderIntoSameFolder_CopiesHierarchyAndPreservesIgnoredDescendantsForInternalCopies()
    {
        var tempRoot = CreateTempRoot();

        try
        {
            var fileSystemService = new FileSystemService();
            var sourceFolder = Path.Combine(tempRoot, "Assets");
            var nestedFolder = Path.Combine(sourceFolder, "Textures");
            var ignoredFolder = Path.Combine(sourceFolder, "bin");
            Directory.CreateDirectory(nestedFolder);
            Directory.CreateDirectory(ignoredFolder);
            File.WriteAllText(Path.Combine(nestedFolder, "grass.png"), "png");
            File.WriteAllText(Path.Combine(ignoredFolder, "artifact.txt"), "artifact");

            await fileSystemService.CopyAssetsAsync([sourceFolder], tempRoot, CopyAssetMode.DuplicateExisting);

            var duplicateRoot = Path.Combine(tempRoot, "Assets (1)");
            Assert.True(File.Exists(Path.Combine(duplicateRoot, "Textures", "grass.png")));
            Assert.True(File.Exists(Path.Combine(duplicateRoot, "bin", "artifact.txt")));
        }
        finally
        {
            Directory.Delete(tempRoot, true);
        }
    }

    [Fact]
    public async Task CopyAssetsAsync_ImportExternal_FiltersIgnoredEntries()
    {
        var tempRoot = CreateTempRoot();

        try
        {
            var fileSystemService = new FileSystemService();
            var sourceRoot = Path.Combine(tempRoot, "External");
            var targetRoot = Path.Combine(tempRoot, "Project");
            Directory.CreateDirectory(Path.Combine(sourceRoot, "bin"));
            Directory.CreateDirectory(targetRoot);
            File.WriteAllText(Path.Combine(sourceRoot, "bin", "ignored.txt"), "skip");
            File.WriteAllText(Path.Combine(sourceRoot, "keep.txt"), "keep");

            await fileSystemService.CopyAssetsAsync([sourceRoot], targetRoot, CopyAssetMode.ImportExternal);

            var importedRoot = Path.Combine(targetRoot, "External");
            Assert.True(File.Exists(Path.Combine(importedRoot, "keep.txt")));
            Assert.False(Directory.Exists(Path.Combine(importedRoot, "bin")));
        }
        finally
        {
            Directory.Delete(tempRoot, true);
        }
    }

    [Fact]
    public async Task CopyAssetsAsync_MultipleConflictingFiles_AllocatesUniqueNamesAcrossBatch()
    {
        var tempRoot = CreateTempRoot();

        try
        {
            var fileSystemService = new FileSystemService();
            var sourceA = Path.Combine(tempRoot, "A");
            var sourceB = Path.Combine(tempRoot, "B");
            var target = Path.Combine(tempRoot, "Target");
            Directory.CreateDirectory(sourceA);
            Directory.CreateDirectory(sourceB);
            Directory.CreateDirectory(target);

            var sourceFileA = Path.Combine(sourceA, "Sprite.png");
            var sourceFileB = Path.Combine(sourceB, "Sprite.png");
            File.WriteAllText(sourceFileA, "A");
            File.WriteAllText(sourceFileB, "B");
            File.WriteAllText(Path.Combine(target, "Sprite.png"), "original");

            await fileSystemService.CopyAssetsAsync([sourceFileA, sourceFileB], target, CopyAssetMode.DuplicateExisting);

            Assert.Equal("A", File.ReadAllText(Path.Combine(target, "Sprite (1).png")));
            Assert.Equal("B", File.ReadAllText(Path.Combine(target, "Sprite (2).png")));
        }
        finally
        {
            Directory.Delete(tempRoot, true);
        }
    }

    [Fact]
    public async Task CopyAssetsAsync_FolderIntoDescendant_IsBlocked()
    {
        var tempRoot = CreateTempRoot();

        try
        {
            var fileSystemService = new FileSystemService();
            var sourceRoot = Path.Combine(tempRoot, "Source");
            var childFolder = Path.Combine(sourceRoot, "Child");
            Directory.CreateDirectory(childFolder);
            File.WriteAllText(Path.Combine(sourceRoot, "root.txt"), "root");

            await fileSystemService.CopyAssetsAsync([sourceRoot], childFolder, CopyAssetMode.DuplicateExisting);

            Assert.Single(Directory.GetDirectories(sourceRoot));
            Assert.False(Directory.Exists(Path.Combine(childFolder, "Source")));
        }
        finally
        {
            Directory.Delete(tempRoot, true);
        }
    }

    private static string CreateTempRoot()
    {
        var tempRoot = Path.Combine(Path.GetTempPath(), "WisdomWarrior.Editor.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempRoot);
        return tempRoot;
    }
}
