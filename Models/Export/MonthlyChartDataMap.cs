using OpenSpreadsheet.Configuration;
using OpenSpreadsheet.Enums;

namespace OvoData.Models.Export;

public sealed class MonthlyChartDataMap : ClassMap<MonthlyChartData>
{
    public MonthlyChartDataMap()
    {
        Map(x => x.Year).Name("Year").ColumnType(ColumnType.Text);

        Map(x => x.JanKwh).Name("Jan Kwh").ColumnType(ColumnType.Number).Style(Styles.Kwh);
        Map(x => x.FebKwh).Name("Feb Kwh").ColumnType(ColumnType.Number).Style(Styles.Kwh);
        Map(x => x.MarKwh).Name("Mar Kwh").ColumnType(ColumnType.Number).Style(Styles.Kwh);
        Map(x => x.AprKwh).Name("Apr Kwh").ColumnType(ColumnType.Number).Style(Styles.Kwh);
        Map(x => x.MayKwh).Name("May Kwh").ColumnType(ColumnType.Number).Style(Styles.Kwh);
        Map(x => x.JunKwh).Name("Jun Kwh").ColumnType(ColumnType.Number).Style(Styles.Kwh);
        Map(x => x.JulKwh).Name("Jul Kwh").ColumnType(ColumnType.Number).Style(Styles.Kwh);
        Map(x => x.AugKwh).Name("Aug Kwh").ColumnType(ColumnType.Number).Style(Styles.Kwh);
        Map(x => x.SepKwh).Name("Sep Kwh").ColumnType(ColumnType.Number).Style(Styles.Kwh);
        Map(x => x.OctKwh).Name("Oct Kwh").ColumnType(ColumnType.Number).Style(Styles.Kwh);
        Map(x => x.NovKwh).Name("Nov Kwh").ColumnType(ColumnType.Number).Style(Styles.Kwh);
        Map(x => x.DecKwh).Name("Dec Kwh").ColumnType(ColumnType.Number).Style(Styles.Kwh);

        Map(x => x.JanCost).Name("Jan Cost").ColumnType(ColumnType.Number).Style(Styles.Pounds);
        Map(x => x.FebCost).Name("Feb Cost").ColumnType(ColumnType.Number).Style(Styles.Pounds);
        Map(x => x.MarCost).Name("Mar Cost").ColumnType(ColumnType.Number).Style(Styles.Pounds);
        Map(x => x.AprCost).Name("Apr Cost").ColumnType(ColumnType.Number).Style(Styles.Pounds);
        Map(x => x.MayCost).Name("Jan Cost").ColumnType(ColumnType.Number).Style(Styles.Pounds);
        Map(x => x.JunCost).Name("Jun Cost").ColumnType(ColumnType.Number).Style(Styles.Pounds);
        Map(x => x.JulCost).Name("Jul Cost").ColumnType(ColumnType.Number).Style(Styles.Pounds);
        Map(x => x.AugCost).Name("Aug Cost").ColumnType(ColumnType.Number).Style(Styles.Pounds);
        Map(x => x.SepCost).Name("Sep Cost").ColumnType(ColumnType.Number).Style(Styles.Pounds);
        Map(x => x.OctCost).Name("Oct Cost").ColumnType(ColumnType.Number).Style(Styles.Pounds);
        Map(x => x.NovCost).Name("Nov Cost").ColumnType(ColumnType.Number).Style(Styles.Pounds);
        Map(x => x.DecCost).Name("Dec Cost").ColumnType(ColumnType.Number).Style(Styles.Pounds);
    }
}