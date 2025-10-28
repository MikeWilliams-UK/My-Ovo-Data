using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OvoData.Models.Api.Account;

public class UsageAccountDetail
{
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("accountSupplyPoints")]
    public List<UsageSupplyPoint> SupplyPoints { get; set; } = new();
}