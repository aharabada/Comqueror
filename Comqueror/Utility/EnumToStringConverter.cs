using Comqueror.Properties;
using System;
using System.Globalization;
using System.Windows.Data;

namespace Comqueror.Utility;

public sealed class EnumToStringConverter : IValueConverter
{
    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value == null || !(value is Enum))
            return null;



        string? valueName = EnumExtensions.GetResourceName(value.GetType(), value);

        if (valueName == null)
            return null;

        return Strings.ResourceManager.GetString(valueName);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        string str = (string)value;
        
        foreach (object enumValue in Enum.GetValues(targetType))
        {
            string? enumString = EnumExtensions.GetResourceName(targetType, enumValue);

            if (enumString == null)
                continue;

            string? localizedString = Strings.ResourceManager.GetString(enumString);

            if (str == localizedString)
                return enumValue;
        }

        throw new ArgumentException(null, nameof(value));
    }
}
