using System.Collections.Generic;

namespace OvoData.Models.Api.Login;

public class LoginResponse
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Identifiers { get; set; } = string.Empty;
    public string AuthenticationPlatform { get; set; } = string.Empty;

    public List<string> Roles { get; set; } = new();
}