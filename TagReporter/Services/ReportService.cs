using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OfficeOpenXml;
using OfficeOpenXml.Drawing.Chart;
using TagReporter.Contracts.Repositories;
using TagReporter.DTOs;
using TagReporter.Models;
using TagReporter.Repositories;

namespace TagReporter.Services;

public class ReportService
{
    private readonly IZoneRepository _zoneRepository;
    private readonly ResourceDictionaryService _resourceDictionaryService;
    private readonly StatusBarService _statusBarService;
    private readonly ILogger _logger;
    public ReportService(IZoneRepository zoneRepository, ResourceDictionaryService resourceDictionaryService, StatusBarService statusBarService, ILogger<ReportService> logger)
    {
        _zoneRepository = zoneRepository;
        _resourceDictionaryService = resourceDictionaryService;
        _statusBarService = statusBarService;
        _logger = logger;
    }


    // Load measurement from cloud server.
    // Parameters: 
    // - zones - List of zones (TagUuids should be populated)
    public async Task LoadMeasurements(List<Zone> zones, DateTimeOffset startDate, DateTimeOffset endDate)
    {
        foreach (var zone in zones)
        {
            zone.Tags = _zoneRepository.FindTagsByZone(zone);
            if (zone.Tags.Count == 0)
                _statusBarService.Content =
                    $"[{DateTime.Now:G}] {_resourceDictionaryService["NoTags"]}. ({zone.Name}).";

            await DownloadMeasurements(zone, startDate, endDate);
            _statusBarService.Content =
                $"[{DateTime.Now:G}] {_resourceDictionaryService["MeasurementsLoaded"]}. ({zone.Name}).";
        }
    }

    public string GenerateReport(Zone zone, DateTimeOffset startDate, DateTimeOffset endDate, string dirPath, TagDataType dataType)
    {
        var filepath = GenerateXlsxReportFile(zone, startDate, endDate, dirPath, dataType);
        _statusBarService.Content =
            $"[{DateTime.Now:G}] {_resourceDictionaryService["XlsxCreated"]}. ({zone.Name}).";

        return filepath;
    }

