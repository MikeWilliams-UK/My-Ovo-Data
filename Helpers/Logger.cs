using System;
using System.Diagnostics;
using System.IO;

namespace OvoData.Helpers;

public static class Logger
{
    public static void WriteLine(string message)
    {
        if (!string.IsNullOrEmpty(message))
        {
            using (var streamWriter = File.AppendText(GetFileName()))
            {
                streamWriter.WriteLine($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
                Debug.WriteLine(message);
            }
        }
    }

    private static string GetFileName()
    {
        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), Constants.ApplicationName);

        if (!Directory.Exists(Path.Combine(folder, "Logs")))
        {
            Directory.CreateDirectory(Path.Combine(folder, "Logs"));
        }

        var fileName = Path.Combine(folder, "Logs", $"{DateTime.Now:yyyy-MM-dd}.log");

        return fileName;
    }

    public static void DumpJson(string responseType, string json)
    {
        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), Constants.ApplicationName);

        if (!Directory.Exists(Path.Combine(folder, "Dump")))
        {
            Directory.CreateDirectory(Path.Combine(folder, "Dump"));
        }

        var fileName = Path.Combine(folder, "Dump", $"{DateTime.Now:yyyy-MM-dd} {responseType}.json");

        File.WriteAllText(fileName, json);
    }
}