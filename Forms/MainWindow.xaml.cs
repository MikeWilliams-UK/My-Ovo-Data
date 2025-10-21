using Microsoft.Extensions.Configuration;
using Microsoft.Win32;
using OvoData.Helpers;
using OvoData.Models.Api.Login;
using OvoData.Models.Database;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
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

        private Tokens _tokens;
        private Account _selectedAccount;

        private bool _cancelRequested;

        private string _stopWhen = string.Empty;

        private HttpHelper _httpHelper;

        public MainWindow()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("AppSettings.json")
                .Build();

            InitializeComponent();

            _httpHelper = new HttpHelper(_configuration);
            _tokens = new Tokens();
            _selectedAccount = new Account();
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
            if (string.IsNullOrEmpty(UserName.Text) && string.IsNullOrEmpty(Password.Password))
            {
                MessageBox.Show("Username and/or password are blank!", "Input Error");
            }
            else
            {
                SetMouseCursor();

                WriteToRegistry();

                SetStatusText("Connecting ...");

                if (_httpHelper.FirstLogin(UserName.Text, Password.Password, out var tokens, out var ovoAccounts))
                {
                    Login.IsEnabled = false;

                    if (ovoAccounts.Any())
                    {
                        _tokens = tokens;

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

                var sqlite = new SqLiteHelper(_selectedAccount.Id);
                AccountInformation.ItemsSource = sqlite.GetUsageInformation();

                SetStatusText($"Account Id: {_selectedAccount.Id} selected");
                Debug.WriteLine($"  HasElectric: {_selectedAccount.HasElectric} from {_selectedAccount.ElectricStartDate}");
                Debug.WriteLine($"  HasGas:      {_selectedAccount.HasGas} from {_selectedAccount.GasStartDate}");
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
            ExportUsage.IsEnabled = state;

            ReadMeterReadings.IsEnabled = state;
            ExportReadings.IsEnabled = state;

            CancelOperations.IsEnabled = !state;
        }

        private void OnClick_ReadUsage(object sender, RoutedEventArgs e)
        {
            SetMouseCursor();
            SetStateOfControls(false);

            try
            {
                var thisYear = DateTime.Now.Year;
                var thisMonth = DateTime.Now.Month;
                var thisDay = DateTime.Now.Day;

                var year = thisYear;

                var monthsFetched = 0;

                var sqlite = new SqLiteHelper(_selectedAccount.Id);

                while (!_cancelRequested)
                {
                    // Determine last month which is not in the future
                    var lastMonth = 12;
                    if (year == thisYear)
                    {
                        lastMonth = thisMonth;
                    }

                    SetStatusText($"Checking Year {year}");

                    var monthly = _httpHelper.ObtainMonthlyUsage(_tokens, _selectedAccount.Id, year);

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

                            SetStatusText($"Checking Month {year}-{month:D2}");

                            if (year == thisYear && month == thisMonth
                                || sqlite.CountDaily("Electric", year, month) < lastDay
                                || sqlite.CountDaily("Gas", year, month) < lastDay)
                            {
                                SetStatusText($"Fetching Daily Usage for account {_selectedAccount.Id} - Month {year}-{month:D2}");
                                var daily = _httpHelper.ObtainDailyUsage(_tokens, _selectedAccount.Id, year, month);

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

                                SetStatusText($"Checking Day {year}-{month:D2}-{day:D2}");

                                if (year == thisYear && month == thisMonth && day == thisDay
                                    || (sqlite.HasHalfHourly("Electric", year, month, day)
                                        && sqlite.CountHalfHourly("Electric", year, month, day) < 48)
                                    || (sqlite.HasHalfHourly("Gas", year, month, day)
                                        && sqlite.CountHalfHourly("Gas", year, month, day) < 48))
                                {
                                    SetStatusText($"Fetching Half Hourly Usage for account {_selectedAccount.Id} - Day {year}-{month:D2}-{day:D2}");
                                    var halfHourly = _httpHelper.ObtainHalfHourlyUsage(_tokens, _selectedAccount.Id, year, month, day);

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
                Logger.WriteLine(exception.ToString());
                MessageBox.Show(exception.ToString(), "Exception");
            }
            finally
            {
                ClearDown();
            }
        }

        private void ClearDown()
        {
            var sqlite = new SqLiteHelper(_selectedAccount.Id);
            AccountInformation.ItemsSource = sqlite.GetUsageInformation();

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
            SetStateOfControls(false);

            var window = new Export(this)
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Account = _selectedAccount.Id
            };
            window.ShowDialog();
            ClearDown();
        }

        private static int LastDayInMonth(int year, int month)
        {
            return DateTime.DaysInMonth(year, month);
        }

        public void SetStatusText(string message)
        {
            Logger.WriteLine(message);
            Status.Text = message;
            DoWpfEvents();
        }

        private static void DoWpfEvents()
        {
            Application.Current.Dispatcher.Invoke(DispatcherPriority.Background, new ThreadStart(delegate { }));
        }

        private void OnClick_ReadMeterReadings(object sender, RoutedEventArgs e)
        {
            try
            {
                SetMouseCursor();
                SetStateOfControls(false);

                var supplyPoints = _httpHelper.ObtainMeterReadings(_tokens, _selectedAccount.Id);

                SetStatusText("Updating values");

                var sqlite = new SqLiteHelper(_selectedAccount.Id);

                foreach (var supplyPoint in supplyPoints)
                {
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
                    }

                    int idx = 0;
                    int records = 0;
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
                            SetStatusText($"Saved {records} {StringHelper.ProperCase(supplyPoint.FuelType)} readings");
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.WriteLine(exception.ToString());
                MessageBox.Show(exception.ToString(), "Exception");
            }
            finally
            {
                ClearDown();
            }
        }

        private void SetMouseCursor()
        {
            CursorManager.SetWaitCursorExcept(CancelOperations);
        }

        private void OnClick_ExportReadings(object sender, RoutedEventArgs e)
        {
            //ToDo: Export the data from SQLite
        }
    }
}