namespace OvoData.Models.Database;

public class MonthlyReading
{
    public string Month { get; set; } = string.Empty;
    public double Consumption { get; set; }
    public double Cost { get; set; }
}