namespace OvoData.Models.Database.Usage;

public class SqLiteMonthly
{
    public string Month { get; set; } = string.Empty;
    public double Consumption { get; set; }
    public double Cost { get; set; }
}