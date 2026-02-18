namespace WisdomWarrior.Editor.FileSystem.Models;

public class Manifest
{
    public string GameProjectName { get; set; }
    public string GameProjectNameStripped { get; set; }
    public string GameProjectPath { get; set; }
    public List<ProjectModule> Modules { get; set; } = [];
}

public class ProjectModule
{
    public string Name { get; set; }
    public string Path { get; set; }
}