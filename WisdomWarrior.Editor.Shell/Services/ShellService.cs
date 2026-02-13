using Avalonia.Controls;
using Dock.Model.Core;
using System.Linq;
using WisdomWarrior.Editor.Core;
using WisdomWarrior.Editor.Shell.ViewModels;

namespace WisdomWarrior.Editor.Shell.Services;

public class ShellService : IShell
{
    private readonly IFactory _factory;
    private readonly IDock _layout;

    public ShellService(IDock layout, IFactory factory)
    {
        _layout = layout;
        _factory = factory;
    }

    public void Dock(DockLocation location, object view, string title = "")
    {
        // 1. Map the location to our Layout IDs
        var paneId = location switch
        {
            DockLocation.Left => "LeftPane",
            DockLocation.Right => "RightPane",
            DockLocation.Bottom => "BottomPane",
            _ => "MainDocumentPane"
        };

        // 2. Find the pane
        var targetPane = FindDockable(_layout, paneId) as IDock;

        if (targetPane != null)
        {
            // 3. Create the Document wrapper
            var document = new EditorDocument
            {
                Id = title,
                Title = title,
                ContentView = view 
            };

            // 4. Add it to the layout
            _factory.AddDockable(targetPane, document);
            _factory.SetActiveDockable(document);
            _factory.SetFocusedDockable(targetPane, document);
        }
    }

    // Simple recursive helper to find the pane
    private IDockable? FindDockable(IDockable current, string id)
    {
        if (current.Id == id) return current;
        if (current is IDock dock && dock.VisibleDockables != null)
        {
            foreach (var child in dock.VisibleDockables)
            {
                var found = FindDockable(child, id);
                if (found != null) return found;
            }
        }
        return null;
    }
    
    public void SetStatus(string message) { /* Future */ }
}