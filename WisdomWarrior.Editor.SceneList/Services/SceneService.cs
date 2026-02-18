using System.Text.Json;
using WisdomWarrior.Editor.Core;
using WisdomWarrior.Editor.FileSystem;
using WisdomWarrior.Engine.Core;

namespace WisdomWarrior.Editor.SceneList.Services;

public class SceneService
{
    private readonly WorkspaceService _workspaceService;

    public Scene ActiveScene { get; set; }

    public SceneService(WorkspaceService workspaceService, FileSystemService fileSystemService)
    {
        _workspaceService = workspaceService;
        _workspaceService.WorkspaceInitialized += OnWorkspaceInitialized;
    }

    private void OnWorkspaceInitialized(FileSystemRegistry registry)
    {
        if (_workspaceService.Project?.ActiveScene != null)
        {
            var json = File.ReadAllText(_workspaceService.Project?.ActiveScene);
            ActiveScene = JsonSerializer.Deserialize<Scene>(json);
        }
    }
}