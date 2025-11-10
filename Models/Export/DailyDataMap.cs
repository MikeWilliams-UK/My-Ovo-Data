using OpenSpreadsheet.Configuration;
using OpenSpreadsheet.Enums;

namespace OvoData.Models.Export;

public sealed class DailyDataMap : ClassMap<DailyData>
{
    public DailyDataMap()
    {
        Map(x => x.Day).Name("Day").ColumnType(ColumnType.Text);

        Map(x => x.ElectricKwh).Name("Electric kWh").ColumnType(ColumnType.Number).Style(Styles.Kwh);
        Map(x => x.ElectricStanding).Name("Electric Standing").ColumnType(ColumnType.Number).Style(Styles.Pence);
        Map(x => x.ElectricAnyTime).Name("Electric AnyTime").ColumnType(ColumnType.Number).Style(Styles.Pence);
        Map(x => x.ElectricPeak).Name("Electric Peak").ColumnType(ColumnType.Number).Style(Styles.Pence);
        Map(x => x.ElectricOffPeak).Name("Electric Off Peak").ColumnType(ColumnType.Number).Style(Styles.Pence);
        Map(x => x.ElectricCost).Name("Electric Cost").ColumnType(ColumnType.Number).Style(Styles.Pounds);

        Map(x => x.GasKwh).Name("Gas kWh").ColumnType(ColumnType.Number).Style(Styles.Kwh);
        Map(x => x.GasStanding).Name("Gas Standing").ColumnType(ColumnType.Number).Style(Styles.Pence);
        Map(x => x.GasAnyTime).Name("Gas AnyTime").ColumnType(ColumnType.Number).Style(Styles.Pence);
        Map(x => x.GasCost).Name("Gas Cost").ColumnType(ColumnType.Number).Style(Styles.Pounds);
    }
}