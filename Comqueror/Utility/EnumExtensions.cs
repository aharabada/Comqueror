using System;

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
