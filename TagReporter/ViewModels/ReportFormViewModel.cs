using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TagReporter.DTOs;
using TagReporter.Models;
using TagReporter.Repositories;

namespace TagReporter.ViewModels
{
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

    public class ReportFormViewModel : ReactiveObject, INotifyPropertyChanged
    {
        private DateTime _startDateTime;
        private DateTime _endDateTime;

        private ZoneRepository _zoneRepository { get; } = new();
        private TagRepository _tagRepository { get; } = new();
        private ZoneGroupRepository _zoneGroupRepository { get; } = new();
        private ConfigRepository _configRepository { get; } = new();

        public ObservableCollection<ZoneGroup> ZoneGroups { get; } = new();
        public ObservableCollection<Zone> Zones { get; } = new();
        public ObservableCollection<Tag> Tags { get; } = new();

        private readonly StatusBarResource _statusBarResource = StatusBarResource.Instance;
        private readonly ResourceDictionary _resource = CommonResources.GetLangResourceDictionary();
        

            public List<ComboBoxItem> MeasurementDataTypes { get; } = new()
        {
            new ComboBoxItem("Temperature", "temperature"),
            new ComboBoxItem("Humidity", "cap"),
        };

        public DateTime StartDateTime
        {
            get => _startDateTime;
            set
            {
                _startDateTime = value;
                OnPropertyChanged(nameof(StartDateTime));
                OnPropertyChanged(nameof(StartTimeSpan));
            }
        }
        public TimeSpan StartTimeSpan
        {
            get => _startDateTime.TimeOfDay;
            set => StartDateTime = new DateTime(StartDateTime.Year, StartDateTime.Month, StartDateTime.Day, value.Hours, value.Minutes, value.Seconds);
        }
        public DateTime EndDateTime
        {
            get => _endDateTime;
            set
            {
                _endDateTime = value;
                OnPropertyChanged(nameof(EndDateTime));
                OnPropertyChanged(nameof(EndTimeSpan));
            }
        }
        public TimeSpan EndTimeSpan
        {
            get => _endDateTime.TimeOfDay;
            set
            {
                EndDateTime = new DateTime(EndDateTime.Year, EndDateTime.Month, EndDateTime.Day, value.Hours, value.Minutes, value.Seconds);
            }
        }

        private Zone? _selectedZone;
        public Zone? SelectedZone
        {
            get => _selectedZone;
            set
            {
                _selectedZone = value;
                this.RaiseAndSetIfChanged(ref _selectedZone, value);
            }
        }


        private ZoneGroup? _selectedZoneGroup;
        public ZoneGroup? SelectedZoneGroup
        {
            get => _selectedZoneGroup;
            set
            {
                _selectedZoneGroup = value;
                this.RaiseAndSetIfChanged(ref _selectedZoneGroup, value);
            }
        }


        private string? _selectedDataType;
        public string? SelectedDataType
        {
            get => _selectedDataType;
            set
            {
                _selectedDataType = value;
                OnPropertyChanged(nameof(SelectedDataType));
                OnPropertyChanged(nameof(IsSettingsValid));
            }
        }

        private bool _zonesSelected;
        public bool IsZonesSelected
        {
            get => _zonesSelected;
            set
            {
                _zonesSelected = value;
                OnPropertyChanged(nameof(IsZonesSelected));
                OnPropertyChanged(nameof(IsSettingsValid));
            }
        }


        public bool IsSettingsValid => !string.IsNullOrEmpty(_selectedDataType);

        public ReactiveCommand<Unit, Unit> SetMorningTimeCmd { get; }
        public ReactiveCommand<Unit, Unit> SetEveningTimeCmd { get; }
        public ReactiveCommand<Unit, Unit> SetWeeklyTimeCmd { get; }
        public ReactiveCommand<Unit, Unit> SetFriToMonTimeCmd { get; }

        public ReactiveCommand<Unit, Unit> CheckAllZonesCmd { get; }
        public ReactiveCommand<Unit, Unit> UncheckAllZonesCmd { get; }

        public ReactiveCommand<SelectionChangedEventArgs, Unit> ZoneSelectionChangedCmd { get; }
        public ReactiveCommand<Unit, Unit> CreateReportCmd { get; }
        public ReactiveCommand<Unit, Unit> ZoneGroupSelectionChangedCmd { get; }
        public ReactiveCommand<Unit, Unit> ZoneGroupDblClickCmd { get; }


