using OvoData.Models;
using OvoData.Models.Database.Readings;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Text;

namespace OvoData.Helpers;

public partial class SqLiteHelper
{
    public void UpsertSupplyPoint(MySupplyPoint supplyPoint)
    {
        using (var connection = GetConnection())
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("INSERT INTO SupplyPoints");
            stringBuilder.AppendLine("VALUES");
            stringBuilder.AppendLine($"('{supplyPoint.Sprn}', '{supplyPoint.FuelType}')");
            stringBuilder.AppendLine("ON CONFLICT (Sprn)");
            stringBuilder.AppendLine("DO UPDATE SET Sprn = excluded.Sprn, FuelType = excluded.FuelType");

            var command = new SQLiteCommand(stringBuilder.ToString(), connection);
            command.ExecuteNonQuery();
        }
    }

    public void UpsertMeter(SqLiteMeter meter, string fuelType)
    {
        using (var connection = GetConnection())
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("INSERT INTO Meters");
            stringBuilder.AppendLine("VALUES");
            stringBuilder.AppendLine($"('{meter.SerialNumber}', '{fuelType}', '{meter.FuelType}', '{meter.Status}')");
            stringBuilder.AppendLine("ON CONFLICT (SerialNumber)");
            stringBuilder.AppendLine("DO UPDATE SET SerialNumber = excluded.SerialNumber, FuelType = excluded.FuelType, MeterType = excluded.MeterType, Status = excluded.Status");

            var command = new SQLiteCommand(stringBuilder.ToString(), connection);
            command.ExecuteNonQuery();
        }
    }

    public void UpsertMeterRegisters(SqLiteRegister register, string fuelType)
    {
        using (var connection = GetConnection())
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("INSERT INTO MeterRegisters");
            stringBuilder.AppendLine("VALUES");
            stringBuilder.AppendLine($"('{register.StartDate}', '{register.EndDate}', '{fuelType}',");
            stringBuilder.AppendLine($" '{register.Id}','{register.TimingCategory}', '{register.UnitOfMeasurement}')");
            stringBuilder.AppendLine("ON CONFLICT (StartDate)");
            stringBuilder.AppendLine("DO UPDATE SET");
            stringBuilder.AppendLine("  StartDate = excluded.StartDate, EndDate = excluded.EndDate, FuelType = excluded.FuelType,");
            stringBuilder.AppendLine("  Id = excluded.Id, TimingCategory = excluded.TimingCategory, UnitOfMeasurement = excluded.UnitOfMeasurement");

            var command = new SQLiteCommand(stringBuilder.ToString(), connection);
            command.ExecuteNonQuery();
        }
    }

    public void UpsertMeterReading(SqLiteReading reading, string fuelType)
    {
        using (var connection = GetConnection())
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("INSERT INTO MeterReadings");
            stringBuilder.AppendLine("VALUES");
            stringBuilder.AppendLine($"('{reading.Date}', '{reading.MeterSerialNumber}', '{fuelType}',");
            stringBuilder.AppendLine($" '{reading.LifeCycle}','{reading.RegisterId}', '{reading.Source}',");
            stringBuilder.AppendLine($" '{reading.TimingCategory}','{reading.FuelType}', '{reading.Value}')");
            stringBuilder.AppendLine("ON CONFLICT (Date, FuelType)");
            stringBuilder.AppendLine("DO UPDATE SET");
            stringBuilder.AppendLine("  Date = excluded.Date, FuelType = excluded.FuelType, MeterSerialNumber = excluded.MeterSerialNumber,");
            stringBuilder.AppendLine("  LifeCycle = excluded.LifeCycle, RegisterId = excluded.RegisterId, Source = excluded.Source,");
            stringBuilder.AppendLine("  TimingCategory = excluded.TimingCategory, FuelType = excluded.FuelType, Value = excluded.Value");

            var command = new SQLiteCommand(stringBuilder.ToString(), connection);
            command.ExecuteNonQuery();
        }
    }

    public List<SqLiteRegister> FetchMeterRegisters()
    {
        var result = new List<SqLiteRegister>();

        using (var connection = GetConnection())
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("SELECT StartDate, EndDate, FuelType, TimingCategory, UnitOfMeasurement");
            stringBuilder.AppendLine("FROM MeterRegisters");
            stringBuilder.AppendLine("ORDER BY StartDate DESC, FuelType ASC");

            var command = new SQLiteCommand(stringBuilder.ToString(), connection);

            using (var reader = command.ExecuteReader())
            {
                if (reader != null)
                {
                    while (reader.Read())
                    {
                        var dto = new SqLiteRegister()
                        {
                            StartDate = FieldAsString(reader["StartDate"]),
                            EndDate = FieldAsString(reader["EndDate"]),
                            FuelType = FieldAsString(reader["FuelType"]),
                            TimingCategory = FieldAsString(reader["TimingCategory"]),
                            UnitOfMeasurement = FieldAsString(reader["UnitOfMeasurement"])
                        };
                        result.Add(dto);
                    }
                }
            }
        }

        return result;
    }

    public List<SqLiteReading> FetchMeterReadings()
    {
        var result = new List<SqLiteReading>();

        using (var connection = GetConnection())
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("SELECT Date, FuelType, TimingCategory, Value");
            stringBuilder.AppendLine("FROM MeterReadings");
            stringBuilder.AppendLine("ORDER BY Date DESC, FuelType ASC");

            var command = new SQLiteCommand(stringBuilder.ToString(), connection);

            using (var reader = command.ExecuteReader())
            {
                if (reader != null)
                {
                    while (reader.Read())
                    {
                        var dto = new SqLiteReading()
                        {
                            Date = FieldAsString(reader["Date"]),
                            FuelType = FieldAsString(reader["FuelType"]),
                            TimingCategory = FieldAsString(reader["TimingCategory"]),
                            Value = FieldAsString(reader["Value"])
                        };
                        result.Add(dto);
                    }
                }
            }
        }

        return result;
    }
}