namespace OvoData.Models.Export;

public class DailyData
{
    public string Day { get; set; }

    public double ElectricKwh { get; set; }
    public double ElectricStanding { get; set; }
    public double ElectricAnyTime { get; set; }
    public double ElectricPeak { get; set; }
    public double ElectricOffPeak { get; set; }
    public double ElectricCost { get; set; }

    public double GasKwh { get; set; }
    public double GasStanding { get; set; }
    public double GasAnyTime { get; set; }
    public double GasCost { get; set; }
}