using Comqueror.Properties;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Comqueror.Utility;

public static class EnumExtensions
{
    public static string[] GetEnumResourceNames<TEnum>() where TEnum : struct, Enum
    {
        return GetEnumResourceNames(typeof(TEnum));
    }
    public static string[] GetEnumResourceNames(Type enumType)
    {
        string[] names = Enum.GetNames(enumType);

        for (int i = 0; i < names.Length; i++)
        {
            names[i] = $"Enum_{enumType.Name}.{names[i]}";
        }

        return names;
    }

    public static string? GetResourceName<TEnum>(this TEnum value) where TEnum : struct, Enum
    {
        return GetResourceName(typeof(TEnum), (object)value);
    }

    public static string? GetResourceName(Type type, object value)
    {
        string enumName = type.Name;
        string? valueName = Enum.GetName(type, value);

        if (valueName == null)
            return null;

        return $"Enum_{enumName}.{valueName}";
    }
}

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

public sealed class EnumerateExtension : MarkupExtension
{
    public Type Type { get; set; }

    public EnumerateExtension(Type type)
    {
        Type = type;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        string[] resourceNames = EnumExtensions.GetEnumResourceNames(Type);

        string?[] values = new string[resourceNames.Length];

        for (int i = 0; i < resourceNames.Length; i++)
        { 
            values[i] = Strings.ResourceManager.GetString(resourceNames[i]);
        }

        return values;
    }
}
