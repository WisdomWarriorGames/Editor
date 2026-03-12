namespace WisdomWarrior.Editor.Core.Models;

public interface IAssetDropData
{
    string FullPath { get; }
    string Extension { get; }
    bool IsFolder { get; }
}
