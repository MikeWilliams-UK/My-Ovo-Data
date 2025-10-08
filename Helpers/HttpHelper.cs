using Microsoft.Extensions.Configuration;
using OvoData.Models.OvoApi;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
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
        _configuration = configuration;
    }

    private static JsonSerializerOptions _options = new JsonSerializerOptions()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public bool Login(string username, string password, out Tokens tokens, out AccountsResponse data)
    {
        var result = false;
        data = new AccountsResponse();
        tokens = new Tokens();

        var request = new LoginRequest
        {
            Username = username, Password = password
        };

        if (DoLogin(request, out tokens))
        {
            tokens = DoGetAccessToken(tokens);
            data = DoGetAccounts(tokens);
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

        var response = _httpClient1.SendAsync(request).Result;
        if (response.IsSuccessStatusCode)
        {
            var responseContent = response.Content.ReadAsStringAsync().Result;
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

        var response = _httpClient1.SendAsync(request).Result;
        if (response.IsSuccessStatusCode)
        {
            var responseContent = response.Content.ReadAsStringAsync().Result;
            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseContent, _options);

            if (tokenResponse != null)
            {
                tokens.AccessToken = tokenResponse.AccessToken.Value;
            }
        }

        return tokens;
    }

    private AccountsResponse DoGetAccounts(Tokens tokens)
    {
        var result = new AccountsResponse();

        var uri = new Uri(_configuration["AccountsUri"]!);
        //var request = new HttpRequestMessage(HttpMethod.Post, uri);
        //request.Headers.Add("Authorization", tokens.AccessToken);

        var graphQl = ConstructGraphQl(tokens.UserGuid);

        var json = JsonSerializer.Serialize(graphQl);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        _httpClient2.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Authorization", tokens.AccessToken);

        _httpClient2.DefaultRequestHeaders.Add("Accept", "application/json");
        _httpClient2.DefaultRequestHeaders.Add("User-Agent", "YourAppName/1.0");

        var response = _httpClient2.PostAsync(uri, content).Result;
        if (response.IsSuccessStatusCode)
        {
            var responseContent = response.Content.ReadAsStringAsync().Result;
            var accountsResponse = JsonSerializer.Deserialize<AccountsResponse>(responseContent, _options);
            if (accountsResponse != null)
            {
                result = accountsResponse;
            }
        }

        return result;
    }

    private GraphQlRequest ConstructGraphQl(string customerGuid)
    {
        var request = new GraphQlRequest
        {
            OperationName = "Bootstrap",
            Variables = new Dictionary<string, object>
            {
                { "customerId", customerGuid }
            },
            Query = @"query Bootstrap($customerId: ID!) {
  customer_nextV1(id: $customerId) {
    id
    customerAccountRelationships {
      edges {
        node {
          account {
            accountNo
            id
            accountSupplyPoints {
              ...AccountSupplyPoint
              __typename
            }
            __typename
          }
          __typename
        }
        __typename
      }
      __typename
    }
    __typename
  }
}

fragment AccountSupplyPoint on AccountSupplyPoint {
  startDate
  supplyPoint {
    sprn
    fuelType
    meterTechnicalDetails {
      meterSerialNumber
      mode
      type
      status
      __typename
    }
    address {
      addressLines
      postCode
      __typename
    }
    __typename
  }
  __typename
}"
        };

        return request;
    }

    public MonthlyResponse GetMonthlyUsage(IConfigurationRoot config, string accountId, int year)
    {
        var result = new MonthlyResponse();

        try
        {
            var uri = string.Format(config["MonthlyUri"]!, accountId, year);
            Logger.WriteLine($"Uri: {uri}");

            var request = new HttpRequestMessage(HttpMethod.Get, uri);

            var response = _httpClient1.SendAsync(request).Result;
            if (response.IsSuccessStatusCode)
            {
                var content = response.Content.ReadAsStringAsync().Result;
                if (ConfigHelper.GetBoolean(config, "DumpData", false))
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

    public DailyResponse GetDailyUsage(IConfigurationRoot config, string accountId, int year, int month)
    {
        var result = new DailyResponse();

        try
        {
            var uri = string.Format(config["DailyUri"]!, accountId, $"{year}-{month:D2}");
            Logger.WriteLine($"Uri: {uri}");

            var request = new HttpRequestMessage(HttpMethod.Get, uri);

            var response = _httpClient1.SendAsync(request).Result;
            if (response.IsSuccessStatusCode)
            {
                var content = response.Content.ReadAsStringAsync().Result;
                if (ConfigHelper.GetBoolean(config, "DumpData", false))
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

    public HalfHourlyResponse GetHalfHourlyUsage(IConfigurationRoot config, string accountId, int year, int month, int day)
    {
        var result = new HalfHourlyResponse();

        try
        {
            var uri = string.Format(config["HalfHourlyUri"]!, accountId, $"{year}-{month:D2}-{day:D2}");
            Logger.WriteLine($"Uri: {uri}");

            var request = new HttpRequestMessage(HttpMethod.Get, uri);

            var response = _httpClient1.SendAsync(request).Result;
            if (response.IsSuccessStatusCode)
            {
                var content = response.Content.ReadAsStringAsync().Result;
                if (ConfigHelper.GetBoolean(config, "DumpData", false))
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