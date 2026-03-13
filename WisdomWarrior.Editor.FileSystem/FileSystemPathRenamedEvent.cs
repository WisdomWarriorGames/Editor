namespace WisdomWarrior.Editor.FileSystem;

public sealed class FileSystemPathRenamedEvent
{
    public required string OldPath { get; init; }
    public required string NewPath { get; init; }
    public required bool IsDirectory { get; init; }
}
