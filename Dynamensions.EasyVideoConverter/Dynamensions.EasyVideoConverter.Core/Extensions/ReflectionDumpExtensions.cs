using System.Collections;
using System.Reflection;

namespace Dynamensions.EasyVideoConverter.Core.Extensions;

public static class ReflectionDumpExtensions
{
    public static string DumpPropertiesFormatted(this object obj)
    {
        return string.Join(',', obj.DumpProperties() ?? Enumerable.Empty<string>());
    }

    public static string[] DumpProperties(this object obj)
    {
        List<string> dumpedProperties = [];

        if (obj == null) return [];

        var type = obj.GetType();
        //Console.WriteLine($"--- Dump ({type.Name}) ---");

        foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            object? value;

            try
            {
                value = prop.GetValue(obj);
            }
            catch (Exception ex)
            {
                value = $"<error: {ex.Message}>";
            }

            string formatted = FormatValue(value);
            dumpedProperties.Add($"{prop.Name}: {formatted}");
            //Console.WriteLine($"{prop.Name}: {formatted}");
        }

        //Console.WriteLine($"--- End Dump ({type.Name}) ---");

        return [.. dumpedProperties];
    }

    private static string FormatValue(object? value)
    {
        if (value == null)
            return "null";

        // Strings
        if (value is string s)
            return s;

        // Enumerables (lists, arrays, etc.)
        if (value is IEnumerable enumerable && value is not string)
        {
            var items = new List<string>();
            foreach (var item in enumerable)
                items.Add(item?.ToString() ?? "null");

            return "[" + string.Join(", ", items) + "]";
        }

        // Everything else
        return value.ToString() ?? "null";
    }
}
