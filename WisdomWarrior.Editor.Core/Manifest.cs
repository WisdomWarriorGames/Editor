namespace WisdomWarrior.Editor.Core;

public class Manifest
{
    public string ProjectName { get; set; }
    public string ProjectNameStripped { get; set; }
    public string GameProjectPath { get; set; }
    public string ProjectRoot { get; set; }
    public string ActiveScene { get; set; }
    public List<ProjectModule> Modules { get; set; } = [];
}

public class ProjectModule
{
    public string Name { get; set; }
    public string Path { get; set; }
}