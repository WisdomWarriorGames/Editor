namespace WisdomWarrior.Editor.Core.Helpers;

public static class AssetPathContext
{
    private static readonly object Sync = new();
    private static string? _projectRoot;
    private static string? _assetImportRoot;

    public static string? ProjectRoot
    {
        get
        {
            lock (Sync) return _projectRoot;
        }
    }

    public static string? AssetImportRoot
    {
        get
        {
            lock (Sync) return _assetImportRoot;
        }
    }

    public static void Configure(string? projectRoot, string? assetImportRoot)
    {
        lock (Sync)
        {
            _projectRoot = NormalizeRoot(projectRoot);
            _assetImportRoot = NormalizeRoot(assetImportRoot) ?? _projectRoot;
        }
    }

    private static string? NormalizeRoot(string? root)
    {
        if (string.IsNullOrWhiteSpace(root))
            return null;

        return Path.GetFullPath(root);
    }
}
