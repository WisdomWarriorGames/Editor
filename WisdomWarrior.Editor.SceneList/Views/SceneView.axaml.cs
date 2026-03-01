using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using WisdomWarrior.Engine.Core;

namespace WisdomWarrior.Editor.SceneList.Views;

public partial class SceneView : UserControl
{
    private readonly TreeView? tree;

    public SceneView()
    {
        InitializeComponent();

        tree = this.FindControl<TreeView>("HierarchyTree");
        if (tree != null)
        {
            tree.ContainerPrepared += OnContainerPrepared;
        }
    }

    private void OnContainerPrepared(object? sender, ContainerPreparedEventArgs e)
    {
        if (e.Container is TreeViewItem item && item.DataContext is Scene)
        {
            item.IsExpanded = true;
            tree.ContainerPrepared -= OnContainerPrepared;
        }
    }
}