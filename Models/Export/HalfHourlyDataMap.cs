using OpenSpreadsheet.Configuration;
using OpenSpreadsheet.Enums;
using System;
using System.Globalization;

namespace OvoData.Models.Export;

public sealed class HalfHourlyDataMap : ClassMap<HalfHourlyData>
{
    public HalfHourlyDataMap()
    {
        Map(x => x.StartTime).Name("Start Time").ColumnType(ColumnType.Date).Style(Styles.TimeStamp)
            .WriteUsing(r =>
            {
                return DateTime.ParseExact(r.StartTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            });
        Map(x => x.ElectricKwh).Name("Electric kWh").ColumnType(ColumnType.Number).Style(Styles.Kwh);
        Map(x => x.GasKwh).Name("Gas kWh").ColumnType(ColumnType.Number).Style(Styles.Kwh);
    }
}