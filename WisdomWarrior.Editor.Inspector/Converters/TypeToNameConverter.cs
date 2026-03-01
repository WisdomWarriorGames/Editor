using System.Globalization;
using Avalonia.Data.Converters;

namespace WisdomWarrior.Editor.Inspector.Converters;

public class TypeToNameConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value?.GetType().Name ?? "Unknown Component";
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
}