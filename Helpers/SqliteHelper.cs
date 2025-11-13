using OvoData.Models;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Text;

namespace OvoData.Helpers;

public partial class SqLiteHelper
{
    private readonly string _dataFile;
    private Logger _logger;

    public SqLiteHelper(string account, Logger logger)
    {
        _logger = logger;

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
        if (!ObjectExists("table", "SupplyPoints"))
        {
            ApplyV105Changes();
        }

        // Add readings tables if required
        if (!ObjectExists("index", "Idx_MeterRegisters_2"))
        {
            ApplyV107Changes();
        }
    }

    private SQLiteConnection GetConnection()
    {
        var conn = new SQLiteConnection($"Data Source={_dataFile};Synchronous=Full");
        return conn.OpenAndReturn();
    }

    private void CreateInitialTables()
    {
        var statements = ResourceHelper.GetStringResource("SqLite.Initial-Database.sql")
            .Split(Environment.NewLine);

        ExecuteStatements(statements);
    }

    private void ApplyV105Changes()
    {
        var statements = ResourceHelper.GetStringResource("SqLite.V1.0.5-Changes.sql")
            .Split(Environment.NewLine);

        ExecuteStatements(statements);
    }

    private void ApplyV107Changes()
    {
        var statements = ResourceHelper.GetStringResource("SqLite.V1.0.7-Changes.sql")
            .Split(Environment.NewLine);

        ExecuteStatements(statements);
    }


    private void ExecuteStatements(string[] statements)
    {
        using (var connection = GetConnection())
        {
            foreach (var statement in statements)
            {
                if (!string.IsNullOrEmpty(statement) && !statement.StartsWith('-'))
                {
                    var command = new SQLiteCommand(statement, connection);
                    command.ExecuteNonQuery();
                }
            }
        }
    }

    public List<MySummary> GetUsageInformation()
    {
        var result = new List<MySummary>();

        using (var connection = GetConnection())
        {
            // Electric first
            GetMonthlyUsageMetric(connection, StringHelper.ProperCase(Constants.FuelTypeElectric));
            GetDailyUsageMetric(connection, StringHelper.ProperCase(Constants.FuelTypeElectric));
            GetHalfHourlyUsageMetric(connection, StringHelper.ProperCase(Constants.FuelTypeElectric));
            GetElectricityReadingMetric(connection);
            // Then Gas
            GetMonthlyUsageMetric(connection, StringHelper.ProperCase(Constants.FuelTypeGas));
            GetDailyUsageMetric(connection, StringHelper.ProperCase(Constants.FuelTypeGas));
            GetHalfHourlyUsageMetric(connection, StringHelper.ProperCase(Constants.FuelTypeGas));
            GetGasReadingMetric(connection);
        }

        return result;

        void GetMonthlyUsageMetric(SQLiteConnection connection, string fuelType)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("SELECT MAX(Month) AS Max, MIN(Month) AS Min, Count(1) AS Count");
            stringBuilder.AppendLine($"FROM Monthly{fuelType}");

            var command = new SQLiteCommand(stringBuilder.ToString(), connection);
            var reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    ExtractMetric(reader, "Monthly", fuelType);
                }
            }
        }

        void GetDailyUsageMetric(SQLiteConnection connection, string fuelType)
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
                    ExtractMetric(reader, "Daily", fuelType);
                }
            }
        }

        void GetHalfHourlyUsageMetric(SQLiteConnection connection, string fuelType)
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("SELECT MAX(StartTime) AS Max, MIN(StartTime) AS Min, Count(1) AS Count");
            stringBuilder.AppendLine($"FROM HalfHourly{fuelType}");

            var command = new SQLiteCommand(stringBuilder.ToString(), connection);
            var reader = command.ExecuteReader();
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    ExtractMetric(reader, "Half Hourly", fuelType);
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
                    ExtractMetric(reader, "Meter Readings", Constants.FuelTypeElectric);
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
                    ExtractMetric(reader, "Meter Readings", Constants.FuelTypeGas);
                }
            }
        }

        void ExtractMetric(SQLiteDataReader reader, string metric, string fuelType)
        {
            var from = FieldAsString(reader["Min"]);
            var to = FieldAsString(reader["Max"]);
            var count = FieldAsInt(reader["count"]);

            if (from.Length > 16)
            {
                from = from.Substring(0, 16);
            }
            if (to.Length > 16)
            {
                to = to.Substring(0, 16);
            }

            if (!string.IsNullOrEmpty(from) && !string.IsNullOrEmpty(to))
            {
                var info = new MySummary
                {
                    FuelType = StringHelper.ProperCase(fuelType),
                    Metric = metric,
                    From = from,
                    To = to,
                    Records = $"{count:#,##0}"
                };

                result.Add(info);
            }
        }
    }

    private bool ObjectExists(string objectType, string objectName)
    {
        bool result = false;

        using (var connection = GetConnection())
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("SELECT name");
            stringBuilder.AppendLine("FROM sqlite_master");
            stringBuilder.AppendLine($"WHERE type='{objectType}' AND name='{objectName}'");

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