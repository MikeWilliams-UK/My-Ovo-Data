using System.Collections.Generic;

namespace OvoData.Models.Api.Account;

public class AccountDetail
{
    public string AccountNo { get; set; } = string.Empty;

    public string Id { get; set; } = string.Empty;

    public List<AccountSupplyPoint> AccountSupplyPoints { get; set; } = new();
}