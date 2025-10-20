using System;
using System.Data.SQLite;
using System.IO;
using System.Text;

namespace OvoData.Helpers;

public partial class SqLiteHelper
{
    private readonly string _dataFile;

    public SqLiteHelper(string account)
    {
        var folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), Constants.ApplicationName);
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        _dataFile = Path.Combine(folder, $"{account}.db");

        // Create database if required
        if (!File.Exists(_dataFile))
        {
            SQLiteConnection.CreateFile(_dataFile);
            CreateBaseTables();
        }

        // Add readings tables if required
        if (!TableExists("SupplyPointsInformation"))
        {
            CreateReadingsTables();
        }
    }

    private SQLiteConnection GetConnection()
    {
        var conn = new SQLiteConnection($"Data Source={_dataFile};Synchronous=Full");
        return conn.OpenAndReturn();
    }

    private void CreateBaseTables()
    {
        var tables = ResourceHelper.GetStringResource("SQLite.Create-Initial-Database.sql").Split(Environment.NewLine);

        using (var connection = GetConnection())
        {
            foreach (var table in tables)
            {
                if (!string.IsNullOrEmpty(table) && !table.StartsWith("-"))
                {
                    var command = new SQLiteCommand(table, connection);
                    command.ExecuteNonQuery();
                }
            }
        }
    }

    private void CreateReadingsTables()
    {
        var tables = ResourceHelper.GetStringResource("SqLite.Add-Meter-Readings-Tables.sql").Split(Environment.NewLine);

        using (var connection = GetConnection())
        {
            foreach (var table in tables)
            {
                if (!string.IsNullOrEmpty(table) && !table.StartsWith("-"))
                {
                    var command = new SQLiteCommand(table, connection);
                    command.ExecuteNonQuery();
                }
            }
        }
    }

    private bool TableExists(string tableName)
    {
        bool result = false;

        using (var connection = GetConnection())
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("SELECT name");
            stringBuilder.AppendLine("FROM sqlite_master");
            stringBuilder.AppendLine($"WHERE type='table' AND name='{tableName}'");

            var command = new SQLiteCommand(stringBuilder.ToString(), connection);
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result = true;
            }
        }

        return result;
    }

    private string FieldAsString(object field)
    {
        return $"{field}";
    }

    private double FieldAsDouble(object field)
    {
        var temp = $"{field}";
        if (string.IsNullOrEmpty(temp))
        {
            return 0;
        }
        else
        {
            return double.Parse(temp);
        }
    }
}