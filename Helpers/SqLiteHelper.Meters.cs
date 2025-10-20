using Microsoft.Win32;
using OvoData.Models.Database;
using OvoData.Models.Database.Readings;
using System.Data.SQLite;
using System.Text;
using Meter = OvoData.Models.Database.Readings.Meter;

namespace OvoData.Helpers;

public partial class SqLiteHelper
{
    public void UpsertSupplyPoint(SupplyPoint supplyPoint)
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

    public void UpsertMeter(Meter meter, string fuelType)
    {
        using (var connection = GetConnection())
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("INSERT INTO Meters");
            stringBuilder.AppendLine("VALUES");
            stringBuilder.AppendLine($"('{meter.SerialNumber}', '{fuelType}', '{meter.Type}', '{meter.Status}')");
            stringBuilder.AppendLine("ON CONFLICT (SerialNumber)");
            stringBuilder.AppendLine("DO UPDATE SET SerialNumber = excluded.SerialNumber, FuelType = excluded.FuelType, MeterType = excluded.MeterType, Status = excluded.Status");

            var command = new SQLiteCommand(stringBuilder.ToString(), connection);
            command.ExecuteNonQuery();
        }
    }

    public void UpsertMeterRegisters(Register register, string fuelType)
    {
        using (var connection = GetConnection())
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("INSERT INTO MeterRegisters");
            stringBuilder.AppendLine("VALUES");
            stringBuilder.AppendLine($"('{register.StartDate }', '{register.EndDate}', '{fuelType}',");
            stringBuilder.AppendLine($" '{register.Id}','{register.TimingCategory}', '{register.UnitOfMeasurement}')");
            stringBuilder.AppendLine("ON CONFLICT (StartDate)");
            stringBuilder.AppendLine("DO UPDATE SET");
            stringBuilder.AppendLine("  StartDate = excluded.StartDate, EndDate = excluded.EndDate, FuelType = excluded.FuelType,");
            stringBuilder.AppendLine("  Id = excluded.Id, TimingCategory = excluded.TimingCategory, UnitOfMeasurement = excluded.UnitOfMeasurement");

            var command = new SQLiteCommand(stringBuilder.ToString(), connection);
            command.ExecuteNonQuery();
        }

    }

    public void UpsertMeterReading(Reading reading, string fuelType)
    {
        using (var connection = GetConnection())
        {
            var stringBuilder = new StringBuilder();

            stringBuilder.AppendLine("INSERT INTO MeterReadings");
            stringBuilder.AppendLine("VALUES");
            stringBuilder.AppendLine($"('{reading.Date}', '{reading.MeterSerialNumber}', '{fuelType}',");
            stringBuilder.AppendLine($" '{reading.LifeCycle}','{reading.RegisterId}', '{reading.Source}',");
            stringBuilder.AppendLine($" '{reading.TimingCategory}','{reading.Type}', '{reading.Value}')");
            stringBuilder.AppendLine("ON CONFLICT (Date)");
            stringBuilder.AppendLine("DO UPDATE SET");
            stringBuilder.AppendLine("  Date = excluded.Date, MeterSerialNumber = excluded.MeterSerialNumber, FuelType = excluded.FuelType,");
            stringBuilder.AppendLine("  LifeCycle = excluded.LifeCycle, RegisterId = excluded.RegisterId, Source = excluded.Source,");
            stringBuilder.AppendLine("  TimingCategory = excluded.TimingCategory, Type = excluded.Type, Value = excluded.Value");

            var command = new SQLiteCommand(stringBuilder.ToString(), connection);
            command.ExecuteNonQuery();
        }
    }
}