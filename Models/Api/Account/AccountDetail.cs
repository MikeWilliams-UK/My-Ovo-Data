using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OvoData.Models.Api.Account;

public class AccountDetail
{
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("accountSupplyPoints")]
    public List<SupplyPoint> SupplyPoints { get; set; } = new();
}