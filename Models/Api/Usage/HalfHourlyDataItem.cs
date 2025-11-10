namespace OvoData.Models.Api.Usage;

public class HalfHourlyDataItem
{
    public double Consumption { get; set; }
    public Interval Interval { get; set; } = new();
    public string Unit { get; set; } = string.Empty;
}