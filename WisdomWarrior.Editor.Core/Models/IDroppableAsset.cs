namespace WisdomWarrior.Editor.Core.Models;

public interface IDroppableAsset
{
    string DisplayName { get; }
    Task ProcessAsync(string destinationDirectory, IProgress<string>? progress = null);
}