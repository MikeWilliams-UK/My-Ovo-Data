namespace OvoData.Models.Database.Readings;

public class Reading
{
    public string Date { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string LifeCycle { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string MeterSerialNumber { get; set; } = string.Empty;

    public string RegisterId { get; set; } = string.Empty;
    public string TimingCategory { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;

    public override string ToString()
    {
        return $"{Date} - {MeterSerialNumber} - {Source} - {Value}";
    }
}