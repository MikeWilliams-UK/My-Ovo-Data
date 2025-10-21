using OvoData.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
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
            CreateInitialTables();
        }

        // Add readings tables if required
        if (!TableExists("SupplyPoints"))
        {
            ApplyV105Changes();
        }
    }

    private SQLiteConnection GetConnection()
    {
        var conn = new SQLiteConnection($"Data Source={_dataFile};Synchronous=Full");
        return conn.OpenAndReturn();
    }

    private void CreateInitialTables()
    {
        var tables = ResourceHelper.GetStringResource("SQLite.Initial-Database.sql").Split(Environment.NewLine);

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

    private void ApplyV105Changes()
    {
        var tables = ResourceHelper.GetStringResource("SqLite.V1.0.5-Changes.sql").Split(Environment.NewLine);

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

    public List<Summary> GetUsageInformation()
    {
        var result = new List<Summary>();

        using (var connection = GetConnection())
        {
            GetUsageMetric(connection, StringHelper.ProperCase(Constants.FuelTypeElectric));
            GetUsageMetric(connection, StringHelper.ProperCase(Constants.FuelTypeGas));
            GetElectricityReadingMetric(connection);
            GetGasReadingMetric(connection);
        }

        return result;

        void GetUsageMetric(SQLiteConnection connection, string fuelType)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("SELECT MAX(Day) AS Max, MIN(Day) AS Min, Count(1) AS Count");
            stringBuilder.AppendLine($"FROM Daily{fuelType}");

            var command = new SQLiteCommand(stringBuilder.ToString(), connection);
            var reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    ExtractMetric(reader, "Usage", $"Daily{fuelType}", fuelType);
                }
            }
        }

        void GetElectricityReadingMetric(SQLiteConnection connection)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("SELECT MAX(Date) AS Max, MIN(Date) AS Min, Count(1) AS Count");
            stringBuilder.AppendLine("FROM MeterReadings");
            stringBuilder.AppendLine($"WHERE FuelType = '{Constants.FuelTypeElectricity}'");

            var command = new SQLiteCommand(stringBuilder.ToString(), connection);
            var reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    ExtractMetric(reader, "Readings", "MeterReadings", Constants.FuelTypeElectric);
                }
            }
        }

        void GetGasReadingMetric(SQLiteConnection connection)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("SELECT MAX(Date) AS Max, MIN(Date) AS Min, Count(1) AS Count");
            stringBuilder.AppendLine("FROM MeterReadings");
            stringBuilder.AppendLine($"WHERE FuelType = '{Constants.FuelTypeGas}'");

            var command = new SQLiteCommand(stringBuilder.ToString(), connection);
            var reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    ExtractMetric(reader, "Readings", "MeterReadings", Constants.FuelTypeGas);
                }
            }
        }

        void ExtractMetric(SQLiteDataReader reader, string metric, string tableName, string fuelType)
        {
            var from = FieldAsString(reader["Min"]);
            var to = FieldAsString(reader["Max"]);
            var count = FieldAsInt(reader["count"]);

            Debug.WriteLine($"Record Count for {tableName} ({fuelType}) is {count}");

            if (!string.IsNullOrEmpty(from) && !string.IsNullOrEmpty(to))
            {
                var info = new Summary
                {
                    FuelType = StringHelper.ProperCase(fuelType),
                    InfoType = metric,
                    From = from,
                    To = to
                };

                result.Add(info);
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
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    result = true;
                }
            }
        }

        return result;
    }

    private string FieldAsString(object field)
    {
        return $"{field}";
    }

    private int FieldAsInt(object field)
    {
        var temp = $"{field}";
        if (string.IsNullOrEmpty(temp))
        {
            return 0;
        }
        else
        {
            return int.Parse(temp);
        }
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