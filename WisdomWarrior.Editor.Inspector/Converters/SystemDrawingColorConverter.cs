using System.Globalization;
using Avalonia.Data.Converters;
using AvaloniaColor = Avalonia.Media.Color;
using SystemColor = System.Drawing.Color;

namespace WisdomWarrior.Editor.Inspector.Converters;

public class SystemDrawingColorConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is SystemColor sysColor)
        {
            return AvaloniaColor.FromArgb(sysColor.A, sysColor.R, sysColor.G, sysColor.B);
        }

        return Avalonia.Media.Colors.White;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is AvaloniaColor avColor)
        {
            return SystemColor.FromArgb(avColor.A, avColor.R, avColor.G, avColor.B);
        }

        return SystemColor.White;
    }
}