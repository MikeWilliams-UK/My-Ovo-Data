using System.Globalization;

namespace OvoData.Helpers;

public static class StringHelper
{
    public static string ProperCase(string value)
    {
        return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value.ToLower());
    }
}