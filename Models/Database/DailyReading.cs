namespace OvoData.Models.Database;

public class DailyReading
{
    public string Day { get; set; }
    public double Consumption { get; set; }
    public double Standing { get; set; }
    public double AnyTime { get; set; }
    public double Peak { get; set; }
    public double OffPeak { get; set; }
    public double Cost { get; set; }
}