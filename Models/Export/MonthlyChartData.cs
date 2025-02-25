namespace OvoData.Models.Export;

public class MonthlyChartData
{
    public string Year { get; set; } = string.Empty;

    public double JanKwh { get; set; }
    public double JanCost { get; set; }

    public double FebKwh { get; set; }
    public double FebCost { get; set; }

    public double MarKwh { get; set; }
    public double MarCost { get; set; }

    public double AprKwh { get; set; }
    public double AprCost { get; set; }

    public double MayKwh { get; set; }
    public double MayCost { get; set; }

    public double JunKwh { get; set; }
    public double JunCost { get; set; }

    public double JulKwh { get; set; }
    public double JulCost { get; set; }

    public double AugKwh { get; set; }
    public double AugCost { get; set; }

    public double SepKwh { get; set; }
    public double SepCost { get; set; }

    public double OctKwh { get; set; }
    public double OctCost { get; set; }

    public double NovKwh { get; set; }
    public double NovCost { get; set; }

    public double DecKwh { get; set; }
    public double DecCost { get; set; }
}