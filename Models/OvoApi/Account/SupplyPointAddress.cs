using System.Collections.Generic;

namespace OvoData.Models.OvoApi.Account;

public class SupplyPointAddress
{
    public List<string> AddressLines { get; set; } = new();

    public string PostCode { get; set; } = string.Empty;
}