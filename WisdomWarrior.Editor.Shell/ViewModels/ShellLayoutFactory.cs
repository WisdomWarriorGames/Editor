using System.Collections.Generic;
using Dock.Model.Avalonia;
using Dock.Model.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;

namespace WisdomWarrior.Editor.Shell.ViewModels;

public class ShellLayoutFactory : Factory
{
    public override IRootDock CreateLayout()
    {
        var centerPane = new DocumentDock
        {
            Id = "MainDocumentPane",
            Proportion = double.NaN,
        };

        // 2. Create Tools for Left/Right (Empty but initialized)
        var leftPane = new ToolDock
        {
            Id = "LeftPane",
            Title = "Explorer",
            Proportion = 0.25,
            CanClose = false
        };

        var rightPane = new ToolDock
        {
            Id = "RightPane",
            Title = "Inspector",
            Proportion = 0.25,
            CanClose = false
        };

        var bottomPane = new ToolDock
        {
            Id = "BottomPane",
            Title = "Output",
            Proportion = 0.3,
            CanClose = false
        };

        // 3. Structure the Layout (Same as before)
        var mainRow = new ProportionalDock
        {
            Orientation = Orientation.Horizontal,
            VisibleDockables = new List<IDockable>
            {
                leftPane,
                new ProportionalDockSplitter(),
                centerPane,
                new ProportionalDockSplitter(),
                rightPane
            }
        };

        var rootLayout = new ProportionalDock
        {
            Orientation = Orientation.Vertical,
            VisibleDockables = new List<IDockable>
            {
                mainRow, // Top section (Left-Center-Right)
                new ProportionalDockSplitter(),
                bottomPane // Bottom section
            }
        };

        return new RootDock
        {
            Title = "Default",
            IsCollapsable = false,
            VisibleDockables = new List<IDockable> { rootLayout },
            ActiveDockable = rootLayout,
            DefaultDockable = rootLayout
        };
    }
}