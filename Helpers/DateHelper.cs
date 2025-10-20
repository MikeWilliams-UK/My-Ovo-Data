using System;

namespace OvoData.Helpers;

public static class DateHelper
{
    public static string IsoDateOnly(DateTime value)
    {
        return $"{value:yyyy-MM-dd}";
    }

    public static string LogFileSuffix(bool addMs = false)
    {
        return addMs
            ? $"{DateTime.Now:yyyy-MM-dd HH-mm-ss.fff}"
            : $"{DateTime.Now:yyyy-MM-dd HH-mm-ss}";
    }


    public static string LogEntryTimestamp()
    {
        return $"{DateTime.Now:yyyy-MM-dd HH;mm:ss.fff}";
    }

}