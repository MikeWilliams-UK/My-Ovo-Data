using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OvoData.Models.OvoApi;

public class Address
{
    public List<string> AddressLines { get; set; }

    public string PostCode { get; set; }

    [JsonPropertyName("__typename")]
    public string Typename { get; set; }
}