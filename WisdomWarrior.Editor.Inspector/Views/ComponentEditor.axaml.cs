using System.Numerics;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using WisdomWarrior.Editor.Core.ShadowTree;
using WisdomWarrior.Editor.Inspector.Helpers;
using WisdomWarrior.Engine.Core.Assets;
using WisdomWarrior.Engine.Core.Attributes;
using WisdomWarrior.Engine.Core.DataTypes;
using Color = System.Drawing.Color;

namespace WisdomWarrior.Editor.Inspector.Views;

public partial class ComponentEditor : UserControl
{
    public ComponentEditor()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);

        if (DataContext is not ComponentTracker tracker) return;

        PropertiesStack.Children.Clear();

        foreach (var propTracker in tracker.Properties)
        {
            var shouldHide = propTracker.GetCustomAttribute<HideInInspectorAttribute>() != null;

            if (shouldHide) continue;

            var row = CreatePropertyRow(propTracker);
            PropertiesStack.Children.Add(row);
        }
    }

    private Control CreatePropertyRow(PropertyTracker prop)
    {
        var grid = new Grid
        {
            ColumnDefinitions = new ColumnDefinitions("100, *"),
            HorizontalAlignment = HorizontalAlignment.Stretch
        };

        var label = new TextBlock
        {
            Text = prop.Name,
            Opacity = 0.5,
            FontSize = 10,
            VerticalAlignment = VerticalAlignment.Center
        };

        Grid.SetColumn(label, 0);
        grid.Children.Add(label);

        var editor = prop.PropertyType switch
        {
            Type t when t == typeof(Vector2) => this.CreateVector2Editor(prop),
            Type t when t == typeof(float) => this.CreateFloatEditor(prop),
            Type t when t == typeof(Size) => this.CreateSizeEditor(prop),
            Type t when t == typeof(Color) => this.CreateColourEditor(prop),
            Type t when t == typeof(ImageAsset) => this.CreateImageAssetEditor(prop),
            // Type t when t == typeof(string) => CreateStringEditor(prop, target),
            _ => new TextBlock { Text = "Unsupported Type", FontStyle = FontStyle.Italic }
        };

        Grid.SetColumn(editor, 1);
        grid.Children.Add(editor);
        return grid;
    }
}