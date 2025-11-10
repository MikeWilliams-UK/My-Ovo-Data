using System;
using System.Diagnostics;
using System.IO;

namespace OvoData.Helpers;

public class Logger
{
    private readonly string _suffix;

    public Logger(ref int logNumber)
    {
        logNumber++;

        _suffix = $"{Environment.ProcessId:X6}-{logNumber:000}";
    }

    public void WriteLine(string message)
    {
        if (!string.IsNullOrEmpty(message))
        {
            using (var streamWriter = File.AppendText(GetFileName()))
            {
                streamWriter.WriteLine($"{DateHelper.LogEntryTimestamp()} - {message}");
                Debug.WriteLine(message);
            }
        }
    }

    private string GetFileName()
    {
        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), Constants.ApplicationName);

        if (!Directory.Exists(Path.Combine(folder, "Logs")))
        {
            Directory.CreateDirectory(Path.Combine(folder, "Logs"));
        }

        var fileName = Path.Combine(folder, "Logs", $"{DateHelper.LogFileSuffix(_suffix)}.log");

        return fileName;
    }

    public void DumpJson(string responseType, string json)
    {
        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), Constants.ApplicationName);

        if (!Directory.Exists(Path.Combine(folder, "Dump")))
        {
            Directory.CreateDirectory(Path.Combine(folder, "Dump"));
        }

        var fileName = Path.Combine(folder, "Dump", $"{DateHelper.LogFileSuffix()} {responseType}.json");

        File.WriteAllText(fileName, json);
    }
}