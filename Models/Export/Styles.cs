using OpenSpreadsheet.Configuration;

namespace OvoData.Models.Export;

public static class Styles
{
    public static ColumnStyle Pounds =>
        new()
        {
            CustomNumberFormat = "£#,##0.00"
        };

    public static ColumnStyle Pence =>
        new()
        {
            CustomNumberFormat = "0.0000\"p\""
        };

    public static ColumnStyle GeneralNumber =>
        new()
        {
            CustomNumberFormat = "#,##0.0##,"
        };

    public static ColumnStyle Kwh =>
        new()
        {
            CustomNumberFormat = "0.00\"kwh\""
        };

    public static ColumnStyle TimeStamp =>
        new()
        {
            CustomNumberFormat = "yyyy-MM-dd HH:mm"
        };
}