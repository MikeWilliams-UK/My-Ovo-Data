using System.Collections.Generic;

namespace OvoData.Models.Api.Account;

public class SupplyPointAddress
{
    public List<string> AddressLines { get; set; } = new();

    public string PostCode { get; set; } = string.Empty;
}