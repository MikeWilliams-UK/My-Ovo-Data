using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OvoData.Models.Api.Readings;

public class Account
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("accountSupplyPoints")]
    public List<AccountSupplyPoint> AccountSupplyPoints { get; set; } = new();
}