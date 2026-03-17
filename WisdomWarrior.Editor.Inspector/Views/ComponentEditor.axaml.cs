using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Threading;
using WisdomWarrior.Editor.Core.ShadowTree;
using WisdomWarrior.Editor.Inspector.Helpers;

namespace WisdomWarrior.Editor.Inspector.Views;

public partial class ComponentEditor : UserControl
{
    private readonly List<TrackerInspectorProperty> _rootProperties = [];

    public ComponentEditor()
    {
        InitializeComponent();
        DetachedFromVisualTree += (_, _) => DisposeRootProperties();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        DisposeRootProperties();
        PropertiesStack.Children.Clear();

        if (DataContext is not IInspectableObjectTracker tracker)
        {
            return;
        }

        foreach (var propTracker in tracker.Properties)
        {
            var prop = new TrackerInspectorProperty(propTracker);
            prop.OnValueChanged += HandleRootPropertyChanged;
            _rootProperties.Add(prop);
        }

        RebuildPropertyRows();
    }

    private void HandleRootPropertyChanged(object? _)
    {
        Dispatcher.UIThread.Post(RebuildPropertyRows);
    }

    private void RebuildPropertyRows()
    {
        PropertiesStack.Children.Clear();

        foreach (var visibleProperty in InspectorPropertyExpander.ExpandVisibleProperties(_rootProperties))
        {
            PropertiesStack.Children.Add(CreatePropertyRow(visibleProperty));
        }
    }

    private Control CreatePropertyRow(VisibleInspectorProperty visibleProperty)
    {
        var prop = visibleProperty.Property;

        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("100, *"),
            ColumnSpacing = 6,
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        var label = new TextBlock
        {
            Text = prop.Name,
            Classes = { "compact-label" },
            VerticalAlignment = VerticalAlignment.Center
        };

        Grid.SetColumn(label, 0);
        grid.Children.Add(label);

        var editor = this.CreateEditorControl(prop);
        Grid.SetColumn(editor, 1);
        grid.Children.Add(editor);

        grid.DetachedFromVisualTree += (_, _) =>
        {
            if (!visibleProperty.DisposeWhenDetached || prop is not IDisposable disposable)
            {
                return;
            }

            disposable.Dispose();
        };

        return grid;
    }

    private void DisposeRootProperties()
    {
        foreach (var prop in _rootProperties)
        {
            prop.OnValueChanged -= HandleRootPropertyChanged;
            prop.Dispose();
        }

        _rootProperties.Clear();
    }
}
