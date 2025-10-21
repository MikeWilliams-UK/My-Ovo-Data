using System;

namespace OvoData.Helpers;

public static class DateHelper
{
    public static string IsoDateOnly(DateTime value)
    {
        return $"{value:yyyy-MM-dd}";
    }

    public static string LogFileSuffix()
    {
        return $"{DateTime.Now:yyyy-MM-dd HH-mm-ss.fff}";
    }

    public static string LogFileSuffix(string suffix)
    {
        return $"{DateTime.Now:yyyy-MM-dd} {suffix}";
    }

    public static string LogEntryTimestamp()
    {
        return $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}";
    }
}