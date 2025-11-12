using Microsoft.Extensions.Configuration;
using Microsoft.Win32;
using OvoData.Helpers;
using OvoData.Models;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace OvoData.Forms
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IConfigurationRoot _configuration;

        private Account _selectedAccount;

        private bool _cancelRequested;

        private string _stopWhen = string.Empty;

        private HttpHelper _httpHelper;

        private Logger? _logger;
        private int logNumber;

        private DispatcherTimer _timer;

        public MainWindow()
        {
            InitializeComponent();

            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("AppSettings.json")
                .Build();

            // Initialize the timer
            _timer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1) // Set interval to 1 second
            };
            _timer.Tick += OnTick_Timer; // Attach the Tick event
            _timer.Start(); // Start the timer

            _httpHelper = new HttpHelper(_configuration);
            _selectedAccount = new Account();
        }

        private void OnTick_Timer(object? sender, EventArgs e)
        {
            UpdateDebugHelper();
        }

        private void UpdateDebugHelper()
        {
            if (Debugger.IsAttached)
            {
                var now = DateTime.Now;
                Value1.Text = $"{now:HH:mm:ss}";

                Value2.Foreground = _httpHelper.Tokens.AccessToken.HasExpired ? Brushes.Red : Brushes.Green;
                Value2.Text = $"{_httpHelper.Tokens.AccessToken.ExpiresAtTime:HH:mm:ss}";

                Value3.Foreground = _httpHelper.Tokens.RefreshToken.HasExpired ? Brushes.Red : Brushes.Green;
                Value3.Text = $"{_httpHelper.Tokens.RefreshToken.ExpiresAtTime:HH:mm:ss}";

                TokensVisualiser.InvalidateVisual();
                TokensVisualiser.InvalidateArrange();
                TokensVisualiser.InvalidateMeasure();
            }
        }

        private void OnLoaded_MainWindow(object sender, RoutedEventArgs e)
        {
            ReadFromRegistry();

            StopWhen.Items.Add(Constants.StopAfterThisMonth);
            StopWhen.Items.Add(Constants.StopAfterTwoMonths);
            StopWhen.Items.Add(Constants.StopAfterThisYear);
            StopWhen.Items.Add(Constants.NeverStop);
            StopWhen.SelectedIndex = 0;

            SetStatusText("Please log in to your account");

            if (!Debugger.IsAttached)
            {
                TokensVisualiser.Visibility = Visibility.Collapsed;
            }
        }

        private void ReadFromRegistry()
        {
            var key = Registry.CurrentUser.OpenSubKey(@$"SOFTWARE\{Constants.ApplicationName}");
            if (key != null)
            {
                UserName.Text = key.GetValue("Username")?.ToString();
                Password.Password = key.GetValue("Password")?.ToString();
            }
        }

        private void WriteToRegistry()
        {
            var key = Registry.CurrentUser.CreateSubKey(@$"SOFTWARE\{Constants.ApplicationName}");

            key.SetValue("Username", UserName.Text);
            key.SetValue("Password", Password.Password);
        }

        private void OnClick_Login(object sender, RoutedEventArgs e)
        {
            _logger = new Logger(ref logNumber);
            _httpHelper.SetLogger(_logger);

            if (string.IsNullOrEmpty(UserName.Text) && string.IsNullOrEmpty(Password.Password))
            {
                MessageBox.Show("Username and/or password are blank!", "Input Error");
            }
            else
            {
                SetMouseCursor();

                WriteToRegistry();

                SetStatusText("Connecting ...");

                if (_httpHelper.Login(UserName.Text, Password.Password, out var ovoAccounts))
                {
                    Login.IsEnabled = false;

                    if (ovoAccounts.Any())
                    {
                        foreach (var ovoAccount in ovoAccounts)
                        {
                            Accounts.Items.Add(ovoAccount);
                        }

                        if (ovoAccounts.Count == 1)
                        {
                            Accounts.SelectedIndex = 0;
                        }
                        else
                        {
                            SetStatusText("Please select an account");
                        }
                    }
                }

                ClearDown();
            }
        }

        private void OnSelectionChanged_Accounts(object sender, SelectionChangedEventArgs e)
        {
            if (Accounts.SelectedItems.Count == 1
                && Accounts.SelectedItem is Account account)
            {
                _selectedAccount = account;
                SetStateOfControls(true);

                ShowAccountInfo();
            }
        }

        private void OnSelectionChanged_StopWhen(object sender, SelectionChangedEventArgs e)
        {
            if (StopWhen.SelectedItem is string selection)
            {
                _stopWhen = selection;
            }
        }

        private void SetStateOfControls(bool state)
        {
            StopWhen.IsEnabled = state;
            ReadUsage.IsEnabled = state;
            ReadMeterReadings.IsEnabled = state;
            ExportUsage.IsEnabled = state;
            CancelOperations.IsEnabled = !state;
        }

        private void OnClick_ReadUsage(object sender, RoutedEventArgs e)
        {
            _logger = new Logger(ref logNumber);
            _httpHelper.SetLogger(_logger);
            var sqlite = new SqLiteHelper(_selectedAccount.Id, _logger);

            SetMouseCursor();
            SetStateOfControls(false);

            try
            {
                var thisYear = DateTime.Now.Year;
                var thisMonth = DateTime.Now.Month;
                var thisDay = DateTime.Now.Day;

                var year = thisYear;

                var monthsFetched = 0;

                while (!_cancelRequested)
                {
                    // Determine last month which is not in the future
                    var lastMonth = 12;
                    if (year == thisYear)
                    {
                        lastMonth = thisMonth;
                    }

                    SetStatusText($"Checking Year {year}");

                    var monthly = _httpHelper.ObtainMonthlyUsage(_selectedAccount.Id, year);

                    int monthlyReadings = 0;

                    if (monthly.Electricity != null && monthly.Electricity.Data != null)
                    {
                        sqlite.UpsertMonthly("Electric", monthly.Electricity.Data);

                        monthlyReadings += monthly.Electricity.Data.Count;
                    }

                    if (monthly.Gas != null && monthly.Gas.Data != null)
                    {
                        sqlite.UpsertMonthly("Gas", monthly.Gas.Data);

                        monthlyReadings += monthly.Gas.Data.Count;
                    }

                    if (monthlyReadings > 0)
                    {
                        for (var month = lastMonth; month >= 1; month--)
                        {
                            if (_cancelRequested)
                            {
                                break;
                            }

                            // Determine last day which is not in the future
                            var lastDay = LastDayInMonth(year, month);
                            if (year == thisYear && month == thisMonth)
                            {
                                lastDay = DateTime.Now.Day;
                            }

                            SetStatusText($"Checking Month {year}-{month:D2}", true);

                            if (year == thisYear && month == thisMonth
                                || sqlite.CountDaily("Electric", year, month) < lastDay
                                || sqlite.CountDaily("Gas", year, month) < lastDay)
                            {
                                SetStatusText($"Fetching Daily Usage for account {_selectedAccount.Id} - Month {year}-{month:D2}", true);
                                var daily = _httpHelper.ObtainDailyUsage(_selectedAccount.Id, year, month);

                                if (daily.Electricity != null && daily.Electricity.Data != null)
                                {
                                    sqlite.UpsertDaily("Electric", daily.Electricity.Data);
                                }

                                if (daily.Gas != null && daily.Gas.Data != null)
                                {
                                    sqlite.UpsertDaily("Gas", daily.Gas.Data);
                                }
                            }

                            for (var day = lastDay; day >= 1; day--)
                            {
                                if (_cancelRequested)
                                {
                                    break;
                                }

                                SetStatusText($"Checking Day {year}-{month:D2}-{day:D2}", true);

                                if (year == thisYear && month == thisMonth && day == thisDay
                                    || (sqlite.HasHalfHourly("Electric", year, month, day)
                                        && sqlite.CountHalfHourly("Electric", year, month, day) < 48)
                                    || (sqlite.HasHalfHourly("Gas", year, month, day)
                                        && sqlite.CountHalfHourly("Gas", year, month, day) < 48))
                                {
                                    SetStatusText($"Fetching Half Hourly Usage for account {_selectedAccount.Id} - Day {year}-{month:D2}-{day:D2}", true);
                                    var halfHourly = _httpHelper.ObtainHalfHourlyUsage(_selectedAccount.Id, year, month, day);

                                    if (halfHourly.Electricity != null && halfHourly.Electricity.Data != null)
                                    {
                                        sqlite.UpsertHalfHourly("Electric", halfHourly.Electricity.Data);
                                    }

                                    if (halfHourly.Gas != null && halfHourly.Gas.Data != null)
                                    {
                                        sqlite.UpsertHalfHourly("Gas", halfHourly.Gas.Data);
                                    }
                                }
                            }

                            if (_stopWhen.Equals(Constants.StopAfterThisMonth) && year == thisYear && month == thisMonth)
                            {
                                _cancelRequested = true;
                            }

                            monthsFetched++;

                            if (_stopWhen.Equals(Constants.StopAfterTwoMonths) && monthsFetched >= 2)
                            {
                                _cancelRequested = true;
                            }
                        }
                    }

                    if (_stopWhen.Equals(Constants.StopAfterThisYear) && year == thisYear)
                    {
                        _cancelRequested = true;
                        break;
                    }

                    if (monthlyReadings == 0)
                    {
                        _cancelRequested = true;
                        break;
                    }

                    year--;
                }
            }
            catch (Exception exception)
            {
                _logger.WriteLine(exception.ToString());
                MessageBox.Show(exception.ToString(), "Exception");
            }
            finally
            {
                ClearDown();
                ShowAccountInfo();
            }
        }

        private void ShowAccountInfo()
        {
            SetStatusText($"Account Id: {_selectedAccount.Id} selected");

            var sqlite = new SqLiteHelper(_selectedAccount.Id, _logger!);
            AccountStatistics.ItemsSource = sqlite.GetUsageInformation();
        }

        private void ClearDown()
        {
            CursorManager.ClearWaitCursor(CancelOperations);
            _cancelRequested = false;

            SetStatusText("");
            SetStateOfControls(true);

            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        private void OnClick_CancelOperations(object sender, RoutedEventArgs e)
        {
            _cancelRequested = true;
        }

        private void OnClick_ExportUsage(object sender, RoutedEventArgs e)
        {
            _logger = new Logger(ref logNumber);

            SetStateOfControls(false);

            var window = new Export(this, _logger!)
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Account = _selectedAccount.Id
            };
            window.ShowDialog();

            ClearDown();
            ShowAccountInfo();
        }

        private static int LastDayInMonth(int year, int month)
        {
            return DateTime.DaysInMonth(year, month);
        }

        public void SetStatusText(string message, bool log = false)
        {
            if (log)
            {
                _logger?.WriteLine(message);
            }
            Status.Text = message;
            UpdateDebugHelper();
            DoWpfEvents();
        }

        private static void DoWpfEvents()
        {
            try
            {
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));
            }
            catch
            {
                // Nothing we can do here
            }
        }

        private void OnClick_ReadMeterReadings(object sender, RoutedEventArgs e)
        {
            _logger = new Logger(ref logNumber);
            _httpHelper.SetLogger(_logger);
            var sqlite = new SqLiteHelper(_selectedAccount.Id, _logger);

            try
            {
                SetMouseCursor();
                SetStateOfControls(false);

                var supplyPoints = _httpHelper.ObtainMeterReadings(_selectedAccount.Id);

                SetStatusText("Updating meter readings ...", true);

                foreach (var supplyPoint in supplyPoints)
                {
                    var fuelType = StringHelper.ProperCase(supplyPoint.FuelType);

                    sqlite.UpsertSupplyPoint(supplyPoint);

                    foreach (var meter in supplyPoint.Meters)
                    {
                        sqlite.UpsertMeter(meter, supplyPoint.FuelType);

                        foreach (var register in meter.Registers)
                        {
                            DoWpfEvents();
                            if (_cancelRequested)
                            {
                                break;
                            }

                            sqlite.UpsertMeterRegisters(register, supplyPoint.FuelType);
                        }
                        _logger?.WriteLine($"Saved {meter.Registers.Count} {fuelType} Meter Registers for Meter {meter.SerialNumber}");
                    }
                    _logger?.WriteLine($"Saved {supplyPoint.Meters.Count} {fuelType} Meters");

                    var idx = 0;
                    var records = 0;
                    foreach (var reading in supplyPoint.Readings)
                    {
                        sqlite.UpsertMeterReading(reading, supplyPoint.FuelType);

                        idx++;
                        records++;

                        DoWpfEvents();
                        if (_cancelRequested)
                        {
                            break;
                        }

                        if (idx >= 25)
                        {
                            idx = 0;
                            SetStatusText($"Saved {records} {fuelType} readings");
                        }
                    }

                    _logger?.WriteLine($"Saved {records} {fuelType} readings");
                }

                _logger?.WriteLine($"Saved {supplyPoints.Count} Supply Points");
            }
            catch (Exception exception)
            {
                _logger?.WriteLine(exception.ToString());
                MessageBox.Show(exception.ToString(), "Exception");
            }
            finally
            {
                ClearDown();
                ShowAccountInfo();
            }
        }

        private void SetMouseCursor()
        {
            CursorManager.SetWaitCursorExcept(CancelOperations);
        }
    }
}