using OpenSpreadsheet.Configuration;
using OpenSpreadsheet.Enums;

namespace OvoData.Models.Export;

public class ReadingsDataMap : ClassMap<ReadingsData>
{
    public ReadingsDataMap()
    {
        Map(x => x.Date).Name("Date").ColumnType(ColumnType.Text);

        Map(x => x.Type).Name("Fuel Type").ColumnType(ColumnType.Text);
        Map(x => x.Value).Name("Reading").ColumnType(ColumnType.Number).Style(Styles.GeneralNumber);
    }
}