using System.Text.Json;
using WisdomWarrior.Editor.Core;

namespace WisdomWarrior.Editor.FileSystem;

public class WorkspaceService(FileSystemRegistry registry)
{
    private FileSystemRegistry _currentRegistry = registry;

    public Manifest? Project { get; private set; }
    public string ProjectRoot { get; private set; }

    public event Action<FileSystemRegistry>? WorkspaceInitialized;

    public void Load(string pathToManifest)
    {
        var json = File.ReadAllText(pathToManifest);
        Project = JsonSerializer.Deserialize<Manifest>(json);
        Load(Project);
    }

    public void Load(Manifest? manifest)
    {
        Project = manifest;

        if (Project == null)
        {
            return;
        }

        ProjectRoot = Project.ProjectRoot;

        if (Project.Modules.Count > 0)
        {
            var module = Project.Modules[0];
            ChangeModule(module.Path);
        }
    }

    public void ChangeModule(string path)
    {
        _currentRegistry.Initialize(path);
        WorkspaceInitialized?.Invoke(_currentRegistry);
    }
}