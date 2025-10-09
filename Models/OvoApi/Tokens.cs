using System;

namespace OvoData.Models.OvoApi;

public class Tokens
{
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime RefreshTokenExpiryTime { get; set; }

    public string AccessToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiryTime { get; set; }

    public string UserGuid { get; set; } = string.Empty;
}