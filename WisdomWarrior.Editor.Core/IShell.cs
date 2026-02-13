namespace WisdomWarrior.Editor.Core;

public interface IShell
{
    /// <summary>
    /// Puts a view (UserControl) into a specific slot in the editor.
    /// </summary>
    void Dock(DockLocation location, object view, string title = "");

    void SetStatus(string message);
}