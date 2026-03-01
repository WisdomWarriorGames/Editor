using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using WisdomWarrior.Editor.SceneList.ViewModels;
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

    private void OnBackgroundClicked(object? sender, PointerPressedEventArgs e)
    {
        var visual = e.Source as Visual;

        while (visual != null)
        {
            if (visual is TreeViewItem)
            {
                return;
            }

            visual = visual.GetVisualParent();
        }

        if (sender is TreeView treeView)
        {
            treeView.SelectedItems.Clear();
            if (DataContext is SceneHierarchyViewModel viewModel)
            {
                viewModel.ResetChanges();
            }
        }
    }
}