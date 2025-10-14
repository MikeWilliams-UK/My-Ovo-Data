using System;
using System.IO;
using System.Reflection;

namespace OvoData.Helpers;

public static class ResourceHelper
{
    public static string GetStringResource(string resourceName)
    {
        string data = string.Empty;

        var resource = GetBinaryResource(resourceName);
        if (resource != null)
        {
            var textStreamReader = new StreamReader(resource);
            data = textStreamReader.ReadToEnd();

            // Repair any "broken" line feeds to Windows style
            var etx = (char)3;
            var temp = data.Replace("\r\n", $"{etx}");
            temp = temp.Replace("\n", $"{etx}");
            temp = temp.Replace("\r", $"{etx}");
            var lines = temp.Split(etx);
            data = string.Join(Environment.NewLine, lines);
        }

        return data;
    }

    private static Stream? GetBinaryResource(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();

        var data = Stream.Null;

        var fullName = string.Empty;
        var count = 0;

        var resources = assembly.GetManifestResourceNames();
        foreach (var s in resources)
        {
            if (s.EndsWith($".{resourceName}"))
            {
                count++;
                fullName = s;
            }
        }

        if (!string.IsNullOrEmpty(fullName))
        {
            data = assembly.GetManifestResourceStream(fullName);
        }

        return count != 1
            ? Stream.Null
            : data;
    }
}