using DocumentFormat.OpenXml.Bibliography;
using Microsoft.Extensions.Configuration;
using OvoData.Models.OvoApi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OvoData.Helpers;

public class HttpHelper
{
    private readonly HttpClient _httpClient1 = new();
    private readonly HttpClient _httpClient2 = new();
    private readonly HttpClient _httpClient3 = new();
    private readonly IConfigurationRoot _configuration;

    public HttpHelper(IConfigurationRoot configuration)
    {
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        _configuration = configuration;
    }

    private static JsonSerializerOptions _options = new JsonSerializerOptions()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public bool Login(string username, string password, out Tokens tokens, out List<OvoAccount> ovoAccounts)
    {
        var result = false;
        ovoAccounts = new List<OvoAccount>();
        tokens = new Tokens();

        var request = new LoginRequest
        {
            Username = username, Password = password
        };

        if (DoLogin(request, out tokens))
        {
            tokens = DoGetAccessToken(tokens);
            ovoAccounts = DoGetOvoAccounts(tokens);
            result = true;
        }

        return result;
    }

    private bool DoLogin(LoginRequest loginRequest, out Tokens tokens)
    {
        bool result;
        tokens = new Tokens();

        var uri = new Uri(_configuration["LoginUri"]!);
        var request = new HttpRequestMessage(HttpMethod.Post, uri);
        var requestContent = JsonSerializer.Serialize(loginRequest, _options);
        request.Content = new StringContent(requestContent, Encoding.ASCII, "application/json");

        Logger.WriteLine($"Logging in as {loginRequest.Username}");

        var response = _httpClient1.SendAsync(request).Result;
        if (response.IsSuccessStatusCode)
        {
            var responseContent = response.Content.ReadAsStringAsync().Result;
            if (ConfigHelper.GetBoolean(_configuration, "DumpData", false))
            {
                Logger.DumpJson("Login-Response", responseContent);
            }
            var loginResponse = JsonSerializer.Deserialize<LoginResponse>(responseContent, _options);
            if (loginResponse != null)
            {
                tokens.UserGuid = loginResponse.UserId;
                Debug.WriteLine($"UserName: {loginResponse.UserName}");
                Debug.WriteLine($"UserGuid: {loginResponse.UserId}");
            }

            var cookies = new List<Cookie>();
            if (response.Headers.TryGetValues("Set-Cookie", out var cookieHeaders))
            {
                foreach (var header in cookieHeaders)
                {
                    var cookieContainer = new CookieContainer();
                    cookieContainer.SetCookies(uri, header);

                    foreach (Cookie cookie in cookieContainer.GetCookies(uri))
                    {
                        cookies.Add(cookie);
                    }
                }
            }

            tokens.RefreshToken = cookies[0].Value;
            result = true;
        }
        else
        {
            result = false;
        }
        
        return result;
    }

    private Tokens DoGetAccessToken(Tokens tokens)
    {
        var uri = new Uri(_configuration["TokenUri"]!);
        var request = new HttpRequestMessage(HttpMethod.Get, uri);
        request.Headers.Add("restricted_refresh_token", tokens.RefreshToken);

        if (string.IsNullOrEmpty(tokens.AccessToken))
        {
            Logger.WriteLine("Obtaining access token");
        }
        else
        {
            Logger.WriteLine("Refreshing access token");
        }

        var response = _httpClient1.SendAsync(request).Result;
        if (response.IsSuccessStatusCode)
        {
            var responseContent = response.Content.ReadAsStringAsync().Result;
            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseContent, _options);

            if (tokenResponse != null)
            {
                tokens.RefreshTokenExpiryTime = DateTime.Now.AddSeconds(tokenResponse.RefreshExpiresIn);
                tokens.AccessToken = tokenResponse.AccessToken.Value;
                tokens.AccessTokenExpiryTime = DateTime.Now.AddSeconds(tokenResponse.ExpiresIn);
            }
        }

