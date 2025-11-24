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
    private LoginRequest? _loginRequest;
    private Logger? _logger;

    public Tokens Tokens { get; set; } = new();

    /// <summary>
    /// This helper <b>MUST</b> only be instantiated ONCE, otherwise obtaining subsequent Access Tokens fails.
    /// </summary>
    /// <param name="configuration"></param>
    public HttpHelper(IConfigurationRoot configuration)
    {
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
        _configuration = configuration;
    }

    public void SetLogger(Logger logger)
    {
        _logger = logger;
    }

    private static readonly JsonSerializerOptions JsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public bool Login(string username, string password, out List<Models.Account> ovoAccounts)
    {
        var result = false;
        ovoAccounts = [];

        try
        {
            _loginRequest = new LoginRequest
            {
                Username = username,
                Password = password
            };

            if (DoLogin(_loginRequest))
            {
                DoGetAccessToken();
                ovoAccounts = DoGetOvoAccounts();
                result = true;
            }
        }
        catch (Exception exception)
        {
            _logger?.WriteLine(exception.ToString());
        }

        return result;
    }

    private bool DoLogin(LoginRequest loginRequest)
    {
        var result = false;

        try
        {
            var uri = new Uri(_configuration["LoginUri"]!);
            var request = new HttpRequestMessage(HttpMethod.Post, uri);
            var requestContent = JsonSerializer.Serialize(loginRequest, JsonSerializerOptions);
            request.Content = new StringContent(requestContent, Encoding.ASCII, "application/json");

            _logger?.WriteLine($"Calling API endpoint at Uri: {uri} to Log in as '{loginRequest.Username}'");

            var response = _httpClient1.SendAsync(request).Result;
            if (response.IsSuccessStatusCode)
            {
                var responseContent = response.Content.ReadAsStringAsync().Result;
                if (ConfigHelper.GetBoolean(_configuration, "DumpData", false))
                {
                    _logger?.DumpJson("Login-Response", responseContent);
                }

                var loginResponse = JsonSerializer.Deserialize<LoginResponse>(responseContent, JsonSerializerOptions);
                if (loginResponse != null)
                {
                    Tokens.UserGuid = loginResponse.UserId;
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

                if (cookies.Count > 0)
                {
                    var cookie = cookies.FirstOrDefault(c => c.Name == "restricted_refresh_token");
                    if (cookie != null)
                    {
                        Tokens.RefreshToken.Jwt = cookie.Value;
                        _logger?.DumpJson("Refresh-Token", Tokens.RefreshToken.ToString());

                        result = true;
                    }
                }
            }
            else
            {
                _logger?.WriteLine($"{response.StatusCode} {response.ReasonPhrase}");
                Debugger.Break();
            }
        }
        catch (Exception exception)
        {
            _logger?.WriteLine(exception.ToString());
            Debugger.Break();
        }

        return result;
    }

    private void CheckTokens()
    {
        if (Tokens.AccessToken.HasExpired)
        {
            Debug.WriteLine($"Access token expired at {Tokens.AccessToken.ExpiresAtTime:HH:mm:ss}");
        }
        if (Tokens.RefreshToken.HasExpired)
        {
            _logger?.WriteLine($"Refresh token expired at {Tokens.RefreshToken.ExpiresAtTime:HH:mm:ss}");
        }

        if (Tokens.RefreshToken.HasExpired)
        {
            if (_loginRequest != null
                && DoLogin(_loginRequest))
            {
                DoGetAccessToken();
            }
        }
        else
        {
            if (Tokens.AccessToken.HasExpired)
            {
                DoGetAccessToken();
            }
        }

        // Tokens are both valid
    }

    private void DoGetAccessToken()
    {
        try
        {
            var uri = new Uri(_configuration["TokenUri"]!);
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Add("restricted_refresh_token", Tokens.RefreshToken.Jwt);

            var response = _httpClient1.SendAsync(request).Result;
            if (response.IsSuccessStatusCode)
            {
                var responseContent = response.Content.ReadAsStringAsync().Result;
                var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(responseContent, JsonSerializerOptions);

                if (tokenResponse != null)
                {
                    if (ConfigHelper.GetBoolean(_configuration, "DumpData", false))
                    {
                        _logger?.DumpJson("DoGetAccessToken-Response", responseContent);
                    }

                    Tokens.AccessToken.Jwt = tokenResponse.AccessToken.Value;

                    _logger?.DumpJson("Access-Token", Tokens.AccessToken.ToString());

                    Debug.WriteLine($"Current Time             {DateTime.Now:dd-MMM-yyyy HH:mm:ss}");
                    Debug.WriteLine($"Access  Token Expires at {Tokens.AccessToken.ExpiresAtTime:dd-MMM-yyyy HH:mm:ss}");
                    Debug.WriteLine($"Refresh Token Expires at {Tokens.RefreshToken.ExpiresAtTime:dd-MMM-yyyy HH:mm:ss}");
                }
            }
            else
            {
                _logger?.WriteLine($"{response.StatusCode} {response.ReasonPhrase}");
                Debugger.Break();
            }
        }
        catch (Exception exception)
        {
            _logger?.WriteLine(exception.ToString());
            Debugger.Break();
        }
    }

    private List<Models.Account> DoGetOvoAccounts()
    {
        var result = new List<Models.Account>();

        try
        {
            var uri = new Uri(_configuration["AccountsUri"]!);
            var request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Headers.Add("Authorization", $"Bearer {Tokens.AccessToken.Jwt}");

            var query = string.Join(@"\n", ResourceHelper.GetStringResource("GraphQL.Accounts.query").Split(Environment.NewLine));
            var graphQl = ResourceHelper.GetStringResource("GraphQL.Accounts.json");
            graphQl = graphQl.Replace("[[customerGuid]]", Tokens.UserGuid).Replace("[[query]]", query);

            var content = new StringContent(graphQl, null, "application/json");
            request.Content = content;

            _logger?.WriteLine($"Calling API endpoint at Uri: {uri} to obtain account details");

            var response = _httpClient2.SendAsync(request).Result;
            response.EnsureSuccessStatusCode();
            if (response.IsSuccessStatusCode)
            {
                var responseContent = response.Content.ReadAsStringAsync().Result;
                if (ConfigHelper.GetBoolean(_configuration, "DumpData", false))
                {
                    _logger?.DumpJson("Accounts-Response", JsonHelper.Prettify(responseContent));
                }
                var accountsResponse = JsonSerializer.Deserialize<UsageResponse>(responseContent, JsonSerializerOptions);
                if (accountsResponse != null)
                {
                    var accounts = accountsResponse.Data.Customer.Relationships.Accounts.ToList();
                    foreach (var account in accounts)
                    {
                        var ovoAccount = new Models.Account
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
            else
            {
                _logger?.WriteLine($"{response.StatusCode} {response.ReasonPhrase}");
                Debugger.Break();
            }
        }
        catch (Exception exception)
        {
            _logger?.WriteLine(exception.ToString());
            Debugger.Break();
        }

        return result;
    }

    public MonthlyResponse ObtainMonthlyUsage(string accountId, int year)
    {
        var result = new MonthlyResponse();

        try
        {
            CheckTokens();

            var uri = string.Format(_configuration["MonthlyUri"]!, accountId, year);
            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Add("Authorization", $"Bearer {Tokens.AccessToken.Jwt}");

            _logger?.WriteLine($"Calling API endpoint at Uri: {uri}");

            var response = _httpClient3.SendAsync(request).Result;
            if (response.IsSuccessStatusCode)
            {
                var content = response.Content.ReadAsStringAsync().Result;
                if (ConfigHelper.GetBoolean(_configuration, "DumpData", false))
                {
                    _logger?.DumpJson($"{nameof(ObtainMonthlyUsage)}-{year}", content);
                }
                result = JsonSerializer.Deserialize<MonthlyResponse>(content, JsonSerializerOptions);
            }
            else
            {
                _logger?.WriteLine($"{response.StatusCode} {response.ReasonPhrase}");
                Debugger.Break();
            }
        }
        catch (Exception exception)
        {
            _logger?.WriteLine(exception.ToString());
            Debugger.Break();
        }

        return result!;
    }

    public DailyResponse ObtainDailyUsage(string accountId, int year, int month)
    {
        var result = new DailyResponse();

        try
        {
            CheckTokens();

            var uri = string.Format(_configuration["DailyUri"]!, accountId, $"{year}-{month:D2}");
            _logger?.WriteLine($"Calling API endpoint at Uri: {uri}");

            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Add("Authorization", $"Bearer {Tokens.AccessToken.Jwt}");

            var response = _httpClient3.SendAsync(request).Result;
            if (response.IsSuccessStatusCode)
            {
                var content = response.Content.ReadAsStringAsync().Result;
                if (ConfigHelper.GetBoolean(_configuration, "DumpData", false))
                {
                    _logger?.DumpJson($"{nameof(ObtainDailyUsage)}-{year}-{month:D2}", content);
                }
                result = JsonSerializer.Deserialize<DailyResponse>(content, JsonSerializerOptions);
            }
            else
            {
                _logger?.WriteLine($"{response.StatusCode} {response.ReasonPhrase}");
                Debugger.Break();
            }
        }
        catch (Exception exception)
        {
            _logger?.WriteLine(exception.ToString());
            Debugger.Break();
        }

        return result;
    }

    public HalfHourlyResponse ObtainHalfHourlyUsage(string accountId, int year, int month, int day)
    {
        var result = new HalfHourlyResponse();

        try
        {
            CheckTokens();

            var uri = string.Format(_configuration["HalfHourlyUri"]!, accountId, $"{year}-{month:D2}-{day:D2}");
            _logger?.WriteLine($"Calling API endpoint at Uri: {uri}");

            var request = new HttpRequestMessage(HttpMethod.Get, uri);
            request.Headers.Add("Authorization", $"Bearer {Tokens.AccessToken.Jwt}");

            var response = _httpClient3.SendAsync(request).Result;
            if (response.IsSuccessStatusCode)
            {
                var content = response.Content.ReadAsStringAsync().Result;
                if (ConfigHelper.GetBoolean(_configuration, "DumpData", false))
                {
                    _logger?.DumpJson($"{nameof(ObtainHalfHourlyUsage)}-{year}-{month:D2}-{day:D2}", content);
                }
                result = JsonSerializer.Deserialize<HalfHourlyResponse>(content, JsonSerializerOptions);
            }
            else
            {
                _logger?.WriteLine($"{response.StatusCode} {response.ReasonPhrase}");
                Debugger.Break();
            }
        }
        catch (Exception exception)
        {
            _logger?.WriteLine(exception.ToString());
            Debugger.Break();
        }

        return result;
    }

    public List<Models.MySupplyPoint> ObtainMeterReadings(string accountId)
    {
        List<Models.MySupplyPoint> result = [];

        try
        {
            CheckTokens();

            var query = string.Join(@"\n", ResourceHelper.GetStringResource("GraphQL.Readings.query").Split(Environment.NewLine));
            var graphQl = ResourceHelper.GetStringResource("GraphQL.Readings.json");
            graphQl = graphQl.Replace("[[accountId]]", accountId).Replace("[[query]]", query);

            var uri = new Uri(_configuration["ReadingsUri"]!);
            var request = new HttpRequestMessage(HttpMethod.Post, uri);
            request.Headers.Add("Authorization", $"Bearer {Tokens.AccessToken.Jwt}");

            var content = new StringContent(graphQl, null, "application/json");
            request.Content = content;

            _logger?.WriteLine($"Calling API endpoint at Uri: {uri} to obtain meter readings");

            var response = _httpClient2.SendAsync(request).Result;
            response.EnsureSuccessStatusCode();
            if (response.IsSuccessStatusCode)
            {
                var responseContent = response.Content.ReadAsStringAsync().Result;

                if (ConfigHelper.GetBoolean(_configuration, "DumpData", false))
                {
                    _logger?.DumpJson("Readings-Response", JsonHelper.Prettify(responseContent));
                }

                var readingsResponse = JsonSerializer.Deserialize<ReadingsResponse>(responseContent, JsonSerializerOptions);
                if (readingsResponse != null)
                {
                    Debug.WriteLine(readingsResponse.Data.Account.Id);

                    var electric = readingsResponse.Data.Account.MeterSupplyPoints
                        .Where(s => s.SupplyPoint.FuelType.Equals(Constants.FuelTypeElectricity))
                        .ToList();
                    if (electric.Any())
                    {
                        var ovoSupplyPoint = new Models.MySupplyPoint
                        {
                            Sprn = electric[0].SupplyPoint.Sprn,
                            FuelType = Constants.FuelTypeElectricity
                        };

                        if (DateTime.TryParseExact(electric[0].StartDate,
                                Constants.ShortDateFormat,
                                CultureInfo.InvariantCulture,
                                DateTimeStyles.AssumeUniversal, out var supplyStartDate))
                        {
                            ovoSupplyPoint.StartDate = supplyStartDate;
                        }

                        if (!string.IsNullOrEmpty(electric[0].Ending.Date) && DateTime.TryParseExact(
                                electric[0].Ending.Date,
                                Constants.ShortDateFormat,
                                CultureInfo.InvariantCulture,
                                DateTimeStyles.AssumeUniversal, out var supplyEndDate))
                        {
                            ovoSupplyPoint.EndDate = supplyEndDate;
                        }

                        foreach (var accountSupplyPoint in electric)
                        {
                            foreach (var meter in accountSupplyPoint.SupplyPoint.MeterTechnicalDetails)
                            {
                                var ovoMeter = new SqLiteMeter
                                {
                                    SerialNumber = meter.MeterSerialNumber,
                                    FuelType = meter.Type,
                                    Status = meter.Status
                                };

                                foreach (var detail in meter.MeterRegisters)
                                {
                                    var ovoMeterRegister = new SqLiteRegister
                                    {
                                        TimingCategory = detail.TimingCategory,
                                        UnitOfMeasurement = detail.UnitMeasurement,
                                        Id = detail.RegisterId,
                                        MeterSerialNumber = ovoMeter.SerialNumber
                                    };

                                    if (DateTime.TryParseExact(detail.RegisterStartDate,
                                            Constants.ZuluDateTimeFormat,
                                            CultureInfo.InvariantCulture,
                                            DateTimeStyles.AssumeUniversal,
                                            out var startDate))
                                    {
                                        ovoMeterRegister.StartDate = DateHelper.IsoDateOnly(startDate);
                                    }

                                    if (DateTime.TryParseExact(detail.RegisterEndDate,
                                            Constants.ZuluDateTimeFormat,
                                            CultureInfo.InvariantCulture,
                                            DateTimeStyles.AssumeUniversal,
                                            out var endDate))
                                    {
                                        ovoMeterRegister.EndDate = DateHelper.IsoDateOnly(endDate);
                                    }

                                    ovoMeter.Registers.Add(ovoMeterRegister);
                                }

                                ovoSupplyPoint.Meters.Add(ovoMeter);
                            }

                            foreach (var meterReadingEdge in accountSupplyPoint.Readings.Edges)
                            {
                                var meterReadingData = meterReadingEdge.MeterNode.MeterReadingData;
                                foreach (ElectricMeterValue meterValue in meterReadingData.ElectricMeterValues)
                                {
                                    var ovoMeterReading = new SqLiteReading
                                    {
                                        FuelType = meterReadingData.Type,
                                        Date = meterReadingData.Date,
                                        LifeCycle = meterReadingData.Lifecycle,
                                        Source = meterReadingData.Source,
                                        MeterSerialNumber = meterReadingData.MeterSerialNumber,
                                        TimingCategory = meterValue.TimingCategory,
                                        RegisterId = meterValue.RegisterId,
                                        Value = meterValue.Value
                                    };

                                    ovoSupplyPoint.Readings.Add(ovoMeterReading);
                                }
                            }

                            result.Add(ovoSupplyPoint);
                        }
                    }

                    var gas = readingsResponse.Data.Account.MeterSupplyPoints
                        .Where(s => s.SupplyPoint.FuelType.Equals(Constants.FuelTypeGas))
                        .ToList();
                    if (gas.Any())
                    {
                        var ovoSupplyPoint = new Models.MySupplyPoint
                        {
                            Sprn = gas[0].SupplyPoint.Sprn,
                            FuelType = Constants.FuelTypeGas
                        };

                        if (DateTime.TryParseExact(gas[0].StartDate,
                                Constants.ShortDateFormat,
                                CultureInfo.InvariantCulture,
                                DateTimeStyles.AssumeUniversal, out var supplyStartDate))
                        {
                            ovoSupplyPoint.StartDate = supplyStartDate;
                        }

                        if (!string.IsNullOrEmpty(gas[0].Ending.Date) && DateTime.TryParseExact(
                                electric[0].Ending.Date,
                                Constants.ShortDateFormat,
                                CultureInfo.InvariantCulture,
                                DateTimeStyles.AssumeUniversal, out var supplyEndDate))
                        {
                            ovoSupplyPoint.EndDate = supplyEndDate;
                        }

                        foreach (var accountSupplyPoint in gas)
                        {
                            foreach (var meter in accountSupplyPoint.SupplyPoint.MeterTechnicalDetails)
                            {
                                var ovoMeter = new SqLiteMeter
                                {
                                    SerialNumber = meter.MeterSerialNumber,
                                    FuelType = meter.Type,
                                    Status = meter.Status
                                };

                                foreach (var detail in meter.MeterRegisters)
                                {
                                    var ovoMeterRegister = new SqLiteRegister
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

                            foreach (var meterReadingEdge in accountSupplyPoint.Readings.Edges)
                            {
                                var meterReadingData = meterReadingEdge.MeterNode.MeterReadingData;
                                var ovoMeterReading = new SqLiteReading
                                {
                                    FuelType = meterReadingData.Type,
                                    Date = meterReadingData.Date,
                                    LifeCycle = meterReadingData.Lifecycle,
                                    Source = meterReadingData.Source,
                                    MeterSerialNumber = meterReadingData.MeterSerialNumber,
                                    Value = meterReadingData.GasMeterValue
                                };

                                ovoSupplyPoint.Readings.Add(ovoMeterReading);
                            }
                        }

                        result.Add(ovoSupplyPoint);
                    }
                }
            }
            else
            {
                _logger?.WriteLine($"{response.StatusCode} {response.ReasonPhrase}");
                Debugger.Break();
            }
        }
        catch (Exception exception)
        {
            _logger?.WriteLine(exception.ToString());
            Debugger.Break();
        }

        return result;
    }
}