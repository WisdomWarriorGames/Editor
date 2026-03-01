namespace WisdomWarrior.Editor.FileSystem.Models;

public class FileSystemNode
{
    public string Name { get; set; } = string.Empty;
    public string FullPath { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public bool IsFolder { get; set; }

    public FileSystemNode? Parent { get; set; } = null;

    public string FileNameWithExtension => IsFolder ? Name : $"{Name}{Extension}";

    public List<FileSystemNode> Children { get; set; } = [];
    public List<FileSystemNode> Folders => Children.Where(x => x.IsFolder).ToList();

    public FileSystemNode(string path, bool isFolder)
    {
        Name = Path.GetFileNameWithoutExtension(path);
        FullPath = path;
        IsFolder = isFolder;

        if (!IsFolder)
        {
            Extension = Path.GetExtension(path);
        }
    }
}