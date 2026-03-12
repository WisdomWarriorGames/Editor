using System.Text.Json;
using WisdomWarrior.Editor.Core;
using WisdomWarrior.Editor.Core.Helpers;

namespace WisdomWarrior.Editor.FileSystem;

public class WorkspaceService(FileSystemRegistry registry)
{
    private readonly FileSystemRegistry _currentRegistry = registry;
    private Manifest? _project;

    public string ProjectRoot { get; private set; } = string.Empty;
    public string CurrentModuleRoot { get; private set; } = string.Empty;
    public string ActiveScene => Path.Combine(ProjectRoot, _project!.ActiveScene);

    public event Action<FileSystemRegistry>? WorkspaceInitialized;

    public void Load(string pathToManifest)
    {
        var json = File.ReadAllText(pathToManifest);
        _project = JsonSerializer.Deserialize<Manifest>(json);

        var baseDirectory = Path.GetDirectoryName(pathToManifest)!;
        Load(_project, baseDirectory);
    }

    public void Load(Manifest? manifest, string rootPath)
    {
        _project = manifest;

        if (_project == null)
        {
            return;
        }

        ProjectRoot = rootPath;
        AssetPathContext.Configure(ProjectRoot, null);

        if (_project.Modules.Count > 0)
        {
            var module = _project.Modules[0];
            var path = Path.Combine(rootPath, module.Path);
            ChangeModule(path);
        }
    }

    public void ChangeModule(string path)
    {
        _currentRegistry.Initialize(path);
        CurrentModuleRoot = path;
        AssetPathContext.Configure(ProjectRoot, CurrentModuleRoot);
        WorkspaceInitialized?.Invoke(_currentRegistry);
    }
}