    private async Task DownloadMeasurements(Zone zone, DateTimeOffset startDate, DateTimeOffset endDate)
    {
        var fromDateStr = startDate.ToString("M/d/yyyy HH:mm");
        var toDateStr = endDate.ToString("M/d/yyyy HH:mm");

        var client = new HttpClient { BaseAddress = CommonResources.BaseAddress };
        client.BaseAddress = CommonResources.BaseAddress;

        foreach (var tag in zone.Tags)
        {
            var request = new TemperatureRawDataRequest
            {
                fromDate = fromDateStr,
                toDate = toDateStr,
                uuid = tag.Uuid.ToString()
            };

            var jsonRequestBody = JsonConvert.SerializeObject(request);
            var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("/ethLogShared.asmx/GetTemperatureRawDataByUUID", content);

            var responseBody = await response.Content.ReadAsStringAsync();
            if (response.StatusCode != HttpStatusCode.OK)
            {
                MessageBox.Show($"[DownloadMeasurements] statusCode = {response.StatusCode}\n{response.Content}", "Error");
                _logger.LogError("[DownloadMeasurements] statusCode = {}\n{}", response.StatusCode, response.Content);
                continue;
            }

            var jsonResponse =
                JsonConvert.DeserializeObject<DefaultWstResponse<TemperatureRawDataResponse>>(responseBody);
            tag.Measurements = new List<Measurement>();
            if (jsonResponse?.D == null) return;
            foreach (var r in jsonResponse.D)
            {
                if (r.time == null) continue;
                var dateTime = DateTimeOffset.Parse(r.time);
                tag.Measurements.Add(new Measurement(dateTime, Math.Round(r.temp_degC, 6), r.cap));
            }

            tag.Measurements = tag.Measurements.Where(t => t.DateTime > startDate && t.DateTime < endDate).ToList();

            if (tag.Measurements.Count == 0)
            {
                MessageBox.Show(
                    $"{_resourceDictionaryService["Tag"]}: {tag.Name}.\n{_resourceDictionaryService["Period"]}: {startDate:G}-{endDate:G}\n{_resourceDictionaryService["NoMeasurements"]}",
                    "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
                _logger.LogError("{}: {} {}-{} {}", _resourceDictionaryService["Tag"], tag.Name, startDate, endDate, 
                    _resourceDictionaryService["NoMeasurements"]);
            }
        }
    }


    private static ExcelChart CreatePredefinedScatterLineChart(ExcelWorksheet worksheet, string chartName,
        string title, TagDataType dataType)
    {
        var chart =
            worksheet.Drawings.AddChart(chartName, eChartType.XYScatterSmoothNoMarkers);

        chart.Title.Text = title;
        chart.Title.Font.Size = 18;
        chart.SetPosition(1, 10, 0, 32);
        // Default column size is 64px, row size 20px
        chart.SetSize(896, 580);

        chart.YAxis.Title.Text = dataType switch
        {
            TagDataType.Temperature => "Температура (ºС)",
            TagDataType.Humidity => "Влажность (%)",
            _ => "Неизвестный тип данных"
        };
        chart.XAxis.Title.Text = "Время";
        chart.XAxis.Format = "dd.MM.yyyy HH:mm:ss";
        chart.XAxis.TextBody.Rotation = 270;

        chart.YAxis.Title.Rotation = 270;
        chart.YAxis.Format = "0.00";
        chart.YAxis.MajorTickMark = eAxisTickMark.Out;
        chart.YAxis.MinorTickMark = eAxisTickMark.None;

        chart.Legend.Position = eLegendPosition.Right;
        return chart;
    }
    private static void SetMinMaxTimeAxis(ExcelChart chart, DateTimeOffset startDate, DateTimeOffset endDate)
    {
        chart.XAxis.MinValue = startDate.DateTime.ToOADate();
        chart.XAxis.MaxValue = endDate.DateTime.ToOADate();
    }

    private static void SetMinMaxYAxis(ExcelChart chart, List<Measurement> measurements, TagDataType dataType)
    {
        double minValue, maxValue;

        switch (dataType)
        {
            case TagDataType.Temperature:
                minValue = measurements.Count > 0 ? measurements.Min(m => m.TemperatureValue) - 3 : 0;
                maxValue = measurements.Count > 0 ? measurements.Max(m => m.TemperatureValue) + 3 : 40;
                break;
            case TagDataType.Humidity:
                minValue = measurements.Count > 0 ? measurements.Min(m => m.Cap) - 3 : 0;
                maxValue = measurements.Count > 0 ? measurements.Max(m => m.Cap) + 3 : 100;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(dataType), $"Unknown data type ${dataType}");
        }

        chart.YAxis.MinValue = minValue;
        chart.YAxis.MaxValue = maxValue;
    }

    private static string GenerateXlsxReportFile(Zone zone, DateTimeOffset startDate, DateTimeOffset endDate,
       string dirPath, TagDataType dataType)
    {
        var date = DateTime.Now.ToString("dd-MM-yyyy HH-mm");
        var filename = $@"{zone.Name}_{date}.xlsx";

        using var fs = File.Create($@"{dirPath}\{filename}");
        using var package = new ExcelPackage(fs);
        var tagMeasurements = new List<Measurement>();
        zone.Tags.ForEach(t => { tagMeasurements.AddRange(t.Measurements); });
        switch (dataType)
        {
            case TagDataType.Temperature:
                /* 1. Create sheets */
                // Create sheets with chart 
                var tempChartSheet = package.Workbook.Worksheets.Add("График");
                tempChartSheet.HeaderFooter.FirstHeader.RightAlignedText = "Ф01-СОП-03.04\nВерсия 01";
                /* 2. Create charts */
                // Create temperature chart
                var tempChart =
                    CreatePredefinedScatterLineChart(tempChartSheet, "scatterLineChart0",
                        $"Журнал мониторинга температуры\n{zone.Name}\n{startDate:dd.MM.yyyy HH:mm:ss} - {endDate:dd.MM.yyyy HH:mm:ss}",
                        TagDataType.Temperature);
                // Create sheets with measurements
                var tempMSheet = package.Workbook.Worksheets.Add($"Данные измерении (Температуры)");
                // Specify min and max values of Time axis
                SetMinMaxTimeAxis(tempChart, startDate, endDate);
                // Specify min and max values of Temperature/Humidity axis
                SetMinMaxYAxis(tempChart, tagMeasurements, TagDataType.Temperature);
                PopulateMSheet(zone, TagDataType.Temperature, tempMSheet, tempChart);
                break;
            case TagDataType.Humidity:
                var humidityChartSheet = package.Workbook.Worksheets.Add("График");
                humidityChartSheet.HeaderFooter.FirstHeader.RightAlignedText = "Ф01-СОП-03.04\nВерсия 01";
                var humidityMSheet = package.Workbook.Worksheets.Add($"Данные измерении (Влажности)");
                // Create humidity chart
                var humidityChart =
                    CreatePredefinedScatterLineChart(humidityChartSheet, "scatterLineChart0",
                        $"Журнал мониторинга влажности\n{zone.Name}\n{startDate:dd.MM.yyyy HH:mm:ss} - {endDate:dd.MM.yyyy HH:mm:ss}",
                        TagDataType.Humidity);
                SetMinMaxTimeAxis(humidityChart, startDate, endDate);
                SetMinMaxYAxis(humidityChart, tagMeasurements, TagDataType.Humidity);
                PopulateMSheet(zone, TagDataType.Humidity, humidityMSheet, humidityChart);
                break;
        }

        // Create sheet of list of tags info
        var tagSheet = package.Workbook.Worksheets.Add("Теги");
        // Populate sheet of measurements
        PopulateTagSheet(tagSheet, zone.Tags);

        package.Save();

        zone.Tags.ForEach(t => t.Measurements.Clear());
        return Path.Combine(dirPath, filename);
    }

