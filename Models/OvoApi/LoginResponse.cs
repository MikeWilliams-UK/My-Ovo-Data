using System.Collections.Generic;

namespace OvoData.Models.OvoApi;

public class LoginResponse
{
    public string UserId { get; set; }
    public string UserName { get; set; }
    public string Identifiers { get; set; }
    public string AuthenticationPlatform { get; set; }

    public List<string> Roles { get; set; }
}