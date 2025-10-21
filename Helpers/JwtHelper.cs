namespace OvoData.Helpers;

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;

public static class JwtHelper
{
    public static string DumpJwt(string jwtToken)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwtToken);

            var payload = token.Payload;
            var convertedPayload = new Dictionary<string, object>();

            foreach (var kvp in payload)
            {
                if (kvp.Key == "exp" || kvp.Key == "iat")
                {
                    var dateTime = DateTimeOffset.FromUnixTimeSeconds((long)kvp.Value).ToLocalTime();
                    convertedPayload[kvp.Key] = $"{dateTime:yyyy-MM-dd HH:mm:ss}";
                    continue;
                }

                convertedPayload[kvp.Key] = kvp.Value;
            }

            var jsonPayload = JsonSerializer.Serialize(convertedPayload, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            return jsonPayload;
        }
        catch (Exception ex)
        {
            return $"Error parsing JWT: {ex.Message}";
        }
    }
}
