using System.Text.Json.Serialization;

namespace OvoData.Models.OvoApi;

public class CustomerNextV1
{
    public string Id { get; set; }

    public CustomerAccountRelationships CustomerAccountRelationships { get; set; }

    [JsonPropertyName("__typename")]
    public string Typename { get; set; }
}