using Microsoft.Extensions.Configuration;
using Microsoft.Win32;
using OvoData.Helpers;
using OvoData.Models.Database;
using OvoData.Models.OvoApi;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace OvoData
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IConfigurationRoot _configuration;

        private string _selectedAccountId;

        private bool _cancelRequested;

        private string _stopWhen;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void OnLoaded_MainWindow(object sender, RoutedEventArgs e)
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("AppSettings.json")
                .Build();

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
                WriteToRegistry();

                var details = new LoginRequest
                {
                    Username = UserName.Text,
                    Password = Password.Password,
                    RememberMe = true
                };

                SetStatusText("Connecting ...");

                if (HttpHelper.Login(_configuration, details))
                {
                    Login.IsEnabled = false;
                    SetStatusText("Obtaining your account(s) ...");

                    var accounts = HttpHelper.GetAccountIds(_configuration);

                    if (accounts.AccountIds.Any())
                    {
                        foreach (var accountId in accounts.AccountIds)
                        {
                            Accounts.Items.Add(accountId);
                        }

                        if (accounts.AccountIds.Count == 1)
                        {
                            Accounts.SelectedIndex = 0;
                        }
                        else
                        {
                            SetStatusText("Please select an account");
                        }
                    }
                }
            }
        }

        private void OnSelectionChanged_Accounts(object sender, SelectionChangedEventArgs e)
        {
            if (Accounts.SelectedItems.Count == 1
                && Accounts.SelectedItem is string accountId)
            {
                _selectedAccountId = accountId;
                SetStateOfControls(true);

                var sqlite = new SqliteHelper(_selectedAccountId);

                var info = sqlite.GetInformation();
                FirstDate.Text = info.FirstDay;
                LastDate.Text = info.LastDay;

                SetStatusText("");
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
            Read.IsEnabled = state;
            StopWhen.IsEnabled = state;
            Export.IsEnabled = state;

            Cancel.IsEnabled = !state;
        }

        private void OnClick_Read(object sender, RoutedEventArgs e)
        {
            SetStateOfControls(false);
            _cancelRequested = false;

            try
            {
                var info = new Information
                {
                    AccountId = _selectedAccountId,
                    FirstDay = FirstDate.Text,
                    LastDay = LastDate.Text
                };

                var thisYear = DateTime.Now.Year;
                var thisMonth = DateTime.Now.Month;
                var thisDay = DateTime.Now.Day;

                var year = thisYear;

                var monthsFetched = 0;

                var sqlite = new SqliteHelper(_selectedAccountId);

                while (!_cancelRequested)
                {
                    // Determine last month which is not in the future
                    var lastMonth = 12;
                    if (year == thisYear)
                    {
                        lastMonth = thisMonth;
                    }

                    SetStatusText($"Checking Year {year}");

                    var monthly = HttpHelper.GetMonthlyUsage(_configuration, _selectedAccountId, year);

                    int monthlyReadings = 0;

                    if (monthly.Electricity != null && monthly.Electricity.Data != null)
                    {
                        sqlite.UpsertMonthly("Electric", monthly.Electricity.Data);

                        monthlyReadings += monthly.Electricity.Data.Count;

                        SetFirstMonth(monthly.Electricity.Data, year, info);
                        SetLastMonth(monthly.Electricity.Data, year, info);
                    }

                    if (monthly.Gas != null && monthly.Gas.Data != null)
                    {
                        sqlite.UpsertMonthly("Gas", monthly.Gas.Data);

                        monthlyReadings += monthly.Gas.Data.Count;

                        SetFirstMonth(monthly.Gas.Data, year, info);
                        SetLastMonth(monthly.Gas.Data, year, info);
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
                                SetStatusText($"Fetching Daily Usage for account {_selectedAccountId} - Month {year}-{month:D2}");
                                var daily = HttpHelper.GetDailyUsage(_configuration, _selectedAccountId, year, month);

                                if (daily.Electricity != null && daily.Electricity.Data != null)
                                {
                                    sqlite.UpsertDaily("Electric", daily.Electricity.Data);

                                    SetFirstDay(daily.Electricity.Data, info);
                                    SetLastDay(daily.Electricity.Data, info);
                                }

                                if (daily.Gas != null && daily.Gas.Data != null)
                                {
                                    sqlite.UpsertDaily("Gas", daily.Gas.Data);

                                    SetFirstDay(daily.Gas.Data, info);
                                    SetLastDay(daily.Gas.Data, info);
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
                                    SetStatusText($"Fetching Half Hourly Usage for account {_selectedAccountId} - Day {year}-{month:D2}-{day:D2}");
                                    var halfHourly = HttpHelper.GetHalfHourlyUsage(_configuration, _selectedAccountId, year, month, day);

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

                sqlite.UpsertInformation(info);

                FirstDate.Text = info.FirstDay;
                LastDate.Text = info.LastDay;
            }
            catch (Exception exception)
            {
                Logger.WriteLine(exception.ToString());
                MessageBox.Show(exception.ToString(), "Exception");
            }

            SetStatusText("");
            SetStateOfControls(true);

            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        private void OnClick_Cancel(object sender, RoutedEventArgs e)
        {
            _cancelRequested = true;
        }

        private void OnClick_Export(object sender, RoutedEventArgs e)
        {
            SetStateOfControls(false);

            var window = new Export
            {
                Owner = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Account = _selectedAccountId,
                Parent = this
            };
            window.ShowDialog();

            SetStateOfControls(true);

            GC.WaitForPendingFinalizers();
            GC.Collect();
        }

        private void SetFirstDay(List<DailyDataItem> data, Information info)
        {
            if (data.Any())
            {
                var minDate = data.Min(x => x.Interval.Start).ToString("yyyy-MM-dd");
                if (string.IsNullOrEmpty(info.FirstDay) || string.CompareOrdinal(minDate, info.FirstDay) < 0)
                {
                    info.FirstDay = minDate;
                }
            }
        }

        private void SetFirstMonth(List<MonthlyDataItem> data, int year, Information info)
        {
            if (data.Any())
            {
                var minMonth = $"{year}-{data.Min(x => x.Month):D2}";
                if (string.IsNullOrEmpty(info.FirstMonth) || string.CompareOrdinal(minMonth, info.FirstMonth) < 0)
                {
                    info.FirstMonth = minMonth;
                }
            }
        }

        private void SetLastDay(List<DailyDataItem> data, Information info)
        {
            if (data.Any())
            {
                var maxDate = data.Max(x => x.Interval.Start).ToString("yyyy-MM-dd");
                if (string.IsNullOrEmpty(info.FirstDay) || string.CompareOrdinal(maxDate, info.LastDay) > 0)
                {
                    info.LastDay = maxDate;
                }
            }
        }

        private void SetLastMonth(List<MonthlyDataItem> data, int year, Information info)
        {
            if (data.Any())
            {
                var maxMonth = $"{year}-{data.Max(x => x.Month):D2}";
                if (string.IsNullOrEmpty(info.LastMonth) || string.CompareOrdinal(maxMonth, info.LastMonth) > 0)
                {
                    info.LastMonth = maxMonth;
                }
            }
        }

        private int LastDayInMonth(int year, int month)
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
    }
}