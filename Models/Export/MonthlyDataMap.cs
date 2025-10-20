using OpenSpreadsheet.Configuration;
using OpenSpreadsheet.Enums;

namespace OvoData.Models.Export;

public sealed class MonthlyDataMap : ClassMap<MonthlyData>
{
    public MonthlyDataMap()
    {
        Map(x => x.Month).Name("Date").ColumnType(ColumnType.Text);

        Map(x => x.ElectricKwh).Name("Electric kWh").ColumnType(ColumnType.Number).Style(Styles.Kwh);
        Map(x => x.ElectricCost).Name("Electric Cost").ColumnType(ColumnType.Number).Style(Styles.Pounds);

        Map(x => x.GasKwh).Name("Gas kWh").ColumnType(ColumnType.Number).Style(Styles.Kwh);
        Map(x => x.GasCost).Name("Gas Cost").ColumnType(ColumnType.Number).Style(Styles.Pounds);
    }
}