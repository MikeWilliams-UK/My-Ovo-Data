using System;

namespace OvoData.Models.OvoApi.Login;

public class Tokens
{
    public string UserGuid { get; set; } = string.Empty;

    public string RefreshToken { get; set; } = string.Empty;
    public DateTime RefreshTokenExpiryTime { get; set; }

    public string AccessToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiryTime { get; set; }
}