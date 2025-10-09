using System.Collections.Generic;

namespace OvoData.Models.OvoApi;

public class SupplyPointAddress
{
    public List<string> AddressLines { get; set; }

    public string PostCode { get; set; }
}