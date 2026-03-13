namespace WisdomWarrior.Editor.FileSystem;

public class WorkspaceDescriptor
{
    public string RootPath { get; init; } = string.Empty;
    public string SolutionFilePath { get; init; } = string.Empty;
    public string DefaultProjectPath { get; init; } = string.Empty;
    public string ActiveScenePath { get; init; } = string.Empty;
}
