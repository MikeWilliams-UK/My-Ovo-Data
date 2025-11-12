using OpenSpreadsheet.Configuration;
using OpenSpreadsheet.Enums;

namespace OvoData.Models.Export;

public class ReadingsDataMap : ClassMap<ReadingsData>
{
    public ReadingsDataMap()
    {
        Map(x => x.Date).Name("Date").ColumnType(ColumnType.Text);

        Map(x => x.FuelType).Name("Fuel FuelType").ColumnType(ColumnType.Text);
        Map(x => x.Category).Name("Category").ColumnType(ColumnType.Text);
        Map(x => x.Value).Name("Reading").ColumnType(ColumnType.Number).Style(Styles.GeneralNumber);
        Map(x => x.UnitOfMeasure).Name("Units").ColumnType(ColumnType.Text);
    }
}