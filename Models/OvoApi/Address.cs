using System.Collections.Generic;

namespace OvoData.Models.OvoApi;

public class Address
{
    public List<string> AddressLines { get; set; }

    public string PostCode { get; set; }
}