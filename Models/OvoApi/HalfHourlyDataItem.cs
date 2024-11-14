namespace OvoData.Models.OvoApi;

public class HalfHourlyDataItem
{
    public double Consumption { get; set; }
    public Interval Interval { get; set; } = new();
    public string Unit { get; set; } = string.Empty;
}