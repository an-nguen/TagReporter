using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using TagReporter.Contracts.Repositories;
using TagReporter.Models;
using TagReporter.Repositories;
using TagReporter.Services;

namespace TagReporter.ViewModels;

public class ComboBoxItem
{
    public string Name { get; set; }
    public string Value { get; set; }

    public ComboBoxItem(string name, string value)
    {
        Name = name;
        Value = value;
    }
}

public class MainViewModel : ObservableObject
{
    private DateTime _startDateTime;
    private DateTime _endDateTime;

    private readonly IZoneRepository _zoneRepository;
    private readonly ZoneGroupRepository _zoneGroupRepository;
    private readonly ConfigRepository _configRepository;

    private readonly ReportService _reportService;
    private readonly StatusBarService _statusBarService;

    public ObservableCollection<ZoneGroup> ZoneGroups { get; } = new();
    public ObservableCollection<Zone> Zones { get; } = new();
    public ObservableCollection<Tag> Tags { get; } = new();

    public ResourceDictionaryService ResourceDictionaryService { get; set; }


    public List<ComboBoxItem> MeasurementDataTypes { get; }

    public DateTime StartDateTime
    {
        get => _startDateTime;
        set
        {
            SetProperty(ref _startDateTime, value);
            OnPropertyChanged(nameof(StartTimeSpan));
        }
    }

    public TimeSpan StartTimeSpan
    {
        get => _startDateTime.TimeOfDay;
        set => StartDateTime = new DateTime(StartDateTime.Year, StartDateTime.Month, StartDateTime.Day, value.Hours,
            value.Minutes, value.Seconds);
    }

    public DateTime EndDateTime
    {
        get => _endDateTime;
        set
        {
            SetProperty(ref _endDateTime, value);
            OnPropertyChanged(nameof(EndTimeSpan));
        }
    }

    public TimeSpan EndTimeSpan
    {
        get => _endDateTime.TimeOfDay;
        set =>
            EndDateTime = new DateTime(EndDateTime.Year, EndDateTime.Month, EndDateTime.Day, value.Hours,
                value.Minutes, value.Seconds);
    }

    private Zone? _selectedZone;

    public Zone? SelectedZone
    {
        get => _selectedZone;
        set => SetProperty(ref _selectedZone, value);
    }


    private ZoneGroup? _selectedZoneGroup;

    public ZoneGroup? SelectedZoneGroup
    {
        get => _selectedZoneGroup;
        set => SetProperty(ref _selectedZoneGroup, value);
    }


    private string? _selectedDataType;

    public string? SelectedDataType
    {
        get => _selectedDataType;
        set
        {
            SetProperty(ref _selectedDataType, value);
            OnPropertyChanged(nameof(IsSettingsValid));
        }
    }

    private bool _zonesSelected;

    public bool IsZonesSelected
    {
        get => _zonesSelected;
        set
        {
            SetProperty(ref _zonesSelected, value);
            OnPropertyChanged(nameof(IsSettingsValid));
        }
    }


    public bool IsSettingsValid => !string.IsNullOrEmpty(_selectedDataType);

    public ICommand SetMorningTimeCmd { get; }
    public ICommand SetEveningTimeCmd { get; }
    public ICommand SetWeeklyTimeCmd { get; }
    public ICommand SetFriToMonTimeCmd { get; }

    public ICommand CheckAllZonesCmd { get; }
    public ICommand UncheckAllZonesCmd { get; }

    public ICommand ZoneSelectionChangedCmd { get; }
    public ICommand CreateReportCmd { get; }
    public ICommand ZoneGroupSelectionChangedCmd { get; }
    public ICommand ZoneGroupDblClickCmd { get; }