        public ReportFormViewModel()
        {

            _zoneGroupRepository.FindAll().ForEach(ZoneGroups.Add);
            _zoneRepository.FindAll().ForEach(Zones.Add);

            SetMorningTimeCmd = ReactiveCommand.Create(() =>
            {
                var now = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
                StartDateTime = now.AddDays(-1).AddHours(16);
                EndDateTime = now.AddHours(9);
            });
            SetEveningTimeCmd = ReactiveCommand.Create(() =>
            {
                var now = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
                StartDateTime = now.AddHours(9);
                EndDateTime = now.AddHours(16);
            });
            SetWeeklyTimeCmd = ReactiveCommand.Create(() =>
            {
                var date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
                StartDateTime = date.AddDays(-(int)date.DayOfWeek - (int)DayOfWeek.Saturday).AddHours(9);
                EndDateTime = date.AddDays(-(int)date.DayOfWeek + 1).AddHours(9);
            });
            SetFriToMonTimeCmd = ReactiveCommand.Create(() =>
            {
                var date = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
                StartDateTime = date.AddDays(-(int)date.DayOfWeek - (int)DayOfWeek.Tuesday).AddHours(16);
                EndDateTime = date.AddDays(-(int)date.DayOfWeek + 1).AddHours(9);
            });
            CheckAllZonesCmd = ReactiveCommand.Create(() =>
            {
                foreach (var zone in Zones)
                {
                    zone.IsChecked = true;
                }

                IsZonesSelected = true;
            });
            UncheckAllZonesCmd = ReactiveCommand.Create(() =>
            {
                foreach (var zone in Zones)
                {
                    zone.IsChecked = false;
                }

                IsZonesSelected = false;
            });

            ZoneSelectionChangedCmd = ReactiveCommand.Create<SelectionChangedEventArgs>((_) =>
            {
                Tags.Clear();
                if (SelectedZone != null)
                {
                    var tags = _tagRepository.FindTagsByZone(SelectedZone);
                    tags.ForEach(Tags.Add);
                }
            });

            CreateReportCmd = ReactiveCommand.CreateFromTask(async () =>
            {
                var isValid = false;
                foreach (var zone in Zones)
                {
                    if (zone.IsChecked)
                    {
                        isValid = true;
                        break;
                    }
                }

                if (!isValid)
                {
                    MessageBox.Show($"{_resource["ZonesNotSelected"].ToString() ?? "ZonesNotSelected"}!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                _statusBarResource.StatusBarLoading = true;
                _statusBarResource.StatusBarStringContent = $"{_resource["CreatingReports"].ToString() ?? "ZonesNotSelected"}";
                if (SelectedDataType == null)
                {
                    _statusBarResource.StatusBarStringContent = $"[{DateTime.Now:G}] {_resource["UnknownDataType"].ToString() ?? "UnknownDataType"}";
                    return;
                }
                var dataType = SelectedDataType;
                await Task.Run(() =>
                {
                    if (GenerateReports(Zones, StartDateTime, EndDateTime, dataType))
                    {
                        _statusBarResource.StatusBarStringContent = $"[{DateTime.Now:G}] {_resource["ReportsCreated"].ToString() ?? "ReportsCreated"}";
                    }
                });
                _statusBarResource.StatusBarLoading = false;
            });


            ZoneGroupSelectionChangedCmd = ReactiveCommand.Create(() =>
            {
                if (SelectedZoneGroup == null) return;

                Tags.Clear();
                var zoneGroup = _zoneGroupRepository.Query().Where(zg => zg.Id == SelectedZoneGroup.Id).FirstOrDefault();
                zoneGroup.Zones.ForEach(z =>
                {
                    var tags = _tagRepository.FindTagsByZone(z);
                    tags.ForEach(Tags.Add);
                });
            });

            ZoneGroupDblClickCmd = ReactiveCommand.Create(() =>
            {
                foreach (var zone in Zones)
                {
                    zone.IsChecked = false;
                }
                var zoneGroup = _zoneGroupRepository.Query().Where(zg => SelectedZoneGroup != null && zg.Id == SelectedZoneGroup.Id).FirstOrDefault();
                zoneGroup.Zones.ForEach(z =>
                {
                    var found = Zones.FirstOrDefault(zone => zone.Uuid == z.Uuid);
                    if (found != null)
                    {
                        found.IsChecked = true;
                    }
                });
            });
        }

        public bool GenerateReports(IEnumerable<Zone> zones, DateTime startDateTime, DateTime endDateTime, string dataType)
        {
            var savePath = _configRepository.Collection().FindById("save_path").Value;
            if (savePath == null)
            {
                MessageBox.Show($"{_resource["InvalidSavePath"].ToString() ?? "InvalidSavePath"}!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            if (!Directory.Exists(savePath))
            {
                MessageBox.Show($"{_resource["InvalidSavePath"].ToString() ?? "InvalidSavePath"}!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            try
            {
                List<Task> tasks = new();
                foreach (var zone in zones)
                {
                    if (!zone.IsChecked) continue;
                    tasks.Add(Task.Factory.StartNew(() =>
                    {
                        if (!GenerateReport(zone, startDateTime, endDateTime, savePath, dataType))
                            _statusBarResource.StatusBarStringContent =
                                $"[{DateTime.Now:G}] Failed to create report for {zone.Name}.";
                        
                    }));
                }

                var taskArray = tasks.ToArray();
                Task.WaitAll(taskArray);

                var startInfo = new ProcessStartInfo()
                {
                    Arguments = savePath,
                    FileName = "explorer.exe"
                };

                Process.Start(startInfo);
            }
            catch (Exception exception)
            {
                var exceptionPathLog = @".\exception.log";
                MessageBox.Show($"{exception.Message}\n{exception}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                if (File.Exists(exceptionPathLog))
                {
                    File.Delete(exceptionPathLog);
                }

                using (var sw = new StreamWriter(exceptionPathLog))
                {
                    sw.Write(exception);
                }
                return false;
            }
            return true;
        }

        private bool GenerateReport(Zone zone, DateTime startDate, DateTime endDate, string dirName, string dataType)
        {
            zone.Tags = _tagRepository.FindTagsByZone(zone);
            if (zone.Tags.Count == 0)
            {
                _statusBarResource.StatusBarStringContent = $"[{DateTime.Now:G}] {_resource["NoTags"].ToString() ?? "NoTags"}. ({zone.Name}).";
                return false;
            }

            LoadMeasurements(zone, startDate, endDate);
            _statusBarResource.StatusBarStringContent = $"[{DateTime.Now:G}] {_resource["MeasurementsLoaded"].ToString() ?? "MeasurementsLoaded"}. ({zone.Name}).";

            GenerateXlsxReportFile(zone, startDate, endDate, dirName, dataType);
            _statusBarResource.StatusBarStringContent = $"[{DateTime.Now:G}] {_resource["XlsxCreated"].ToString() ?? "XlsxCreated"}. ({zone.Name}).";

            zone.Tags.Clear();

            return true;
        }

        private void LoadMeasurements(Zone zone, DateTime startDate, DateTime endDate)
        {
            string fromDateStr = startDate.ToString("M/d/yyyy HH:mm");
            string toDateStr = endDate.ToString("M/d/yyyy HH:mm");

            var tasks = new List<Task>();
            
            foreach (var tag in zone.Tags)
            {
                tasks.Add(Task.Factory.StartNew(() =>
                {
                    try
                    {
                        var request = new TemperatureRawDataRequest();
                        request.fromDate = fromDateStr;
                        request.toDate = toDateStr;
                        request.uuid = tag.Uuid.ToString();
                        var client = new HttpClient() {BaseAddress = CommonResources.BaseAddress};

                        var jsonRequestBody = JsonConvert.SerializeObject(request);
                        var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");
                        var response = client.PostAsync("/ethLogShared.asmx/GetTemperatureRawDataByUUID", content)
                            .Result;

                        var responseBody = response.Content.ReadAsStringAsync().Result;
                        var jsonResponse =
                            JsonConvert.DeserializeObject<DefaultWstResponse<TemperatureRawDataResponse>>(responseBody);
                        tag.Measurements = new List<Measurement>();
                        if (jsonResponse == null || jsonResponse.d == null) return;
                        foreach (var r in jsonResponse.d)
                        {
                            if (r.time == null) continue;
                            var dateTime = DateTime.Parse(r.time);
                            if (dateTime > startDate && dateTime < endDate)
                            {
                                tag.Measurements.Add(new Measurement(dateTime, Math.Round(r.temp_degC, 6), r.cap));
                            }
                        }

                        if (tag.Measurements.Count == 0) MessageBox.Show($"{tag.Name}");
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show($"{e.Message}\n; {_resource["TryUpdateTags"].ToString() ?? "TryUpdateTags"}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }));
            }
            Task.WaitAll(tasks.ToArray());
        }

        private static double GetExcelDecimalValueForDate(DateTime date)
        {
            DateTime start = new DateTime(1900, 1, 1);
            TimeSpan diff = date - start;
            return diff.TotalDays + 2;
        }

        private void GenerateXlsxReportFile(Zone zone, DateTime startDate, DateTime endDate, string dirName, string dataType)
        {
            var date = DateTime.Now.ToString("dd-MM-yyyy HH-mm");
            var path = $@"{dirName}\{zone.Name} - {date}.xlsx";


            using (var fs = File.Create(path))
            {
                using (var package = new ExcelPackage(fs))
                {
                    var chartSheet = package.Workbook.Worksheets.Add("График");
                    var tagsSheet = package.Workbook.Worksheets.Add("Теги");
                    var scatterLineChart0 = chartSheet.Drawings.AddChart("scatterLineChart0", eChartType.XYScatterSmoothNoMarkers);
                    scatterLineChart0.Title.Text = $"{zone.Name}\n{startDate:dd.MM.yyyy HH:mm:ss} - {endDate:dd.MM.yyyy HH:mm:ss}";
                    scatterLineChart0.Title.Font.Size = 18;
                    scatterLineChart0.SetSize(1024, 768);
                    scatterLineChart0.XAxis.Title.Text = "Время";
                    scatterLineChart0.XAxis.Format = "dd.MM.yyyy HH:mm:ss";
                    scatterLineChart0.XAxis.TextBody.Rotation = 270;
                    scatterLineChart0.XAxis.MinValue = GetExcelDecimalValueForDate(startDate);
                    scatterLineChart0.XAxis.MaxValue = GetExcelDecimalValueForDate(endDate);
                    switch (dataType)
                    {
                        case "temperature":
                            scatterLineChart0.YAxis.Title.Text = "Температура (ºС)";
                            break;
                        case "cap":
                            scatterLineChart0.YAxis.Title.Text = "Влажность (%)";
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(dataType), $"Unknown data type ${dataType}");
                    }
                    scatterLineChart0.YAxis.Title.Rotation = 270;
                    scatterLineChart0.YAxis.Format = "0.00";
                    scatterLineChart0.YAxis.MajorTickMark = eAxisTickMark.Out;
                    scatterLineChart0.YAxis.MinorTickMark = eAxisTickMark.None;


                    scatterLineChart0.Legend.Position = eLegendPosition.Right;
                    int dateTimePointer = 1; // A
                    int valuePointer = 2; // B

                    
                    double minValue = zone.Tags[0].Measurements.Count > 0 ? zone.Tags[0].Measurements[0].TemperatureValue : 0;
                    double maxValue = zone.Tags[0].Measurements.Count > 0 ? zone.Tags[0].Measurements[0].TemperatureValue : 30;
                    foreach (var tag in zone.Tags)
                    {
                        int rowNumber = 1;
                        tagsSheet.Cells[rowNumber, dateTimePointer].Value = "Дата Время";
                        tagsSheet.Cells[rowNumber, valuePointer].Value = tag.Name;
                        // ExcelRange rangeLabel = tagsSheet.Cells[rowNumber, dateTimePointer, rowNumber, valuePointer];

                        foreach (var m in tag.Measurements)
                        {
                            if (m.TemperatureValue < minValue) minValue = m.TemperatureValue;
                            if (m.TemperatureValue > maxValue) maxValue = m.TemperatureValue;
                            rowNumber++;
                            tagsSheet.Cells[rowNumber, dateTimePointer].Value = m.DateTime;
                            tagsSheet.Cells[rowNumber, dateTimePointer].Style.Numberformat.Format = "dd.MM.yyyy HH:mm:ss";
                            switch (dataType)
                            {
                                case "temperature":
                                    tagsSheet.Cells[rowNumber, valuePointer].Value = Math.Round(m.TemperatureValue, 6);
                                    break;
                                case "cap":
                                    tagsSheet.Cells[rowNumber, valuePointer].Value = Math.Round(m.Cap, 6);
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException(nameof(dataType), $"Unknown data type ${dataType}");
                            }
                        }

                        scatterLineChart0.YAxis.MinValue = Math.Round(minValue - 1);
                        scatterLineChart0.YAxis.MaxValue = Math.Round(maxValue + 1);

                        var serie = scatterLineChart0.Series.Add(
                            tagsSheet.Cells[2, valuePointer, rowNumber, valuePointer],
                            tagsSheet.Cells[2, dateTimePointer, rowNumber, dateTimePointer]
                            );
                        serie.HeaderAddress = tagsSheet.Cells[1, valuePointer];

                        tagsSheet.Cells[rowNumber, dateTimePointer].AutoFitColumns();
                        tagsSheet.Cells[rowNumber, valuePointer].AutoFitColumns();

                        dateTimePointer += 2;
                        valuePointer += 2;
                        tag.Measurements.Clear();
                    }


                    package.Save();
                }
            }

        }

        public new event PropertyChangedEventHandler? PropertyChanged;

        public void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler? handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
