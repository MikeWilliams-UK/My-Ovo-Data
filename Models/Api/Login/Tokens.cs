using System;

namespace OvoData.Models.Api.Login;

public class Tokens
{
    public string UserGuid { get; set; } = string.Empty;

    // Access token is short (60 seconds)
    public string AccessToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiryTime { get; set; }
    public bool AccessTokenExpired { get; set; } = true;

    // Refresh token is longer (1800 seconds == 30 minutes)
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime RefreshTokenExpiryTime { get; set; }
    public bool RefreshTokenExpired { get; set; } = true;
}