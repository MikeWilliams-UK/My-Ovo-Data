using CsvHelper;
using Ookii.Dialogs.Wpf;
using OpenSpreadsheet;
using OpenSpreadsheet.Configuration;
using OvoData.Helpers;
using OvoData.Models.Export;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace OvoData.Forms
{
    /// <summary>
    /// Interaction logic for Export.xaml
    /// </summary>
    public partial class Export : Window
    {
        public MainWindow ParentWindow { get; set; }

        public string Account { get; set; } = string.Empty;

        private SortedDictionary<string, MonthlyData> _monthlyData = new();
        private SortedDictionary<string, DailyData> _dailyData = new();
        private SortedDictionary<string, HalfHourlyData> _halfHourlyData = new();
        private SortedDictionary<string, ReadingsData> _readingsData = new();

        private Logger _logger;

        public Export(MainWindow parent, Logger logger)
        {
            ParentWindow = parent;
            _logger = logger;
            InitializeComponent();
        }

        private void OnClick_Export(object sender, RoutedEventArgs e)
        {
            try
            {
                CursorManager.SetWaitCursorExcept();

                if (sender is Button button)
                {
                    Excel.IsEnabled = false;
                    Csv.IsEnabled = false;

                    var tag = button.Tag.ToString();

                    switch (tag)
                    {
                        case "Excel":
                            ExportAsExcel();
                            break;

                        case "CSV":
                            ExportAsCsv();
                            break;
                    }
                }

                ParentWindow.SetStatusText("Data exported", true);
                Close();
            }
            catch (Exception exception)
            {
                _logger.WriteLine(exception.ToString());
                MessageBox.Show(exception.ToString(), "Exception");
            }
            finally
            {
                CursorManager.ClearWaitCursor();
            }
        }

        private void ExportAsCsv()
        {
            var folder = ChoseFolder();
            if (!string.IsNullOrEmpty(folder))
            {
                CollectData();
                ExportToCsv(folder);
            }
        }

        private void ExportToCsv(string folder)
        {
            ParentWindow.SetStatusText("Exporting Monthly data");

            var monthlyFile = Path.Combine(folder, $"{Account} Monthly.csv");
            if (File.Exists(monthlyFile))
            {
                File.Delete(monthlyFile);
            }

            using (var writer = new StreamWriter(monthlyFile))
            {
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteRecords(_monthlyData.Values.ToList());
                }
            }

            ParentWindow.SetStatusText("Exporting Daily data");
            var dailyFile = Path.Combine(folder, $"{Account} Daily.csv");
            if (File.Exists(dailyFile))
            {
                File.Delete(dailyFile);
            }

            using (var writer = new StreamWriter(dailyFile))
            {
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteRecords(_dailyData.Values.ToList());
                }
            }

            ParentWindow.SetStatusText("Exporting Half Hourly data");
            var halfHourlyFile = Path.Combine(folder, $"{Account} Half Hourly.csv");
            if (File.Exists(halfHourlyFile))
            {
                File.Delete(halfHourlyFile);
            }

            using (var writer = new StreamWriter(halfHourlyFile))
            {
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteRecords(_halfHourlyData.Values.ToList());
                }
            }

            ParentWindow.SetStatusText("Exporting Meter Readings data");
            var meterReadingsFile = Path.Combine(folder, $"{Account} Meter Readings.csv");
            if (File.Exists(meterReadingsFile))
            {
                File.Delete(meterReadingsFile);
            }

            using (var writer = new StreamWriter(meterReadingsFile))
            {
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteRecords(_readingsData.Values.ToList());
                }
            }
        }

        private void ExportAsExcel()
        {
            var folder = ChoseFolder();
            if (!string.IsNullOrEmpty(folder))
            {
                CollectData();
                ExportToExcel(folder);
            }
        }

        private void ExportToExcel(string folder)
        {
            var file = Path.Combine(folder, $"{Account}.xlsx");

            if (File.Exists(file))
            {
                File.Delete(file);
            }

            using (var spreadsheet = new Spreadsheet(file))
            {
                var worksheetStyle = new WorksheetStyle
                {
                    ShouldFreezeHeaderRow = true,
                    ShouldAutoFitColumns = true,
                    ShouldWriteHeaderRow = true
                };

                ParentWindow.SetStatusText("Exporting Monthly data");
                using (var writer = spreadsheet.CreateWorksheetWriter<MonthlyData, MonthlyDataMap>("Monthly", worksheetStyle))
                {
                    writer.WriteRecords(_monthlyData.Values.ToList());
                }

                ParentWindow.SetStatusText("Exporting Monthly Chart data");
                var chartData = new SortedDictionary<string, MonthlyChartData>();
                foreach (var monthlyData in _monthlyData)
                {
                    var value = monthlyData.Value;
                    var year = value.Month.Split('-')[0];
                    var month = value.Month.Split('-')[1];
                    var row = new MonthlyChartData();
                    if (chartData.ContainsKey(year))
                    {
                        row = chartData[year];
                    }
                    else
                    {
                        row.Year = year;
                        chartData.Add(year, row);
                    }

                    switch (month)
                    {
                        case "01":
                            row.JanKwh = value.ElectricKwh + value.GasKwh;
                            row.JanCost = value.ElectricCost + value.GasCost;
                            break;

                        case "02":
                            row.FebKwh = value.ElectricKwh + value.GasKwh;
                            row.FebCost = value.ElectricCost + value.GasCost;
                            break;

                        case "03":
                            row.MarKwh = value.ElectricKwh + value.GasKwh;
                            row.MarCost = value.ElectricCost + value.GasCost;
                            break;

                        case "04":
                            row.AprKwh = value.ElectricKwh + value.GasKwh;
                            row.AprCost = value.ElectricCost + value.GasCost;
                            break;

                        case "05":
                            row.MayKwh = value.ElectricKwh + value.GasKwh;
                            row.MayCost = value.ElectricCost + value.GasCost;
                            break;

                        case "06":
                            row.JunKwh = value.ElectricKwh + value.GasKwh;
                            row.JunCost = value.ElectricCost + value.GasCost;
                            break;

                        case "07":
                            row.JulKwh = value.ElectricKwh + value.GasKwh;
                            row.JulCost = value.ElectricCost + value.GasCost;
                            break;

                        case "08":
                            row.AugKwh = value.ElectricKwh + value.GasKwh;
                            row.AugCost = value.ElectricCost + value.GasCost;
                            break;

                        case "09":
                            row.SepKwh = value.ElectricKwh + value.GasKwh;
                            row.SepCost = value.ElectricCost + value.GasCost;
                            break;

                        case "10":
                            row.OctKwh = value.ElectricKwh + value.GasKwh;
                            row.OctCost = value.ElectricCost + value.GasCost;
                            break;

                        case "11":
                            row.NovKwh = value.ElectricKwh + value.GasKwh;
                            row.NovCost = value.ElectricCost + value.GasCost;
                            break;

                        case "12":
                            row.DecKwh = value.ElectricKwh + value.GasKwh;
                            row.DecCost = value.ElectricCost + value.GasCost;
                            break;
                    }
                }
                using (var writer = spreadsheet.CreateWorksheetWriter<MonthlyChartData, MonthlyChartDataMap>("Monthly Chart", worksheetStyle))
                {
                    writer.WriteRecords(chartData.Values.ToList());
                }

                ParentWindow.SetStatusText("Exporting Daily data");
                using (var writer = spreadsheet.CreateWorksheetWriter<DailyData, DailyDataMap>("Daily", worksheetStyle))
                {
                    writer.WriteRecords(_dailyData.Values.ToList());
                }

                ParentWindow.SetStatusText("Exporting Half Hourly data");
                using (var writer = spreadsheet.CreateWorksheetWriter<HalfHourlyData, HalfHourlyDataMap>("Half Hourly", worksheetStyle))
                {
                    writer.WriteRecords(_halfHourlyData.Values.ToList());
                }

                ParentWindow.SetStatusText("Exporting Meter Readings data");
                using (var writer = spreadsheet.CreateWorksheetWriter<ReadingsData, ReadingsDataMap>("Meter Readings", worksheetStyle))
                {
                    writer.WriteRecords(_readingsData.Values.ToList());
                }
            }
        }

        private void CollectData()
        {
            var helper = new SqLiteHelper(Account, _logger);

            ParentWindow.SetStatusText("Fetching Monthly data");
            _monthlyData = GetMonthlyData(helper);

            ParentWindow.SetStatusText("Fetching Daily data");
            _dailyData = GetDailyData(helper);

            ParentWindow.SetStatusText("Fetching Half Hourly data");
            _halfHourlyData = GetHalfHourlyData(helper);

            ParentWindow.SetStatusText("Fetching Meter Readings Data");
            _readingsData = GetReadingsData(helper);
        }

        private SortedDictionary<string, ReadingsData> GetReadingsData(SqLiteHelper helper)
        {
            var result = new SortedDictionary<string, ReadingsData>();
            var listOfRegisters = new List<RegistersData>();

            var meterRegisters = helper.FetchMeterRegisters();
            foreach (var register in meterRegisters)
            {
                DateTime.TryParseExact(register.StartDate, Constants.ShortDateFormat,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal,
                    out var startDate);

                var endDate = DateTime.MaxValue;

                if (!string.IsNullOrEmpty(register.EndDate))
                {
                    DateTime.TryParseExact(register.EndDate, Constants.ShortDateFormat,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeUniversal,
                        out endDate);
                }

                var r = new RegistersData()
                {
                    StartDate = startDate,
                    EndDate = endDate,
                    TimingCategory = register.TimingCategory,
                    FuelType = register.FuelType,
                    UnitOfMeasurement = register.UnitOfMeasurement
                };

                listOfRegisters.Add(r);
            }

            var meterReadings = helper.FetchMeterReadings();
            foreach (var reading in meterReadings)
            {
                var readingDate = DateTime.ParseExact(reading.Date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                var data = listOfRegisters
                    .Where(register => (register.EndDate >= readingDate && register.StartDate < readingDate)
                                                                         && register.FuelType.Equals(reading.FuelType))
                    .Select(r => r.UnitOfMeasurement)
                    .FirstOrDefault();

                var readingsData = new ReadingsData
                {
                    Date = reading.Date,
                    FuelType = StringHelper.ProperCase(reading.FuelType),
                    Category = reading.TimingCategory,
                    Value = reading.Value,
                    UnitOfMeasure = data!
                };
                result.Add($"{reading.Date}-{reading.FuelType}", readingsData);
            }

            return result;
        }

        private SortedDictionary<string, HalfHourlyData> GetHalfHourlyData(SqLiteHelper helper)
        {
            var result = new SortedDictionary<string, HalfHourlyData>(new DescendingComparer<string>());

            var halfHourlies = helper.FetchHalfHourly("Electric");
            foreach (var halfHourly in halfHourlies)
            {
                var halfHourlyData = new HalfHourlyData
                {
                    StartTime = halfHourly.StartTime,
                    ElectricKwh = halfHourly.Consumption
                };
                result.Add(halfHourly.StartTime, halfHourlyData);
            }

            halfHourlies = helper.FetchHalfHourly("Gas");
            foreach (var reading in halfHourlies)
            {
                if (result.TryGetValue(reading.StartTime, out var halfHourlyData))
                {
                    halfHourlyData.GasKwh = reading.Consumption;
                }
                else
                {
                    halfHourlyData = new HalfHourlyData
                    {
                        StartTime = reading.StartTime,
                        GasKwh = reading.Consumption
                    };
                    result.Add(reading.StartTime, halfHourlyData);
                }
            }

            return result;
        }

        private SortedDictionary<string, DailyData> GetDailyData(SqLiteHelper helper)
        {
            var result = new SortedDictionary<string, DailyData>(new DescendingComparer<string>());

            var dailies = helper.FetchDaily("Electric");
            foreach (var daily in dailies)
            {
                var dailyData = new DailyData
                {
                    Day = daily.Day,
                    ElectricKwh = daily.Consumption,
                    ElectricCost = daily.Cost,
                    ElectricStanding = daily.Standing,
                    ElectricAnyTime = daily.AnyTime,
                    ElectricPeak = daily.Peak,
                    ElectricOffPeak = daily.OffPeak
                };
                result.Add(daily.Day, dailyData);
            }

            dailies = helper.FetchDaily("Gas");
            foreach (var reading in dailies)
            {
                if (result.TryGetValue(reading.Day, out var dailyData))
                {
                    dailyData.GasKwh = reading.Consumption;
                    dailyData.GasCost = reading.Cost;
                    dailyData.GasStanding = reading.Standing;
                    dailyData.GasAnyTime = reading.AnyTime;
                }
                else
                {
                    dailyData = new DailyData
                    {
                        Day = reading.Day,
                        GasKwh = reading.Consumption,
                        GasCost = reading.Cost,
                        GasStanding = reading.Standing,
                        GasAnyTime = reading.AnyTime,
                    };
                    result.Add(reading.Day, dailyData);
                }
            }

            return result;
        }

        private SortedDictionary<string, MonthlyData> GetMonthlyData(SqLiteHelper helper)
        {
            var result = new SortedDictionary<string, MonthlyData>(new DescendingComparer<string>());

            var monthlies = helper.FetchMonthly("Electric");
            foreach (var monthly in monthlies)
            {
                var monthlyData = new MonthlyData
                {
                    Month = monthly.Month,
                    ElectricKwh = monthly.Consumption,
                    ElectricCost = monthly.Cost
                };
                result.Add(monthly.Month, monthlyData);
            }

            monthlies = helper.FetchMonthly("Gas");
            foreach (var reading in monthlies)
            {
                if (result.TryGetValue(reading.Month, out var monthlyData))
                {
                    monthlyData.GasKwh = reading.Consumption;
                    monthlyData.GasCost = reading.Cost;
                }
                else
                {
                    monthlyData = new MonthlyData
                    {
                        Month = reading.Month,
                        GasKwh = reading.Consumption,
                        GasCost = reading.Cost
                    };
                    result.Add(reading.Month, monthlyData);
                }
            }

            return result;
        }

        private string ChoseFolder()
        {
            var folder = string.Empty;

            var dialog = new VistaFolderBrowserDialog
            {
                ShowNewFolderButton = true,
                Description = "Please select a folder"
            };

            var result = dialog.ShowDialog(this);

            if (result is true)
            {
                if (Directory.Exists(dialog.SelectedPath))
                {
                    folder = dialog.SelectedPath;
                }
            }

            return folder;
        }
    }
}