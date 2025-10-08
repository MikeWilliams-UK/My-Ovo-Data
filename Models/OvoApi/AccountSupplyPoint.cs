using System.Text.Json.Serialization;

namespace OvoData.Models.OvoApi;

public class AccountSupplyPoint
{
    public string StartDate { get; set; }

    public SupplyPoint SupplyPoint { get; set; }

    [JsonPropertyName("__typename")]
    public string __typename { get; set; }
}