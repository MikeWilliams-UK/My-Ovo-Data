namespace OvoData.Models.Api.Usage;

public class DailyDataItem
{
    public bool HasHhData { get; set; }
    public double Consumption { get; set; }

    public Interval Interval { get; set; } = new();

    public Cost Cost { get; set; } = new();
    public Rates Rates { get; set; } = new();
}