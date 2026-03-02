using System.Globalization;
using Avalonia.Data.Converters;
using WisdomWarrior.Editor.Core.ShadowTree;

namespace WisdomWarrior.Editor.Inspector.Converters;

public class TypeToNameConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null) return "Unknown Component";

        if (value is ComponentTracker tracker)
        {
            return tracker.EngineComponent.GetType().Name;
        }

        return value?.GetType().Name ?? "Unknown Component";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}