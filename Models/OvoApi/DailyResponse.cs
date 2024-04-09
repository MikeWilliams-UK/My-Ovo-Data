namespace OvoData.Models.OvoApi;

public class DailyResponse
{
    public DailyUtility Electricity { get; set; } = new DailyUtility();
    public DailyUtility Gas { get; set; } = new DailyUtility();
}