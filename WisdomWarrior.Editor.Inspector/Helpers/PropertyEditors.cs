using System;
using System.Numerics;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Threading;
using WisdomWarrior.Editor.Inspector.Models;
using WisdomWarrior.Engine.Core.Assets;
using DrawingColor = System.Drawing.Color;
using Size = WisdomWarrior.Engine.Core.DataTypes.Size;

namespace WisdomWarrior.Editor.Inspector.Helpers;

public static class PropertyEditors
{
    public static bool SupportsDirectEditor(Type propertyType)
    {
        return propertyType switch
        {
            Type t when t == typeof(Vector2) => true,
            Type t when t == typeof(float) => true,
            Type t when t == typeof(Size) => true,
            Type t when t == typeof(DrawingColor) => true,
            Type t when t == typeof(ImageAsset) => true,
            Type t when t == typeof(bool) => true,
            Type t when t.IsEnum => true,
            _ => false
        };
    }

    public static Control CreateEditorControl(this UserControl control, IInspectorProperty prop)
    {
        return prop.PropertyType switch
        {
            Type t when t == typeof(Vector2) => control.CreateVector2Editor(prop),
            Type t when t == typeof(float) => control.CreateFloatEditor(prop),
            Type t when t == typeof(Size) => control.CreateSizeEditor(prop),
            Type t when t == typeof(DrawingColor) => control.CreateColourEditor(prop),
            Type t when t == typeof(ImageAsset) => control.CreateImageAssetEditor(prop),
            Type t when t == typeof(bool) => control.CreateBoolEditor(prop),
            Type t when t.IsEnum => control.CreateEnumEditor(prop),
            _ => CreateUnsupportedEditor()
        };
    }

    public static Control CreateVector2Editor(this UserControl control, IInspectorProperty prop)
    {
        return BuildEditor(
            control,
            prop,
            "Vector2Template",
            new Vector2ViewModel((Vector2)prop.GetValue()!, newValue => prop.SetValue(newValue)),
            viewModel => viewModel.UpdateFromEngine((Vector2)prop.GetValue()!));
    }

    public static Control CreateFloatEditor(this UserControl control, IInspectorProperty prop)
    {
        return BuildEditor(
            control,
            prop,
            "FloatTemplate",
            new FloatViewModel((float)prop.GetValue()!, newValue => prop.SetValue(newValue)),
            viewModel => viewModel.UpdateFromEngine((float)prop.GetValue()!));
    }

    public static Control CreateSizeEditor(this UserControl control, IInspectorProperty prop)
    {
        return BuildEditor(
            control,
            prop,
            "SizeTemplate",
            new SizeViewModel((Size)prop.GetValue()!, newValue => prop.SetValue(newValue)),
            viewModel => viewModel.UpdateFromEngine((Size)prop.GetValue()!));
    }

    public static Control CreateColourEditor(this UserControl control, IInspectorProperty prop)
    {
        return BuildEditor(
            control,
            prop,
            "ColourTemplate",
            new ColourViewModel((DrawingColor)prop.GetValue()!, newValue => prop.SetValue(newValue)),
            viewModel => viewModel.UpdateFromEngine((DrawingColor)prop.GetValue()!));
    }

    public static Control CreateImageAssetEditor(this UserControl control, IInspectorProperty prop)
    {
        return BuildEditor(
            control,
            prop,
            "ImageAssetTemplate",
            new ImageAssetViewModel((ImageAsset)prop.GetValue()!, newValue => prop.SetValue(newValue)),
            viewModel => viewModel.UpdateFromEngine((ImageAsset)prop.GetValue()!));
    }

    public static Control CreateEnumEditor(this UserControl control, IInspectorProperty prop)
    {
        var currentValue = prop.GetValue() ?? throw new InvalidOperationException("Enum property value cannot be null.");

        return BuildEditor(
            control,
            prop,
            "EnumTemplate",
            new EnumPropertyViewModel(prop.PropertyType, currentValue, newValue => prop.SetValue(newValue)),
            viewModel => viewModel.UpdateFromEngine(prop.GetValue() ?? currentValue));
    }

    public static Control CreateBoolEditor(this UserControl control, IInspectorProperty prop)
    {
        return BuildEditor(
            control,
            prop,
            "BoolTemplate",
            new BoolPropertyViewModel((bool)prop.GetValue()!, newValue => prop.SetValue(newValue)),
            viewModel => viewModel.UpdateFromEngine((bool)prop.GetValue()!));
    }

    public static Control CreateUnsupportedEditor()
    {
        return new TextBlock
        {
            Text = "Unsupported Type",
            FontStyle = Avalonia.Media.FontStyle.Italic
        };
    }

    private static Control BuildEditor<TViewModel>(
        UserControl control,
        IInspectorProperty prop,
        string templateKey,
        TViewModel viewModel,
        Action<TViewModel> updateViewModelAction)
    {
        var editor = new ContentControl
        {
            DataContext = viewModel,
            Content = viewModel,
            ContentTemplate = control.FindResource(templateKey) as DataTemplate
        };

        Action updateAction = () => updateViewModelAction(viewModel);
        Action<object?> valueChangedHandler = _ => Dispatcher.UIThread.Post(updateAction);
        prop.OnValueChanged += valueChangedHandler;

        editor.DetachedFromVisualTree += (_, _) => { prop.OnValueChanged -= valueChangedHandler; };

        return editor;
    }
}
