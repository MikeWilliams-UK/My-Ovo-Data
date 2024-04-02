namespace OvoData.Models.OvoApi;

public class MonthlyDataItem
{
    public string Mpxn { get; set; }
    public int Month { get; set; }
    public int Year { get; set; }
    public double Consumption { get; set; }
    public Cost Cost { get; set; }
}