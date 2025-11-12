using System.Text.Json.Serialization;

namespace OvoData.Models.Api.Account;

public class UsageSupplyPoint
{
    public string StartDate { get; set; } = string.Empty;

    [JsonPropertyName("supplyPoint")]
    public UsageSupplyPointDetail SupplyPointDetail { get; set; } = new();
}