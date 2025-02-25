namespace OvoData.Models.Export;

public class HalfHourlyData
{
    public string StartTime { get; set; } = string.Empty;

    public double ElectricKwh { get; set; }

    public double GasKwh { get; set; }
}