        return tokens;
    }

    private List<OvoAccount> DoGetOvoAccounts(Tokens tokens)
    {
        var result = new List<OvoAccount>();

        var request = new HttpRequestMessage(HttpMethod.Post, _configuration["AccountsUri"]!);
        request.Headers.Add("Authorization", $"Bearer {tokens.AccessToken}");

        var graphQl = "{\r\n \"operationName\": \"Bootstrap\",\r\n \"variables\": {\r\n \"customerId\": \"[[CustomerGuid]]\"\r\n },\r\n \"query\": \"query Bootstrap($customerId: ID!) {\\n customer_nextV1(id: $customerId) {\\n id\\n customerAccountRelationships {\\n edges {\\n node {\\n account {\\n accountNo\\n id\\n accountSupplyPoints {\\n ...AccountSupplyPoint\\n }\\n }\\n }\\n }\\n }\\n }\\n}\\n\\nfragment AccountSupplyPoint on AccountSupplyPoint {\\n startDate\\n supplyPoint {\\n fuelType\\n }\\n}\"\r\n}";
        graphQl = graphQl.Replace("[[CustomerGuid]]", tokens.UserGuid);
        var content = new StringContent(graphQl, null, "application/json");
        request.Content = content;

        Logger.WriteLine("Obtaining account details");

        var response = _httpClient2.SendAsync(request).Result;
        response.EnsureSuccessStatusCode();
        if (response.IsSuccessStatusCode)
        {
            var responseContent = response.Content.ReadAsStringAsync().Result;
            if (ConfigHelper.GetBoolean(_configuration, "DumpData", false))
            {
                Logger.DumpJson("Accounts-Response",  JsonHelper.Prettify(responseContent));
            }
            var accountsResponse = JsonSerializer.Deserialize<AccountsResponse>(responseContent, _options);
            if (accountsResponse != null)
            {
                var edges = accountsResponse.Data.CustomerNextV1.CustomerAccountRelationships.Edges.ToList();
                foreach (var edge in edges)
                {
                    var ovoAccount = new OvoAccount();
                    ovoAccount.Id = edge.Node.Account.AccountNo;
                    var electric = edge.Node.Account.AccountSupplyPoints.Any(s => s.SupplyPoint.FuelType.Equals("ELECTRICITY"));
                    ovoAccount.HasElectric = electric;

                    var gas = edge.Node.Account.AccountSupplyPoints.Any(s => s.SupplyPoint.FuelType.Equals("GAS"));
                    ovoAccount.HasElectric = gas;
                    result.Add(ovoAccount);
                }
            }
        }

        return result;
    }

    public MonthlyResponse GetMonthlyUsage(Tokens tokens, string accountId, int year)
    {
        var result = new MonthlyResponse();

        try
        {
            if (DateTime.Now > tokens.AccessTokenExpiryTime)
            {
                tokens = DoGetAccessToken(tokens);
            }

            var uri = string.Format(_configuration["MonthlyUri"]!, accountId, year);
            Logger.WriteLine($"Uri: {uri}");

            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Add("Authorization", $"Bearer {tokens.AccessToken}");

            var response = _httpClient3.SendAsync(request).Result;
            if (response.IsSuccessStatusCode)
            {
                var content = response.Content.ReadAsStringAsync().Result;
                if (ConfigHelper.GetBoolean(_configuration, "DumpData", false))
                {
                    Logger.DumpJson($"{nameof(GetMonthlyUsage)}-{year}", content);
                }
                result = JsonSerializer.Deserialize<MonthlyResponse>(content, _options);
            }
        }
        catch (Exception exception)
        {
            Logger.WriteLine(exception.ToString());
        }

        return result!;
    }

    public DailyResponse GetDailyUsage(Tokens tokens, string accountId, int year, int month)
    {
        var result = new DailyResponse();

        try
        {
            if (DateTime.Now > tokens.AccessTokenExpiryTime)
            {
                tokens = DoGetAccessToken(tokens);
            }

            var uri = string.Format(_configuration["DailyUri"]!, accountId, $"{year}-{month:D2}");
            Logger.WriteLine($"Uri: {uri}");

            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Add("Authorization", $"Bearer {tokens.AccessToken}");

            var response = _httpClient3.SendAsync(request).Result;
            if (response.IsSuccessStatusCode)
            {
                var content = response.Content.ReadAsStringAsync().Result;
                if (ConfigHelper.GetBoolean(_configuration, "DumpData", false))
                {
                    Logger.DumpJson($"{nameof(GetDailyUsage)}-{year}-{month:D2}", content);
                }
                result = JsonSerializer.Deserialize<DailyResponse>(content, _options);
            }
        }
        catch (Exception exception)
        {
            Logger.WriteLine(exception.ToString());
        }

        return result;
    }

    public HalfHourlyResponse GetHalfHourlyUsage(Tokens tokens, string accountId, int year, int month, int day)
    {
        var result = new HalfHourlyResponse();

        try
        {
            if (DateTime.Now > tokens.AccessTokenExpiryTime)
            {
                tokens = DoGetAccessToken(tokens);
            }

            var uri = string.Format(_configuration["HalfHourlyUri"]!, accountId, $"{year}-{month:D2}-{day:D2}");
            Logger.WriteLine($"Uri: {uri}");

            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Add("Authorization", $"Bearer {tokens.AccessToken}");

            var response = _httpClient3.SendAsync(request).Result;
            if (response.IsSuccessStatusCode)
            {
                var content = response.Content.ReadAsStringAsync().Result;
                if (ConfigHelper.GetBoolean(_configuration, "DumpData", false))
                {
                    Logger.DumpJson($"{nameof(GetHalfHourlyUsage)}-{year}-{month:D2}-{day:D2}", content);
                }
                result = JsonSerializer.Deserialize<HalfHourlyResponse>(content, _options);
            }
        }
        catch (Exception exception)
        {
            Logger.WriteLine(exception.ToString());
        }

        return result;
    }
}