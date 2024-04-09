namespace OvoData.Models.OvoApi;

public class HalfHourlyResponse
{
    public HalfHourlyUtility Electricity { get; set; } = new HalfHourlyUtility();
    public HalfHourlyUtility Gas { get; set; } = new HalfHourlyUtility();
}