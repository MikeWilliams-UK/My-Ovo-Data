using System;

namespace OvoData.Models.Api.Login;

public class Tokens
{
    public string UserGuid { get; set; } = string.Empty;

    /// <summary>
    /// Access Token (60 seconds)
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    public DateTime AccessTokenExpiryTime { get; set; }
    public bool AccessTokenExpired { get; set; } = true;

    /// <summary>
    /// Refresh Token 1800 seconds (30 minutes)
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    public DateTime RefreshTokenExpiryTime { get; set; }
    public bool RefreshTokenExpired { get; set; } = true;
}