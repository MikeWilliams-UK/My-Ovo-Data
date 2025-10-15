using Microsoft.Extensions.Configuration;
using OvoData.Models;
using OvoData.Models.Api;
using OvoData.Models.Api.Account;
using OvoData.Models.Api.Login;
using OvoData.Models.Api.Readings;
using OvoData.Models.Api.Usage;
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
    private LoginRequest? _loginRequest = null;

    public HttpHelper(IConfigurationRoot configuration)
    {
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        _configuration = configuration;
    }

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public bool FirstLogin(string username, string password, out Tokens tokens, out List<OvoAccount> ovoAccounts)
    {
        var result = false;
        ovoAccounts = [];
        tokens = new Tokens();

        try
        {
            _loginRequest = new LoginRequest
            {
                Username = username,
                Password = password
            };

            if (ExecuteLoginRequest(_loginRequest, out tokens))
            {
                tokens = ObtainAccessTokens(tokens);
                ovoAccounts = ObtainAccountDetails(tokens);
                result = true;
            }
        }
        catch (Exception exception)
        {
            Logger.WriteLine(exception.ToString());
        }

        return result;
    }

    private bool ExecuteLoginRequest(LoginRequest loginRequest, out Tokens tokens)
    {
        bool result;
        tokens = new Tokens();

        var uri = new Uri(_configuration["LoginUri"]!);
        var request = new HttpRequestMessage(HttpMethod.Post, uri);
        var requestContent = JsonSerializer.Serialize(loginRequest, JsonSerializerOptions);
        request.Content = new StringContent(requestContent, Encoding.ASCII, "application/json");

        Logger.WriteLine($"Logging in as {loginRequest.Username}");

        var response = _httpClient1.SendAsync(request).Result;
        if (response.IsSuccessStatusCode)
        {
            var responseContent = response.Content.ReadAsStringAsync().Result;
            if (ConfigHelper.GetBoolean(_configuration, "DumpData", false))
            {
                Logger.DumpJson("FirstLogin-Response", responseContent);
            }
            var loginResponse = JsonSerializer.Deserialize<LoginResponse>(responseContent, JsonSerializerOptions);
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

    private Tokens ObtainBothTokens()
    {
        Tokens tokens = new Tokens();

        if (_loginRequest != null)
        {
            ExecuteLoginRequest(_loginRequest, out tokens);
        }

        return tokens;
    }

    private Tokens ObtainAccessTokens(Tokens tokens)
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
            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseContent, JsonSerializerOptions);

            if (tokenResponse != null)
            {
                tokens.RefreshTokenExpiryTime = DateTime.Now.AddSeconds(tokenResponse.RefreshExpiresIn);
                tokens.AccessToken = tokenResponse.AccessToken.Value;
                tokens.AccessTokenExpiryTime = DateTime.Now.AddSeconds(tokenResponse.ExpiresIn);

                Debug.WriteLine($"Access  Token Expires at {tokens.AccessTokenExpiryTime:dd-MMM-yyyy HH:mm:ss}");
                Debug.WriteLine($"Refresh Token Expires at {tokens.RefreshTokenExpiryTime:dd-MMM-yyyy HH:mm:ss}");
            }
        }

        return tokens;
    }

    private List<OvoAccount> ObtainAccountDetails(Tokens tokens)
    {
        var result = new List<OvoAccount>();

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, _configuration["AccountsUri"]!);
            request.Headers.Add("Authorization", $"Bearer {tokens.AccessToken}");

            var graphQl = "{\r\n \"operationName\": \"Bootstrap\",\r\n \"variables\": {\r\n \"customerId\": \"[[CustomerGuid]]\"\r\n },\r\n \"query\": \"query Bootstrap($customerId: ID!) {\\n customer_nextV1(id: $customerId) {\\n id\\n customerAccountRelationships {\\n edges {\\n node {\\n account {\\n id\\n accountSupplyPoints {\\n ...AccountSupplyPoint\\n }\\n }\\n }\\n }\\n }\\n }\\n}\\n\\nfragment AccountSupplyPoint on AccountSupplyPoint {\\n startDate\\n supplyPoint {\\n fuelType\\n }\\n}\"\r\n}";
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
                    Logger.DumpJson("Accounts-Response", JsonHelper.Prettify(responseContent));
                }
                var accountsResponse = JsonSerializer.Deserialize<AccountsResponse>(responseContent, JsonSerializerOptions);
                if (accountsResponse != null)
                {
                    var accounts = accountsResponse.AccountsData.Customer.Relationships.Accounts.ToList();
                    foreach (var account in accounts)
                    {
                        var ovoAccount = new OvoAccount();
                        ovoAccount.Id = account.Details.AccountDetail.Id;
                        var electric = account.Details.AccountDetail.SupplyPoints.Where(s => s.SupplyPointDetail.FuelType.Equals("ELECTRICITY")).ToList();
                        if (electric.Any())
                        {
                            ovoAccount.HasElectric = true;
                            ovoAccount.ElectricStartDate = electric[0].StartDate;
                        }

                        var gas = account.Details.AccountDetail.SupplyPoints.Where(s => s.SupplyPointDetail.FuelType.Equals("GAS")).ToList();
                        if (gas.Any())
                        {
                            ovoAccount.HasGas = true;
                            ovoAccount.GasStartDate = gas[0].StartDate;
                        }
                        result.Add(ovoAccount);
                    }
                }
            }
        }
        catch (Exception exception)
        {
            Logger.WriteLine(exception.ToString());
        }

        return result;
    }

    public MonthlyResponse ObtainMonthlyUsage(Tokens tokens, string accountId, int year)
    {
        var result = new MonthlyResponse();

        try
        {
            if (DateTime.Now > tokens.RefreshTokenExpiryTime)
            {
                tokens = ObtainBothTokens();
            }
            else if (DateTime.Now > tokens.AccessTokenExpiryTime)
            {
                tokens = ObtainAccessTokens(tokens);
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
                    Logger.DumpJson($"{nameof(ObtainMonthlyUsage)}-{year}", content);
                }
                result = JsonSerializer.Deserialize<MonthlyResponse>(content, JsonSerializerOptions);
            }
        }
        catch (Exception exception)
        {
            Logger.WriteLine(exception.ToString());
        }

        return result!;
    }

    public DailyResponse ObtainDailyUsage(Tokens tokens, string accountId, int year, int month)
    {
        var result = new DailyResponse();

        try
        {
            if (DateTime.Now > tokens.RefreshTokenExpiryTime)
            {
                tokens = ObtainBothTokens();
            }
            else if (DateTime.Now > tokens.AccessTokenExpiryTime)
            {
                tokens = ObtainAccessTokens(tokens);
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
                    Logger.DumpJson($"{nameof(ObtainDailyUsage)}-{year}-{month:D2}", content);
                }
                result = JsonSerializer.Deserialize<DailyResponse>(content, JsonSerializerOptions);
            }
        }
        catch (Exception exception)
        {
            Logger.WriteLine(exception.ToString());
        }

        return result;
    }

    public HalfHourlyResponse ObtainHalfHourlyUsage(Tokens tokens, string accountId, int year, int month, int day)
    {
        var result = new HalfHourlyResponse();

        try
        {
            if (DateTime.Now > tokens.RefreshTokenExpiryTime)
            {
                tokens = ObtainBothTokens();
            }
            else if (DateTime.Now > tokens.AccessTokenExpiryTime)
            {
                tokens = ObtainAccessTokens(tokens);
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
                    Logger.DumpJson($"{nameof(ObtainHalfHourlyUsage)}-{year}-{month:D2}-{day:D2}", content);
                }
                result = JsonSerializer.Deserialize<HalfHourlyResponse>(content, JsonSerializerOptions);
            }
        }
        catch (Exception exception)
        {
            Logger.WriteLine(exception.ToString());
        }

        return result;
    }

    public List<OvoMeterReading> ObtainMeterReadings(Tokens tokens, string accountId)
    {
        List<OvoMeterReading> result = [];

        try
        {
            if (DateTime.Now > tokens.RefreshTokenExpiryTime)
            {
                tokens = ObtainBothTokens();
            }
            else if (DateTime.Now > tokens.AccessTokenExpiryTime)
            {
                tokens = ObtainAccessTokens(tokens);
            }

            var request = new HttpRequestMessage(HttpMethod.Post, _configuration["ReadingsUri"]!);
            request.Headers.Add("Authorization", $"Bearer {tokens.AccessToken}");

            //var query = string.Join(@"\n", ResourceHelper.GetStringResource("GraphQL.Readings.Query.txt").Split(Environment.NewLine));
            //var graphQl = ResourceHelper.GetStringResource("GraphQL.Readings.json")
            //    .Replace("[[accountId]]", accountId)
            //    .Replace("[[query]]", query);

            var graphQl = "{\r\n    \"operationName\": \"MeterReads_nextV1\",\r\n    \"variables\": {\r\n        \"accountId\": \"[[accountId]]\",\r\n        \"query\": {\r\n            \"includeReads\": \"TOP_VALID_ONLY\"\r\n        }\r\n    },\r\n    \"query\": \"query MeterReads_nextV1($accountId: ID!, $query: MeterReadsInputV2!) {account(id: $accountId) {\\n    id\\n    accountSupplyPoints {\\n      ...AccountSupplyPointReads\\n      \\n    }\\n    \\n  }\\n}\\n\\nfragment AccountSupplyPointReads on AccountSupplyPoint {\\n  startDate\\n  end {\\n    date\\n    \\n  }\\n  supplyPoint {\\n    timezone\\n    sprn\\n    meterTechnicalDetails {\\n      ...MeterTechnicalDetails\\n      \\n    }\\n    address {\\n      addressLines\\n      postCode\\n      \\n    }\\n    region\\n    fuelType\\n    id\\n    \\n  }\\n  meterReads_nextV1(query: $query, last: 10000) {\\n    edges {\\n      node {\\n        reading {\\n          ...MeterRead\\n          \\n        }\\n        \\n      }\\n      \\n    }\\n    \\n  }\\n  \\n}\\n\\nfragment MeterTechnicalDetails on SupplyPointMeterTechnicalDetails {\\n  registers {\\n    registerId\\n    timingCategory\\n    numberOfDigits\\n    unitMeasurement\\n    registerStartDate\\n    registerEndDate\\n    \\n  }\\n  type\\n  meterSerialNumber\\n  status\\n  \\n}\\n\\nfragment MeterRead on MeterRead_nextV1 {\\n  type\\n  date\\n  lifecycle\\n  source\\n  meterSerialNumber\\n  ... on ElectricityMeterRead_nextV2 {\\n    registers {\\n      registerId\\n      timingCategory\\n      value\\n      \\n    }\\n    \\n  }\\n  ... on GasMeterRead_nextV2 {\\n    value\\n    \\n  }\\n  \\n}\"\r\n}";
            graphQl = graphQl.Replace("[[accountId]]", accountId);

            var content = new StringContent(graphQl, null, "application/json");
            request.Content = content;

            Logger.WriteLine("Obtaining meter readings");

            var response = _httpClient2.SendAsync(request).Result;
            response.EnsureSuccessStatusCode();
            if (response.IsSuccessStatusCode)
            {
                var responseContent = response.Content.ReadAsStringAsync().Result;
                if (ConfigHelper.GetBoolean(_configuration, "DumpData", false))
                {
                    Logger.DumpJson("MeterReadings-Response", JsonHelper.Prettify(responseContent));
                }
                var readingsResponse = JsonSerializer.Deserialize<ReadingsResponse>(responseContent, JsonSerializerOptions);
                if (readingsResponse != null)
                {
                    Debug.WriteLine(readingsResponse.Data.Account.Id);
                }
            }
        }
        catch (Exception exception)
        {
            Logger.WriteLine(exception.ToString());
        }

        return result;
    }
}