    public MainViewModel(IZoneRepository zoneRepository,
        ReportService reportService,
        ZoneGroupRepository zoneGroupRepository,
        ConfigRepository configRepository,
        ResourceDictionaryService resourceDictionaryService,
        StatusBarService statusBarService)
    {
        _zoneRepository = zoneRepository;
        _reportService = reportService;
        _zoneGroupRepository = zoneGroupRepository;
        _configRepository = configRepository;
        ResourceDictionaryService = resourceDictionaryService;
        _zoneGroupRepository.FindAll().ForEach(ZoneGroups.Add);
        _statusBarService = statusBarService;
        UpdateZones();
        MeasurementDataTypes = new List<ComboBoxItem>
        {
            new($"{ResourceDictionaryService["Temperature"]}", "temperature"),
            new($"{ResourceDictionaryService["Humidity"]}", "cap"),
        };

        SetMorningTimeCmd = new RelayCommand(() =>
        {
            var now = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
            StartDateTime = now.AddDays(-1).AddHours(16);
            EndDateTime = now.AddHours(9);
        });
        SetEveningTimeCmd = new RelayCommand(() =>
        {
            var now = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
            StartDateTime = now.AddHours(9);
            EndDateTime = now.AddHours(16);
        });
        SetWeeklyTimeCmd = new RelayCommand(() =>
        {
            var date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
            StartDateTime = date.AddDays(-(int)date.DayOfWeek - (int)DayOfWeek.Saturday).AddHours(9);
            EndDateTime = date.AddDays(-(int)date.DayOfWeek + 1).AddHours(9);
        });
        SetFriToMonTimeCmd = new RelayCommand(() =>
        {
            var date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
            StartDateTime = date.AddDays(-(int)date.DayOfWeek - (int)DayOfWeek.Tuesday).AddHours(16);
            EndDateTime = date.AddDays(-(int)date.DayOfWeek + 1).AddHours(9);
        });
        CheckAllZonesCmd = new RelayCommand(() =>
        {
            foreach (var zone in Zones)
                zone.IsChecked = true;

            IsZonesSelected = true;
        });
        UncheckAllZonesCmd = new RelayCommand(() =>
        {
            foreach (var zone in Zones)
            {
                zone.IsChecked = false;
            }

            IsZonesSelected = false;
        });

        ZoneSelectionChangedCmd = new RelayCommand(() =>
        {
            Tags.Clear();
            if (SelectedZone == null) return;
            var tags = _zoneRepository.FindTagsByZone(SelectedZone);
            tags.ForEach(Tags.Add);
        });

        CreateReportCmd = new RelayCommand(async () =>
        {
            var isValid = false;
            foreach (var zone in Zones)
            {
                if (!zone.IsChecked) continue;
                isValid = true;
            }

            if (!isValid)
            {
                MessageBox.Show($"{ResourceDictionaryService["ZonesNotSelected"]}!", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            _statusBarService.Loading = true;
            _statusBarService.Content =
                $"{ResourceDictionaryService["CreatingReports"]}";
            if (SelectedDataType == null)
            {
                _statusBarService.Content =
                    $"[{DateTime.Now:G}] {ResourceDictionaryService["UnknownDataType"]}";
                return;
            }

            var dataType = SelectedDataType;
            await Task.Run(async () =>
            {
                if (await GenerateReports(Zones, StartDateTime, EndDateTime, dataType))
                {
                    _statusBarService.Content =
                        $"[{DateTime.Now:G}] {ResourceDictionaryService["ReportsCreated"]}";
                }
            });
            _statusBarService.Loading = false;
        });


        ZoneGroupSelectionChangedCmd = new RelayCommand(() =>
        {
            if (SelectedZoneGroup == null) return;

            Tags.Clear();
            var zoneGroup = _zoneGroupRepository.FindAll()
                .FirstOrDefault(zg => zg.Id == SelectedZoneGroup.Id);
            zoneGroup.Zones.ForEach(z =>
            {
                var tags = _zoneRepository.FindTagsByZone(z);
                tags.ForEach(Tags.Add);
            });
        });

        ZoneGroupDblClickCmd = new RelayCommand(() =>
        {
            foreach (var zone in Zones)
                zone.IsChecked = false;


            var zoneGroup = _zoneGroupRepository.FindAll()
                .Where(zg => SelectedZoneGroup != null && zg.Id == SelectedZoneGroup.Id).FirstOrDefault();
            zoneGroup.Zones.ForEach(z =>
            {
                var found = Zones.FirstOrDefault(zone => zone.Id == z.Id);
                if (found != null)
                    found.IsChecked = true;
            });
        });
    }

    private async void UpdateZones()
    {
        var zones = await _zoneRepository.FindAllAsync();
        zones.ForEach(Zones.Add);
    }

    public async Task<bool> GenerateReports(IEnumerable<Zone> zones, DateTimeOffset startDateTime, DateTimeOffset endDateTime,
        string dataType)
    {
        var savePath = _configRepository.FindAll().FirstOrDefault(c => c.Parameter == "save_path")?.Value;
        if (savePath == null)
        {
            MessageBox.Show($"{ResourceDictionaryService["InvalidSavePath"]}!", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        if (!Directory.Exists(savePath))
        {
            MessageBox.Show($"{ResourceDictionaryService["InvalidSavePath"]}!", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        var type = dataType switch
        {
            "temperature" => TagDataType.Temperature,
            "cap" => TagDataType.Humidity,
            _ => TagDataType.Temperature,
        };
        try
        {
            List<Task> tasks = new();
            var checkedZones = zones.Where(z => z.IsChecked).ToList();
            await _reportService.LoadMeasurements(checkedZones, startDateTime, endDateTime);

            foreach (var zone in checkedZones)
            {
                zone.Tags = _zoneRepository.FindTagsByZone(zone);

                tasks.Add(Task.Factory.StartNew(() =>
                {
                    _reportService.GenerateReport(zone, startDateTime, endDateTime, savePath, type);
                }));
            }

            var taskArray = tasks.ToArray();
            Task.WaitAll(taskArray);

            var startInfo = new ProcessStartInfo
            {
                Arguments = savePath,
                FileName = "explorer.exe"
            };

            Process.Start(startInfo);
        }
        catch (Exception exception)
        {
            const string? exceptionPathLog = @".\exception.log";
            MessageBox.Show($"{exception.Message}\n{exception}", "Error", MessageBoxButton.OK,
                MessageBoxImage.Error);
            if (File.Exists(exceptionPathLog))
                File.Delete(exceptionPathLog);

            using var sw = new StreamWriter(exceptionPathLog);
            sw.Write(exception);

            return false;
        }

        return true;
    }

    // private bool GenerateReport(Zone zone, DateTimeOffset startDate, DateTimeOffset endDate, string dirName, string dataType)
    // {
    //     if (zone.Tags.Count == 0)
    //     {
    //         _statusBarService.Content =
    //             $"[{DateTime.Now:G}] {ResourceDictionaryService["NoTags"]}. ({zone.Name}).";
    //         return false;
    //     }
    //
    //     LoadMeasurements(zone, startDate, endDate);
    //     _statusBarService.Content =
    //         $"[{DateTime.Now:G}] {ResourceDictionaryService["MeasurementsLoaded"]}. ({zone.Name}).";
    //
    //     GenerateXlsxReportFile(zone, startDate, endDate, dirName, dataType);
    //     _statusBarService.Content =
    //         $"[{DateTime.Now:G}] {ResourceDictionaryService["XlsxCreated"]}. ({zone.Name}).";
    //
    //     return true;
    // }

    // private void LoadMeasurements(Zone zone, DateTimeOffset startDate, DateTimeOffset endDate)
    // {
    //     string fromDateStr = startDate.ToString("M/D/yyyy HH:mm");
    //     string toDateStr = endDate.ToString("M/D/yyyy HH:mm");
    //
    //     var tasks = new List<Task>();
    //
    //     foreach (var tag in zone.Tags)
    //     {
    //         tasks.Add(Task.Factory.StartNew(() =>
    //         {
    //             try
    //             {
    //                 var request = new TemperatureRawDataRequest
    //                 {
    //                     fromDate = fromDateStr,
    //                     toDate = toDateStr,
    //                     uuid = tag.Uuid.ToString()
    //                 };
    //                 var client = new HttpClient { BaseAddress = CommonResources.BaseAddress };
    //
    //                 var jsonRequestBody = JsonConvert.SerializeObject(request);
    //                 var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");
    //                 var response = client.PostAsync("/ethLogShared.asmx/GetTemperatureRawDataByUUID", content)
    //                     .Result;
    //
    //                 var responseBody = response.Content.ReadAsStringAsync().Result;
    //                 var jsonResponse =
    //                     JsonConvert.DeserializeObject<DefaultWstResponse<TemperatureRawDataResponse>>(responseBody);
    //                 tag.Measurements = new List<Measurement>();
    //                 if (jsonResponse?.D == null) return;
    //                 foreach (var r in jsonResponse.D)
    //                 {
    //                     if (r.time == null) continue;
    //                     var dateTime = DateTime.Parse(r.time);
    //                     tag.Measurements.Add(new Measurement(dateTime, Math.Round(r.temp_degC, 6), r.cap));
    //                 }
    //                 tag.Measurements = tag.Measurements.Where(t => t.DateTime > startDate && t.DateTime < endDate).ToList();
    //
    //             }
    //             catch (Exception e)
    //             {
    //                 MessageBox.Show($"{e.Message}\n; {ResourceDictionaryService["TryUpdateTags"]}",
    //                     "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    //             }
    //         }));
    //     }
    //
    //     Task.WaitAll(tasks.ToArray());
    // }


    // private void GenerateXlsxReportFile(Zone zone, DateTimeOffset startDate, DateTimeOffset endDate, string dirName,
    //     string dataType)
    // {
    //     var date = DateTime.Now.ToString("dd-MM-yyyy HH-mm");
    //     var path = $@"{dirName}\{zone.Name} - {date}.xlsx";
    //
    //
    //     using var fs = File.Create(path);
    //     using var package = new ExcelPackage(fs);
    //     // Create excel lists for 
    //     var chartSheet = package.Workbook.Worksheets.Add($"{ResourceDictionaryService["Chart"]}");
    //     chartSheet.HeaderFooter.FirstHeader.RightAlignedText = "Ф01-СОП-03.04\nВерсия 01";
    //     var tagsSheet = package.Workbook.Worksheets.Add($"{ResourceDictionaryService["Tags"]}");
    //     // Add chart and set chart title
    //     var title = SelectedDataType switch
    //     {
    //         "temperature" => "температуры",
    //         "cap" => "влажности",
    //         _ => string.Empty
    //     };
    //     var scatterLineChart0 = CreatePredefinedScatterLineChart(chartSheet, "scatterLineChart0", 
    //         $"Журнал мониторинга {title}\n{zone.Name}\n{startDate:dd.MM.yyyy HH:mm:ss} - {endDate:dd.MM.yyyy HH:mm:ss}", SelectedDataType!);
    //         chartSheet.Drawings.AddChart("scatterLineChart0", eChartType.XYScatterSmoothNoMarkers);
    //
    //
    //
    //     double minValue, maxValue;
    //     var tagMeasurements = new List<Measurement>();
    //     zone.Tags.ForEach(t =>
    //     {
    //         tagMeasurements.AddRange(t.Measurements);
    //     });
    //
    //     switch (dataType)
    //     {
    //         case "temperature":
    //             scatterLineChart0.YAxis.Title.Text = $"{ResourceDictionaryService["Temperature"]} (ºС)";
    //             minValue = tagMeasurements.Count > 0 ? tagMeasurements.Min(m => m.TemperatureValue) - 3 : 0;
    //             maxValue = tagMeasurements.Count > 0 ? tagMeasurements.Max(m => m.TemperatureValue) + 3 : 40;
    //             break;
    //         case "cap":
    //             scatterLineChart0.YAxis.Title.Text = $"{ResourceDictionaryService["Humidity"]} (%)";
    //             minValue = tagMeasurements.Count > 0 ? tagMeasurements.Min(m => m.Cap) - 3 : 0;
    //             maxValue = tagMeasurements.Count > 0 ? tagMeasurements.Max(m => m.Cap) + 3 : 100;
    //             break;
    //         default:
    //             throw new ArgumentOutOfRangeException(nameof(dataType), $"Unknown data type ${dataType}");
    //     }
    //
    //     int dateTimePointer = 1; // A
    //     int valuePointer = 2; // B
    //
    //     // Populate second list 'Tags' with tag measurements data
    //     foreach (var tag in zone.Tags)
    //     {
    //         int rowNumber = 1;
    //         tagsSheet.Cells[rowNumber, dateTimePointer].Value = $"{ResourceDictionaryService["Date"]} {ResourceDictionaryService["Time"]}";
    //         tagsSheet.Cells[rowNumber, valuePointer].Value = tag.Name;
    //
    //         foreach (var m in tag.Measurements)
    //         {
    //             rowNumber++;
    //             // Set date time
    //             tagsSheet.Cells[rowNumber, dateTimePointer].Value = m.DateTime;
    //             tagsSheet.Cells[rowNumber, dateTimePointer].Style.Numberformat.Format =
    //                 "dd.MM.yyyy HH:mm:ss";
    //             // According to data type set value
    //             switch (dataType)
    //             {
    //                 case "temperature":
    //                     tagsSheet.Cells[rowNumber, valuePointer].Value = Math.Round(m.TemperatureValue, 6);
    //                     break;
    //                 case "cap":
    //                     tagsSheet.Cells[rowNumber, valuePointer].Value = Math.Round(m.Cap, 6);
    //                     break;
    //                 default:
    //                     throw new ArgumentOutOfRangeException(nameof(dataType),
    //                         $"Unknown data type ${dataType}");
    //             }
    //         }
    //
    //         scatterLineChart0.YAxis.MinValue = minValue;
    //         scatterLineChart0.YAxis.MaxValue = maxValue;
    //
    //         var series = scatterLineChart0.Series.Add(
    //             tagsSheet.Cells[2, valuePointer, rowNumber, valuePointer],
    //             tagsSheet.Cells[2, dateTimePointer, rowNumber, dateTimePointer]
    //         );
    //         series.HeaderAddress = tagsSheet.Cells[1, valuePointer];
    //
    //         tagsSheet.Cells[rowNumber, dateTimePointer].AutoFitColumns();
    //         tagsSheet.Cells[rowNumber, valuePointer].AutoFitColumns();
    //
    //         dateTimePointer += 2;
    //         valuePointer += 2;
    //
    //         tag.Measurements.Clear();
    //     }
    //
    //     package.Save();
    // }
}