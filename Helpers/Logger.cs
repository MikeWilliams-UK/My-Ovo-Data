using DocumentFormat.OpenXml.Bibliography;
using System;
using System.Diagnostics;
using System.IO;

namespace OvoData.Helpers;

public class Logger
{
    private string _suffix;

    public Logger()
    {
        var startOfYear = new DateTime(DateTime.Today.Year, 1, 1, 0,0,0, DateTimeKind.Utc);

        var deltaDays = DateTime.UtcNow - startOfYear;
        var deltaSeconds = DateTime.Now - DateTime.Now.Date;

        _suffix = $"{Math.Floor(deltaDays.TotalDays)}.{Math.Floor(deltaSeconds.TotalSeconds)}";
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