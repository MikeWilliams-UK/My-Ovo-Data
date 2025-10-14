namespace OvoData.Models.OvoApi.Account;

public class AccountSupplyPoint
{
    public string StartDate { get; set; } = string.Empty;

    public SupplyPoint SupplyPoint { get; set; } = new();
}