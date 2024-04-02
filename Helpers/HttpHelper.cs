using Microsoft.Extensions.Configuration;
using OvoData.Models.OvoApi;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace OvoData.Helpers;

public static class HttpHelper
{
    private static HttpClient _httpClient = new HttpClient();

    private static JsonSerializerOptions _options = new JsonSerializerOptions()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true
    };

    public static bool Login(IConfigurationRoot config, LoginRequest loginRequest)
    {
        var result = false;

        var request = new HttpRequestMessage(HttpMethod.Post, config["LoginUri"]);

        var content = JsonSerializer.Serialize(loginRequest, _options);
        request.Content = new StringContent(content, Encoding.ASCII, "application/json");
        var response = _httpClient.SendAsync(request).Result;

        if (response.IsSuccessStatusCode)
        {
            result = true;
        }
        else
        {
            result = false;
        }

        return result;
    }

    public static AccountsResponse GetAccountIds(IConfigurationRoot config)
    {
        var result = new AccountsResponse();

        var request = new HttpRequestMessage(HttpMethod.Get, config["AccountsUri"]);

        var response = _httpClient.SendAsync(request).Result;
        if (response.IsSuccessStatusCode)
        {
            var content = response.Content.ReadAsStringAsync().Result;
            result = JsonSerializer.Deserialize<AccountsResponse>(content, _options);
        }

        return result;
    }

    public static MonthlyResponse GetMonthlyUsage(IConfigurationRoot config, string accountId, int year)
    {
        var result = new MonthlyResponse();

        var request = new HttpRequestMessage(HttpMethod.Get, string.Format(config["MonthlyUri"], accountId, year));

        var response = _httpClient.SendAsync(request).Result;
        if (response.IsSuccessStatusCode)
        {
            var content = response.Content.ReadAsStringAsync().Result;
            result = JsonSerializer.Deserialize<MonthlyResponse>(content, _options);
        }

        return result;
    }

    public static DailyResponse GetDailyUsage(IConfigurationRoot config, string accountId, int year, int month)
    {
        var result = new DailyResponse();

        var request = new HttpRequestMessage(HttpMethod.Get, string.Format(config["DailyUri"], accountId, $"{year}-{month:D2}"));

        var response = _httpClient.SendAsync(request).Result;
        if (response.IsSuccessStatusCode)
        {
            var content = response.Content.ReadAsStringAsync().Result;
            result = JsonSerializer.Deserialize<DailyResponse>(content, _options);
        }

        return result;
    }

    public static HalfHourlyResponse GetHalfHourlyUsage(IConfigurationRoot config, string accountId, int year, int month, int day)
    {
        var result = new HalfHourlyResponse();

        var request = new HttpRequestMessage(HttpMethod.Get, string.Format(config["HalfHourlyUri"], accountId, $"{year}-{month:D2}-{day:D2}"));

        var response = _httpClient.SendAsync(request).Result;
        if (response.IsSuccessStatusCode)
        {
            var content = response.Content.ReadAsStringAsync().Result;
            result = JsonSerializer.Deserialize<HalfHourlyResponse>(content, _options);
        }

        return result;
    }
}