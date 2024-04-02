namespace OvoData.Models.OvoApi;

public class DailyDataItem
{
    public bool HasHhData { get; set; }
    public double Consumption { get; set; }

    public Interval Interval { get; set; }

    public Cost Cost { get; set; }
    public Rates Rates { get; set; }
}