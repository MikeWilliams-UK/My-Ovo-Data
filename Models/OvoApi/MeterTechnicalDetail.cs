using System.Text.Json.Serialization;

namespace OvoData.Models.OvoApi;

public class MeterTechnicalDetail
{
    public string MeterSerialNumber { get; set; }

    public string Mode { get; set; }

    public string Type { get; set; }

    public string Status { get; set; }

    [JsonPropertyName("__typename")]
    public string Typename { get; set; }
}