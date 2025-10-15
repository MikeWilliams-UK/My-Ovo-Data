namespace OvoData.Models;

public class OvoReading
{
    public string Type { get; set; } = string.Empty;

    public string Source { get; set; } = string.Empty;

    public double Value { get; set; }
}