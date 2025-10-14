using System.Text.Json.Serialization;

namespace OvoData.Models.Api.Account;

public class SupplyPoint
{
    [JsonPropertyName("supplyPoint")]
    public SupplyPointDetail SupplyPointDetail { get; set; } = new();
}