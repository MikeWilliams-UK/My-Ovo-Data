namespace OvoData.Models.OvoApi;

public class Tokens
{
    public string RefreshToken { get; set; } = string.Empty;

    public string AccessToken { get; set; } = string.Empty;

    public string UserGuid { get; set; } = string.Empty;
}