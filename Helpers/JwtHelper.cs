using System;

namespace OvoData.Helpers;

using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;

public static class JwtHelper
{
    /// <summary>
    /// Parses and dumps the contents of a JWT token.
    /// </summary>
    /// <param name="jwtToken">The JWT token string.</param>
    /// <returns>A formatted JSON string of the token's payload.</returns>
    public static string DumpJwt(string jwtToken)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(jwtToken);

            // Extract the payload (claims)
            var payload = token.Payload;

            // Serialize the payload to a formatted JSON string
            var jsonPayload = JsonSerializer.Serialize(payload, new JsonSerializerOptions
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
