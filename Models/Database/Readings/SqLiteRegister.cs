namespace OvoData.Models.Database.Readings;

public class SqLiteRegister
{
    public string MeterSerialNumber { get; set; } = string.Empty;
    public string Id { get; set; } = string.Empty;

    public string StartDate { get; set; } = string.Empty;
    public string EndDate { get; set; } = string.Empty;

    public string TimingCategory { get; set; } = string.Empty;
    public string FuelType { get; set; } = string.Empty;
    public string UnitOfMeasurement { get; set; } = string.Empty;

    public override string ToString()
    {
        return string.IsNullOrEmpty(EndDate)
            ? $"{StartDate} - {TimingCategory} - {UnitOfMeasurement}"
            : $"{StartDate} - {EndDate} - {TimingCategory} - {UnitOfMeasurement}";
    }
}