using OvoData.Models.Api.Usage;
using OvoData.Models.Database;
using OvoData.Models.Database.Usage;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;

namespace OvoData.Helpers;

public partial class SqLiteHelper
{
    public Information GetInformation()
    {
        var result = new Information();

        using (var connection = GetConnection())
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("SELECT AccountId, FirstMonth, LastMonth, FirstDay, LastDay");
            stringBuilder.AppendLine("FROM Information");

            var command = new SQLiteCommand(stringBuilder.ToString(), connection);
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                result.AccountId = reader["AccountId"] as string ?? string.Empty;
                result.FirstMonth = reader["FirstMonth"] as string ?? string.Empty;
                result.LastMonth = reader["LastMonth"] as string ?? string.Empty;
                result.FirstDay = reader["FirstDay"] as string ?? string.Empty;
                result.LastDay = reader["LastDay"] as string ?? string.Empty;
            }
        }

        return result;
    }

    public void UpsertInformation(Information info)
    {
        using (var connection = GetConnection())
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("INSERT INTO Information");
            stringBuilder.AppendLine("VALUES");
            stringBuilder.AppendLine($"('{info.AccountId}', '{info.FirstMonth}', '{info.LastMonth}', '{info.FirstDay}', '{info.LastDay}')");
            stringBuilder.AppendLine("ON CONFLICT (AccountId)");
            stringBuilder.AppendLine("DO UPDATE SET FirstMonth = excluded.FirstMonth, LastMonth = excluded.LastMonth, FirstDay = excluded.FirstDay, LastDay = excluded.LastDay");

            var command = new SQLiteCommand(stringBuilder.ToString(), connection);
            command.ExecuteNonQuery();
        }
    }

    public int CountMonthy(string fuelType, int year)
    {
        var result = 0;

        using (var connection = GetConnection())
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("SELECT COUNT(1)");
            stringBuilder.AppendLine($"FROM Monthly{fuelType}");
            stringBuilder.AppendLine($"WHERE Month LIKE '{year}%'");

            var command = new SQLiteCommand(stringBuilder.ToString(), connection);
            result = Convert.ToInt32(command.ExecuteScalar());

            Logger.WriteLine($"  Table Monthly{fuelType} has {result} records like '{year}%'");
        }

        return result;
    }

    public void UpsertMonthly(string fuelType, List<MonthlyDataItem> items)
    {
        using (var connection = GetConnection())
        {
            var transaction = connection.BeginTransaction();

            foreach (var item in items)
            {
                var stringBuilder = new StringBuilder();

                double.TryParse(item.Cost.Amount, out var safeCost);

                stringBuilder.AppendLine($"INSERT INTO Monthly{fuelType}");
                stringBuilder.AppendLine("VALUES");
                stringBuilder.AppendLine($"('{item.Year}-{item.Month:D2}', '{item.Mpxn}', {item.Consumption}, {safeCost})");
                stringBuilder.AppendLine("ON CONFLICT (Month)");
                stringBuilder.AppendLine("DO UPDATE SET Mxpn = excluded.Mxpn, Consumption = excluded.Consumption, Cost = excluded.Cost");

                var command = new SQLiteCommand(stringBuilder.ToString(), connection);
                command.ExecuteNonQuery();
            }

            transaction.Commit();
        }
    }

    public List<Monthly> FetchMonthly(string fuelType)
    {
        var result = new List<Monthly>();

        using (var connection = GetConnection())
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("SELECT Month, Consumption, Cost");
            stringBuilder.AppendLine($"FROM Monthly{fuelType}");
            stringBuilder.AppendLine("ORDER BY Month DESC");

            var command = new SQLiteCommand(stringBuilder.ToString(), connection);

            using (var reader = command.ExecuteReader())
            {
                if (reader != null)
                {
                    while (reader.Read())
                    {
                        var dto = new Monthly
                        {
                            Month = FieldAsString(reader["Month"]),
                            Consumption = FieldAsDouble(reader["Consumption"]),
                            Cost = FieldAsDouble(reader["Cost"])
                        };
                        result.Add(dto);
                    }
                }
            }
        }

        return result;
    }

    public int CountDaily(string type, int year, int month)
    {
        var result = 0;

        using (var connection = GetConnection())
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("SELECT COUNT(1)");
            stringBuilder.AppendLine($"FROM Daily{type}");
            stringBuilder.AppendLine($"WHERE Day LIKE '{year}-{month:D2}%'");

            var command = new SQLiteCommand(stringBuilder.ToString(), connection);
            result = Convert.ToInt32(command.ExecuteScalar());

            Logger.WriteLine($"  Table Daily{type} has {result} records like '{year}-{month:D2}%'");
        }

        return result;
    }

    public void UpsertDaily(string fuelType, List<DailyDataItem> items)
    {
        using (var connection = GetConnection())
        {
            var transaction = connection.BeginTransaction();

            foreach (var item in items)
            {
                var stringBuilder = new StringBuilder();

                var day = item.Interval.Start.ToString("yyyy-MM-dd");

                double.TryParse(item.Cost.Amount, out var safeCost);

                stringBuilder.AppendLine($"INSERT INTO Daily{fuelType}");
                stringBuilder.AppendLine("VALUES");
                stringBuilder.AppendLine($"('{day}', {item.Consumption}, {safeCost}, {item.Rates.Standing}, {item.Rates.AnyTime}, {item.Rates.Peak}, {item.Rates.OffPeak}, {item.HasHhData})");
                stringBuilder.AppendLine("ON CONFLICT (Day)");
                stringBuilder.AppendLine("DO UPDATE SET Consumption = excluded.Consumption, Cost = excluded.Cost, Standing = excluded.Standing, AnyTime = excluded.AnyTime, Peak = excluded.Peak, OffPeak = excluded.OffPeak, HasHhData = excluded.HasHhData");

                var command = new SQLiteCommand(stringBuilder.ToString(), connection);
                command.ExecuteNonQuery();
            }

            transaction.Commit();
        }
    }

    public List<Daily> FetchDaily(string fuelType)
    {
        var result = new List<Daily>();

        using (var connection = GetConnection())
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("SELECT Day, Consumption, Cost, Standing, AnyTime, Peak, OffPeak");
            stringBuilder.AppendLine($"FROM Daily{fuelType}");
            stringBuilder.AppendLine("ORDER BY Day DESC");

            var command = new SQLiteCommand(stringBuilder.ToString(), connection);

            using (var reader = command.ExecuteReader())
            {
                if (reader != null)
                {
                    while (reader.Read())
                    {
                        var dto = new Daily
                        {
                            Day = FieldAsString(reader["Day"]),
                            Consumption = FieldAsDouble(reader["Consumption"]),
                            Cost = FieldAsDouble(reader["Cost"]),
                            Standing = FieldAsDouble(reader["Standing"]),
                            AnyTime = FieldAsDouble(reader["AnyTime"]),
                            Peak = FieldAsDouble(reader["Peak"]),
                            OffPeak = FieldAsDouble(reader["OffPeak"])
                        };
                        result.Add(dto);
                    }
                }
            }
        }

        return result;
    }

    public bool HasHalfHourly(string type, int year, int month, int day)
    {
        var result = false;

        using (var connection = GetConnection())
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("SELECT HasHhData");
            stringBuilder.AppendLine($"FROM Daily{type}");
            stringBuilder.AppendLine($"WHERE Day = '{year}-{month:D2}-{day:D2}'");

            var command = new SQLiteCommand(stringBuilder.ToString(), connection);
            result = Convert.ToBoolean(command.ExecuteScalar());
        }

        Logger.WriteLine($"  Daily{type} Half Hour records for '{year}-{month:D2}-{day:D2}' are available {result}");
        return result;
    }

    public int CountHalfHourly(string type, int year, int month, int day)
    {
        var result = 0;

        using (var connection = GetConnection())
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("SELECT COUNT(1)");
            stringBuilder.AppendLine($"FROM HalfHourly{type}");
            stringBuilder.AppendLine($"WHERE StartTime LIKE '{year}-{month:D2}-{day:D2}%'");

            var command = new SQLiteCommand(stringBuilder.ToString(), connection);
            result = Convert.ToInt32(command.ExecuteScalar());

            Logger.WriteLine($"  Table Daily{type} has {result} records like '{year}-{month:D2}-{day:D2}%'");
        }

        return result;
    }

    public void UpsertHalfHourly(string fuelType, List<HalfHourlyDataItem> items)
    {
        using (var connection = GetConnection())
        {
            var transaction = connection.BeginTransaction();

            foreach (var item in items)
            {
                var stringBuilder = new StringBuilder();

                var timeStamp = item.Interval.Start.ToString("yyyy-MM-dd HH:mm:ss");

                stringBuilder.AppendLine($"INSERT INTO HalfHourly{fuelType}");
                stringBuilder.AppendLine("VALUES");
                stringBuilder.AppendLine($"('{timeStamp}', {item.Consumption})");
                stringBuilder.AppendLine("ON CONFLICT (StartTime)");
                stringBuilder.AppendLine("DO UPDATE SET Consumption = excluded.Consumption");

                var command = new SQLiteCommand(stringBuilder.ToString(), connection);
                command.ExecuteNonQuery();
            }

            transaction.Commit();
        }
    }

    public List<HalfHourly> FetchHalfHourly(string fuelType)
    {
        var result = new List<HalfHourly>();

        using (var connection = GetConnection())
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("SELECT StartTime, Consumption");
            stringBuilder.AppendLine($"FROM HalfHourly{fuelType}");
            stringBuilder.AppendLine("ORDER BY StartTime DESC");

            var command = new SQLiteCommand(stringBuilder.ToString(), connection);

            using (var reader = command.ExecuteReader())
            {
                if (reader != null)
                {
                    while (reader.Read())
                    {
                        var dto = new HalfHourly
                        {
                            StartTime = FieldAsString(reader["StartTime"]),
                            Consumption = FieldAsDouble(reader["Consumption"])
                        };
                        result.Add(dto);
                    }
                }
            }
        }

        return result;
    }
}