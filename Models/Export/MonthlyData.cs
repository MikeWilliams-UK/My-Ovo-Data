namespace OvoData.Models.Export;

public class MonthlyData
{
    public string Month { get; set; }

    public double ElectricKwh { get; set; }
    public double ElectricCost { get; set; }

    public double GasKwh { get; set; }
    public double GasCost { get; set; }
}