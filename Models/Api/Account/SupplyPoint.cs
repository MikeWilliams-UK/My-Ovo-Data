using System.Text.Json.Serialization;

namespace OvoData.Models.Api.Account;

public class SupplyPoint
{
    public string StartDate { get; set; } = string.Empty;

    [JsonPropertyName("supplyPoint")]
    public SupplyPointDetail SupplyPointDetail { get; set; } = new();
}