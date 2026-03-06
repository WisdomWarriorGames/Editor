using System;
using System.Drawing;
using System.Numerics;
using Avalonia.Controls;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Threading;
using WisdomWarrior.Editor.Core.ShadowTree;
using WisdomWarrior.Editor.Inspector.Models;
using Size = WisdomWarrior.Engine.Core.DataTypes.Size;

namespace WisdomWarrior.Editor.Inspector.Helpers;

public static class PropertyEditors
{
    public static Control CreateVector2Editor(this UserControl control, PropertyTracker prop)
    {
        return BuildEditor<Vector2, Vector2ViewModel>(
            control,
            prop,
            "Vector2Template",
            (val, setter) => new Vector2ViewModel(val, setter),
            (vm, val) => vm.UpdateFromEngine(val)
        );
    }

    public static Control CreateFloatEditor(this UserControl control, PropertyTracker prop)
    {
        return BuildEditor<float, FloatViewModel>(
            control,
            prop,
            "FloatTemplate",
            (val, setter) => new FloatViewModel(val, setter),
            (vm, val) => vm.UpdateFromEngine(val)
        );
    }

    public static Control CreateSizeEditor(this UserControl control, PropertyTracker prop)
    {
        return BuildEditor<Size, SizeViewModel>(
            control,
            prop,
            "SizeTemplate",
            (val, setter) => new SizeViewModel(val, setter),
            (vm, val) => vm.UpdateFromEngine(val)
        );
    }

    public static Control CreateColourEditor(this UserControl control, PropertyTracker prop)
    {
        return BuildEditor<Color, ColourViewModel>(
            control,
            prop,
            "ColourTemplate",
            (val, setter) => new ColourViewModel(val, setter),
            (vm, val) => vm.UpdateFromEngine(val)
        );
    }

    private static Control BuildEditor<TValue, TViewModel>(
        UserControl control,
        PropertyTracker prop,
        string templateKey,
        Func<TValue, Action<TValue>, TViewModel> viewModelFactory,
        Action<TViewModel, TValue> updateViewModelAction)
    {
        var currentValue = (TValue)prop.GetValue()!;

        var viewModel = viewModelFactory(currentValue, (newValue) => prop.SetValue(newValue));

        var editor = new ContentControl
        {
            DataContext = viewModel,
            Content = viewModel,
            ContentTemplate = control.FindResource(templateKey) as DataTemplate
        };

        Action updateAction = () => { updateViewModelAction(viewModel, (TValue)prop.GetValue()!); };
        editor.Tag = updateAction;

        Action<object?> valueChangedHandler = (newValue) => { Dispatcher.UIThread.Post(updateAction); };
        prop.OnValueChanged += valueChangedHandler;

        editor.DetachedFromVisualTree += (s, e) => { prop.OnValueChanged -= valueChangedHandler; };

        return editor;
    }
}