    private static void PopulateMSheet(Zone zone, TagDataType dataType, ExcelWorksheet sheet, ExcelChart chart)
    {
        var dateTimePointer = 1; // A
        var valuePointer = 2; // B

        foreach (var tag in zone.Tags)
        {
            var rowNumber = 1;
            sheet.Cells[rowNumber, dateTimePointer].Value = "Дата Время";
            sheet.Cells[rowNumber, valuePointer].Value = tag.Name;

            foreach (var m in tag.Measurements)
            {
                rowNumber++;
                // Set date time
                sheet.Cells[rowNumber, dateTimePointer].Value = m.DateTime.DateTime;
                sheet.Cells[rowNumber, dateTimePointer].Style.Numberformat.Format =
                    "dd.MM.yyyy HH:mm:ss";
                // According to data type set value
                switch (dataType)
                {
                    case TagDataType.Temperature:
                        sheet.Cells[rowNumber, valuePointer].Value = Math.Round(m.TemperatureValue, 6);
                        break;
                    case TagDataType.Humidity:
                        sheet.Cells[rowNumber, valuePointer].Value = Math.Round(m.Cap, 6);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(dataType),
                            $"Unknown data type ${dataType}");
                }
            }

            var series = chart.Series.Add(
                sheet.Cells[2, valuePointer, rowNumber, valuePointer],
                sheet.Cells[2, dateTimePointer, rowNumber, dateTimePointer]
            );
            series.HeaderAddress = sheet.Cells[1, valuePointer];

            sheet.Cells[rowNumber, dateTimePointer].AutoFitColumns();
            sheet.Cells[rowNumber, valuePointer].AutoFitColumns();

            dateTimePointer += 2;
            valuePointer += 2;
        }
    }

    private static void PopulateTagSheet(ExcelWorksheet sheet, List<Tag> tags)
    {
        List<string> columns = new()
        {
            "Аккаунт",
            "Менеджер",
            "MAC",
            "Тег",
        };
        // Set columns
        var rowPtr = 1;
        var colPtr = 1;

        columns.ForEach(c =>
        {
            sheet.Cells[rowPtr, colPtr].Value = c;
            colPtr++;
        });
        rowPtr++;
        tags.ForEach(t =>
        {
            if (t.Account != null)
            {
                sheet.Cells[rowPtr, 1].Value = t.Account.Email;
                sheet.Cells[rowPtr, 1].AutoFitColumns();
            }

            sheet.Cells[rowPtr, 2].Value = t.TagManagerName;
            sheet.Cells[rowPtr, 2].AutoFitColumns();
            sheet.Cells[rowPtr, 3].Value = t.TagManagerMac;
            sheet.Cells[rowPtr, 3].AutoFitColumns();
            sheet.Cells[rowPtr, 4].Value = t.Name;
            sheet.Cells[rowPtr, 4].AutoFitColumns();

            rowPtr++;
        });
    }
}