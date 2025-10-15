using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OvoData.Models.Api.Readings;

public class Address
{
    [JsonPropertyName("addressLines")]
    public List<string> AddressLines { get; set; } = new();

    [JsonPropertyName("postCode")]
    public string PostCode { get; set; } = string.Empty;
}