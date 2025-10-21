using Microsoft.Extensions.Configuration;
using OvoData.Models.Api.Account;
using OvoData.Models.Api.Login;
using OvoData.Models.Api.Readings;
using OvoData.Models.Api.Usage;
using OvoData.Models.Database.Readings;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
    private Logger _logger;

    public HttpHelper(IConfigurationRoot configuration, Logger logger)
    {
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        _configuration = configuration;
        _logger = logger;
    }

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public bool FirstLogin(string username, string password, out Tokens tokens, out List<Models.Database.Account> ovoAccounts)
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
            _logger.WriteLine(exception.ToString());
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

        _logger.WriteLine($"Calling API endpoint at Uri: {uri} to Log in as '{loginRequest.Username}'");

        var response = _httpClient1.SendAsync(request).Result;
        if (response.IsSuccessStatusCode)
        {
            var responseContent = response.Content.ReadAsStringAsync().Result;
            if (ConfigHelper.GetBoolean(_configuration, "DumpData", false))
            {
                _logger.DumpJson("FirstLogin-Response", responseContent);
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

    private Tokens CheckTokens(Tokens tokens)
    {
        var now = DateTime.Now;
        Debug.WriteLine($"CheckTokens() Now: {now} Refresh Token: {tokens.RefreshTokenExpiryTime} Access Token: {tokens.AccessTokenExpiryTime}");

        if (now >= tokens.AccessTokenExpiryTime)
        {
            tokens = ObtainAccessTokens(tokens);
        }
        else if (now >= tokens.RefreshTokenExpiryTime)
        {
            tokens = ObtainBothTokens();
        }

        return tokens;
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
            _logger.WriteLine($"Calling API endpoint at Uri: {uri} to obtain access token");
        }
        else
        {
            _logger.WriteLine($"Calling API endpoint at Uri: {uri} to refresh access token"); 
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

    private List<Models.Database.Account> ObtainAccountDetails(Tokens tokens)
    {
        var result = new List<Models.Database.Account>();

        try
        {
            var uri = new Uri(_configuration["AccountsUri"]!);
            var request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Headers.Add("Authorization", $"Bearer {tokens.AccessToken}");

            var query = string.Join(@"\n", ResourceHelper.GetStringResource("GraphQL.Accounts.query").Split(Environment.NewLine));
            var graphQl = ResourceHelper.GetStringResource("GraphQL.Accounts.json");
            graphQl = graphQl.Replace("[[customerGuid]]", tokens.UserGuid).Replace("[[query]]", query);

            var content = new StringContent(graphQl, null, "application/json");
            request.Content = content;

            _logger.WriteLine($"Calling API endpoint at Uri: {uri} to obtain account details");

            var response = _httpClient2.SendAsync(request).Result;
            response.EnsureSuccessStatusCode();
            if (response.IsSuccessStatusCode)
            {
                var responseContent = response.Content.ReadAsStringAsync().Result;
                if (ConfigHelper.GetBoolean(_configuration, "DumpData", false))
                {
                    _logger.DumpJson("Accounts-Response", JsonHelper.Prettify(responseContent));
                }
                var accountsResponse = JsonSerializer.Deserialize<AccountsResponse>(responseContent, JsonSerializerOptions);
                if (accountsResponse != null)
                {
                    var accounts = accountsResponse.Data.Customer.Relationships.Accounts.ToList();
                    foreach (var account in accounts)
                    {
                        var ovoAccount = new Models.Database.Account
                        {
                            Id = account.Details.AccountDetail.Id
                        };

                        var electric = account.Details.AccountDetail.SupplyPoints
                            .Where(s => s.SupplyPointDetail.FuelType.Equals(Constants.FuelTypeElectricity))
                            .ToList();
                        if (electric.Any())
                        {
                            ovoAccount.HasElectric = true;
                            ovoAccount.ElectricStartDate = electric[0].StartDate;
                        }

                        var gas = account.Details.AccountDetail.SupplyPoints
                            .Where(s => s.SupplyPointDetail.FuelType.Equals(Constants.FuelTypeGas))
                            .ToList();
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
            _logger.WriteLine(exception.ToString());
        }

        return result;
    }

    public MonthlyResponse ObtainMonthlyUsage(Tokens tokens, string accountId, int year)
    {
        var result = new MonthlyResponse();

        try
        {
            tokens = CheckTokens(tokens);

            var uri = string.Format(_configuration["MonthlyUri"]!, accountId, year);
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Add("Authorization", $"Bearer {tokens.AccessToken}");

            _logger.WriteLine($"Calling API endpoint at Uri: {uri}");

            var response = _httpClient3.SendAsync(request).Result;
            if (response.IsSuccessStatusCode)
            {
                var content = response.Content.ReadAsStringAsync().Result;
                if (ConfigHelper.GetBoolean(_configuration, "DumpData", false))
                {
                    _logger.DumpJson($"{nameof(ObtainMonthlyUsage)}-{year}", content);
                }
                result = JsonSerializer.Deserialize<MonthlyResponse>(content, JsonSerializerOptions);
            }
        }
        catch (Exception exception)
        {
            _logger.WriteLine(exception.ToString());
        }

        return result!;
    }

    public DailyResponse ObtainDailyUsage(Tokens tokens, string accountId, int year, int month)
    {
        var result = new DailyResponse();

        try
        {
            tokens = CheckTokens(tokens);

            var uri = string.Format(_configuration["DailyUri"]!, accountId, $"{year}-{month:D2}");
            _logger.WriteLine($"Calling API endpoint at Uri: {uri}");

            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Add("Authorization", $"Bearer {tokens.AccessToken}");

            var response = _httpClient3.SendAsync(request).Result;
            if (response.IsSuccessStatusCode)
            {
                var content = response.Content.ReadAsStringAsync().Result;
                if (ConfigHelper.GetBoolean(_configuration, "DumpData", false))
                {
                    _logger.DumpJson($"{nameof(ObtainDailyUsage)}-{year}-{month:D2}", content);
                }
                result = JsonSerializer.Deserialize<DailyResponse>(content, JsonSerializerOptions);
            }
        }
        catch (Exception exception)
        {
            _logger.WriteLine(exception.ToString());
        }

        return result;
    }

    public HalfHourlyResponse ObtainHalfHourlyUsage(Tokens tokens, string accountId, int year, int month, int day)
    {
        var result = new HalfHourlyResponse();

        try
        {
            tokens = CheckTokens(tokens);

            var uri = string.Format(_configuration["HalfHourlyUri"]!, accountId, $"{year}-{month:D2}-{day:D2}");
            _logger.WriteLine($"Calling API endpoint at Uri: {uri}");

            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Add("Authorization", $"Bearer {tokens.AccessToken}");

            var response = _httpClient3.SendAsync(request).Result;
            if (response.IsSuccessStatusCode)
            {
                var content = response.Content.ReadAsStringAsync().Result;
                if (ConfigHelper.GetBoolean(_configuration, "DumpData", false))
                {
                    _logger.DumpJson($"{nameof(ObtainHalfHourlyUsage)}-{year}-{month:D2}-{day:D2}", content);
                }
                result = JsonSerializer.Deserialize<HalfHourlyResponse>(content, JsonSerializerOptions);
            }
        }
        catch (Exception exception)
        {
            _logger.WriteLine(exception.ToString());
        }

        return result;
    }

    public List<Models.Database.SupplyPoint> ObtainMeterReadings(Tokens tokens, string accountId)
    {
        List<Models.Database.SupplyPoint> result = [];

        try
        {
            tokens = CheckTokens(tokens);

            var query = string.Join(@"\n", ResourceHelper.GetStringResource("GraphQL.Readings.query").Split(Environment.NewLine));
            var graphQl = ResourceHelper.GetStringResource("GraphQL.Readings.json");
            graphQl = graphQl.Replace("[[accountId]]", accountId).Replace("[[query]]", query);

            var uri = new Uri(_configuration["ReadingsUri"]!);
            var request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Headers.Add("Authorization", $"Bearer {tokens.AccessToken}");

            var content = new StringContent(graphQl, null, "application/json");
            request.Content = content;

            _logger.WriteLine($"Calling API endpoint at Uri: {uri} to obtain meter readings");

            var response = _httpClient2.SendAsync(request).Result;
            response.EnsureSuccessStatusCode();
            if (response.IsSuccessStatusCode)
            {
                var responseContent = response.Content.ReadAsStringAsync().Result;

                if (ConfigHelper.GetBoolean(_configuration, "DumpData", false))
                {
                    _logger.DumpJson("MeterReadings-Response", JsonHelper.Prettify(responseContent));
                }
                var readingsResponse = JsonSerializer.Deserialize<ReadingsResponse>(responseContent, JsonSerializerOptions);
                if (readingsResponse != null)
                {
                    Debug.WriteLine(readingsResponse.Data.Account.Id);

                    var electric = readingsResponse.Data.Account.AccountSupplyPoints
                        .Where(s => s.SupplyPoint.FuelType.Equals(Constants.FuelTypeElectricity))
                        .ToList();
                    if (electric.Any())
                    {
                        var ovoSupplyPoint = new Models.Database.SupplyPoint
                        {
                            Sprn = electric[0].SupplyPoint.Sprn,
                            FuelType = Constants.FuelTypeElectricity
                        };

                        foreach (var accountSupplyPoint in electric)
                        {
                            foreach (var meter in accountSupplyPoint.SupplyPoint.MeterTechnicalDetails)
                            {
                                var ovoMeter = new Meter
                                {
                                    SerialNumber = meter.MeterSerialNumber,
                                    Type = meter.Type,
                                    Status = meter.Status
                                };

                                foreach (var detail in meter.MeterRegisters)
                                {
                                    var ovoMeterRegister = new Register
                                    {
                                        TimingCategory = detail.TimingCategory,
                                        UnitOfMeasurement = detail.UnitMeasurement,
                                        Id = detail.RegisterId
                                    };

                                    if (DateTime.TryParseExact(detail.RegisterStartDate,
                                            Constants.ZuluDateTimeFormat,
                                            CultureInfo.InvariantCulture,
                                            DateTimeStyles.AssumeUniversal, out var startDate))
                                    {
                                        ovoMeterRegister.StartDate = DateHelper.IsoDateOnly(startDate);
                                    }
                                    if (DateTime.TryParseExact(detail.RegisterEndDate,
                                            Constants.ZuluDateTimeFormat,
                                            CultureInfo.InvariantCulture,
                                            DateTimeStyles.AssumeUniversal, out var endDate))
                                    {
                                        ovoMeterRegister.EndDate = DateHelper.IsoDateOnly(endDate);
                                    }

                                    ovoMeter.Registers.Add(ovoMeterRegister);
                                }

                                ovoSupplyPoint.Meters.Add(ovoMeter);
                            }

                            foreach (var edge in accountSupplyPoint.MeterReadings.Edges)
                            {
                                var node = edge.MeterNode.MeterReadingData;
                                var ovoMeterReading = new Reading
                                {
                                    Type = node.Type,
                                    Date = node.Date,
                                    LifeCycle = node.Lifecycle,
                                    Source = node.Source,
                                    MeterSerialNumber = node.MeterSerialNumber
                                };

                                if (node.MeterReadingValues.Count > 0)
                                {
                                    ovoMeterReading.TimingCategory = node.MeterReadingValues[0].TimingCategory;
                                    ovoMeterReading.RegisterId = node.MeterReadingValues[0].RegisterId;
                                    ovoMeterReading.Value = node.MeterReadingValues[0].Value;
                                }

                                ovoSupplyPoint.Readings.Add(ovoMeterReading);
                            }

                            result.Add(ovoSupplyPoint);
                        }
                    }

                    var gas = readingsResponse.Data.Account.AccountSupplyPoints
                        .Where(s => s.SupplyPoint.FuelType.Equals(Constants.FuelTypeGas))
                        .ToList();
                    if (gas.Any())
                    {
                        var ovoSupplyPoint = new Models.Database.SupplyPoint
                        {
                            Sprn = gas[0].SupplyPoint.Sprn,
                            FuelType = Constants.FuelTypeGas
                        };

                        foreach (var accountSupplyPoint in gas)
                        {
                            foreach (var meter in accountSupplyPoint.SupplyPoint.MeterTechnicalDetails)
                            {
                                var ovoMeter = new Meter
                                {
                                    SerialNumber = meter.MeterSerialNumber,
                                    Type = meter.Type,
                                    Status = meter.Status
                                };

                                foreach (var detail in meter.MeterRegisters)
                                {
                                    var ovoMeterRegister = new Register
                                    {
                                        TimingCategory = detail.TimingCategory,
                                        UnitOfMeasurement = detail.UnitMeasurement,
                                        Id = detail.RegisterId
                                    };

                                    if (DateTime.TryParseExact(detail.RegisterStartDate,
                                            Constants.ZuluDateTimeFormat,
                                            CultureInfo.InvariantCulture,
                                            DateTimeStyles.AssumeUniversal, out var startDate))
                                    {
                                        ovoMeterRegister.StartDate = DateHelper.IsoDateOnly(startDate);
                                    }
                                    if (DateTime.TryParseExact(detail.RegisterEndDate,
                                            Constants.ZuluDateTimeFormat,
                                            CultureInfo.InvariantCulture,
                                            DateTimeStyles.AssumeUniversal, out var endDate))
                                    {
                                        ovoMeterRegister.EndDate = DateHelper.IsoDateOnly(endDate);
                                    }

                                    ovoMeter.Registers.Add(ovoMeterRegister);
                                }

                                ovoSupplyPoint.Meters.Add(ovoMeter);
                            }

                            foreach (var edge in accountSupplyPoint.MeterReadings.Edges)
                            {
                                var node = edge.MeterNode.MeterReadingData;
                                var ovoMeterReading = new Reading
                                {
                                    Type = node.Type,
                                    Date = node.Date,
                                    LifeCycle = node.Lifecycle,
                                    Source = node.Source,
                                    MeterSerialNumber = node.MeterSerialNumber,
                                    Value = node.Value
                                };

                                ovoSupplyPoint.Readings.Add(ovoMeterReading);
                            }
                        }

                        result.Add(ovoSupplyPoint);
                    }
                }
            }
        }
        catch (Exception exception)
        {
            _logger.WriteLine(exception.ToString());
            Debugger.Break();
        }

        return result;
    }
}