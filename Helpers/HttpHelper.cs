using Microsoft.Extensions.Configuration;
using OvoData.Models.OvoApi;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OvoData.Helpers;

public static class HttpHelper
{
    private static HttpClient _httpClient = new HttpClient();

    private static JsonSerializerOptions _options = new JsonSerializerOptions()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static bool Login(IConfigurationRoot config, LoginRequest loginRequest)
    {
        bool result;

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

        try
        {
            var uri = config["AccountsUri"];
            Logger.WriteLine($"Uri: {uri}");

            var request = new HttpRequestMessage(HttpMethod.Get, uri);

            var response = _httpClient.SendAsync(request).Result;
            if (response.IsSuccessStatusCode)
            {
                var content = response.Content.ReadAsStringAsync().Result;
                result = JsonSerializer.Deserialize<AccountsResponse>(content, _options);
            }
        }
        catch (Exception exception)
        {
            Logger.WriteLine(exception.ToString());
        }

        return result;
    }

    public static MonthlyResponse GetMonthlyUsage(IConfigurationRoot config, string accountId, int year)
    {
        var result = new MonthlyResponse();

        try
        {
            var uri = string.Format(config["MonthlyUri"], accountId, year);
            Logger.WriteLine($"Uri: {uri}");

            var request = new HttpRequestMessage(HttpMethod.Get, uri);

            var response = _httpClient.SendAsync(request).Result;
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

        return result;
    }

    public static DailyResponse GetDailyUsage(IConfigurationRoot config, string accountId, int year, int month)
    {
        var result = new DailyResponse();

        try
        {
            var uri = string.Format(config["DailyUri"], accountId, $"{year}-{month:D2}");
            Logger.WriteLine($"Uri: {uri}");

            var request = new HttpRequestMessage(HttpMethod.Get, uri);

            var response = _httpClient.SendAsync(request).Result;
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

    public static HalfHourlyResponse GetHalfHourlyUsage(IConfigurationRoot config, string accountId, int year, int month, int day)
    {
        var result = new HalfHourlyResponse();

        try
        {
            var uri = string.Format(config["HalfHourlyUri"], accountId, $"{year}-{month:D2}-{day:D2}");
            Logger.WriteLine($"Uri: {uri}");

            var request = new HttpRequestMessage(HttpMethod.Get, uri);

            var response = _httpClient.SendAsync(request).Result;
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