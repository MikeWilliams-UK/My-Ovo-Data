using OvoData.Helpers;
using System;

namespace OvoData.Models.Api.Login;

public class Token
{
    private string _jwtValue = string.Empty;
    private string _jwtAsString = string.Empty;

    public string Jwt
    {
        get => _jwtValue;
        set
        {
            _jwtValue = value;
            if (!string.IsNullOrEmpty(value))
            {
                _jwtAsString = JwtHelper.Decode(value, out var lifetimes);

                if (lifetimes.TryGetValue("iat", out var dateTime))
                {
                    IssuedAtTime = dateTime;
                }

                if (lifetimes.TryGetValue("exp", out dateTime))
                {
                    ExpiresAtTime = dateTime;
                }
            }
        }
    }

    public DateTime IssuedAtTime { get; private set; }
    public DateTime ExpiresAtTime { get; private set; }

    public bool HasExpired
        => DateTime.UtcNow.AddSeconds(5) > ExpiresAtTime;

    public override string ToString()
    {
        return _jwtAsString;
    }
}