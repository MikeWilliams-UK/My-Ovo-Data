using System.Text.Json.Serialization;

namespace OvoData.Models.OvoApi;

public class CustomerNextV1
{
    public string Id { get; set; }

    public CustomerAccountRelationships customerAccountRelationships { get; set; }

    [JsonPropertyName("__typename")]
    public string Typename { get; set; }
}