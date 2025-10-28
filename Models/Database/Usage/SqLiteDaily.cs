namespace OvoData.Models.Database.Usage;

public class SqLiteDaily
{
    public string Day { get; set; } = string.Empty;
    public double Consumption { get; set; }
    public double Standing { get; set; }
    public double AnyTime { get; set; }
    public double Peak { get; set; }
    public double OffPeak { get; set; }
    public double Cost { get; set; }
}