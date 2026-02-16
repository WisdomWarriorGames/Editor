using Avalonia.Input;

namespace WisdomWarrior.Editor.AssetBrowser.ViewModels;

public interface IAssetHandler
{
    void OnSelected(AssetItem item, KeyModifiers modifiers);
    void OnDoubleClicked(AssetItem item);
    void OnRenameCommitted(AssetItem item, string newName);
    void OnDeleteRequested(AssetItem item);
}