namespace OvoData.Models.Api.Login;

public class Tokens
{
    public string UserGuid { get; set; } = string.Empty;

    /// <summary>
    /// Access Token (60 seconds)
    /// </summary>
    public Token AccessToken { get; set; } = new();

    /// <summary>
    /// Refresh Token (1800 seconds [30 minutes])
    /// </summary>
    public Token RefreshToken { get; set; } = new();
}