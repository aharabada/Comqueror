using Comqueror.Properties;
using System;
using System.Windows.Markup;

namespace Comqueror.Utility;

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
