namespace OvoData.Models.OvoApi;

public class MonthlyResponse
{
    public MonthlyUtility Electricity { get; set; } = new MonthlyUtility();
    public MonthlyUtility Gas { get; set; } = new MonthlyUtility();
}