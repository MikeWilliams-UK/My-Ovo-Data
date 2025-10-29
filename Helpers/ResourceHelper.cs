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