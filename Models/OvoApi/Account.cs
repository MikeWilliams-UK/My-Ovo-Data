using System.Collections.Generic;

namespace OvoData.Models.OvoApi;

public class Account
{
    public string AccountNo { get; set; }

    public string Id { get; set; }

    public List<AccountSupplyPoint> AccountSupplyPoints { get; set; }
}