using Microsoft.Extensions.Configuration;
using System;

namespace OvoData.Helpers;

public static class ConfigHelper
{
    public static bool GetBoolean(IConfigurationRoot config, string key, bool defaultValue)
    {
        var result = defaultValue;

        if (config != null)
        {
            try
            {
                var value = config[key];
                if (bool.TryParse(value, out result))
                {
                    return result;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        return result;
    }
}