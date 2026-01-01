using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mirage.UI.Services;
using PortalMirage.Core.Dtos;
using Refit;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Mirage.UI.ViewModels;

public record ShiftFilterItem(int Id, string Name);

public partial class ReportsViewModel : ObservableObject
{
    private readonly IPortalMirageApi _apiClient;
    private readonly IAuthService _authService;
    private readonly IPdfExportService _pdfExportService;
    private readonly DispatcherTimer _timer;

    [ObservableProperty] private string _selectedReport = "Machine Breakdowns";
    public ObservableCollection<string> AvailableReports { get; } = new() { "Machine Breakdowns", "Handover Summary", "Kit Validation Log", "Repeat Sample Log", "Daily Task Compliance", "Media Sterility Log", "Sample Storage Log", "Calibration Log" };
    [ObservableProperty] private DateTime _startDate = DateTime.Today;
    [ObservableProperty] private DateTime _endDate = DateTime.Today;
    [ObservableProperty] private bool _isLoading;

    // Machine Breakdown
    [ObservableProperty] private string? _selectedMachineName;
    [ObservableProperty] private string? _selectedBreakdownStatus = "All";
    public ObservableCollection<MachineBreakdownReportDto> MachineBreakdownReportData { get; } = new();
    public ObservableCollection<string> MachineNames { get; } = new();
    public ObservableCollection<string> BreakdownStatusOptions { get; } = new() { "All", "Pending", "Resolved" };

    // Handover
    [ObservableProperty] private string? _selectedShift;
    [ObservableProperty] private string? _selectedPriority;
    [ObservableProperty] private string? _selectedHandoverStatus = "All";
    public ObservableCollection<HandoverReportDto> HandoverReportData { get; } = new();
    public ObservableCollection<string> HandoverShiftOptions { get; } = new();
    public ObservableCollection<string> PriorityOptions { get; } = new() { "All", "Normal", "Urgent" };
    public ObservableCollection<string> HandoverStatusOptions { get; } = new() { "All", "Pending", "Received" };

    // Kit Validation
    [ObservableProperty] private string? _selectedKitName;
    [ObservableProperty] private string? _selectedKitStatus = "All";
    public ObservableCollection<KitValidationReportDto> KitValidationReportData { get; } = new();
    public ObservableCollection<string> KitNameOptions { get; } = new();
    public ObservableCollection<string> KitStatusOptions { get; } = new() { "All", "Accepted", "Rejected" };

    // Repeat Sample
    [ObservableProperty] private string? _selectedReason;
    [ObservableProperty] private string? _selectedDepartment;
    public ObservableCollection<RepeatSampleReportDto> RepeatSampleReportData { get; } = new();
    public ObservableCollection<string> ReasonOptions { get; } = new();
    public ObservableCollection<string> DepartmentOptions { get; } = new();

    // Daily Task Compliance
    [ObservableProperty] private ShiftFilterItem? _selectedTaskShift;
    [ObservableProperty] private string? _selectedTaskStatus = "All";
    public ObservableCollection<DailyTaskLogDto> DailyTaskReportData { get; } = new();
    public ObservableCollection<ShiftFilterItem> TaskShiftOptions { get; } = new();
    public ObservableCollection<string> TaskStatusOptions { get; } = new() { "All", "Pending", "Completed", "Incomplete", "Not Available" };
    [ObservableProperty] private int _totalTasks;
    [ObservableProperty] private int _completedTasks;
    // The UI 'P' format automatically multiplies by 100, so we just return the fraction (0.0 to 1.0)
    public double CompletionPercentage => TotalTasks > 0 ? (double)CompletedTasks / TotalTasks : 0;

    // Media Sterility Report Properties
    [ObservableProperty] private string? _selectedMediaName;
    [ObservableProperty] private string? _selectedMediaStatus = "All";
    public ObservableCollection<MediaSterilityReportDto> MediaSterilityReportData { get; } = new();
    public ObservableCollection<string> MediaNameOptions { get; } = new();
    public ObservableCollection<string> MediaStatusOptions { get; } = new() { "All", "Passed", "Failed" };

    // Sample Storage Report Properties
    [ObservableProperty] private string? _selectedTestName;
    [ObservableProperty] private string? _selectedSampleStatus = "All";
    public ObservableCollection<SampleStorageReportDto> SampleStorageReportData { get; } = new();
    public ObservableCollection<string> TestNameOptions { get; } = new();
    public ObservableCollection<string> SampleStatusOptions { get; } = new() { "All", "Pending", "Test Done" };

    // Calibration Log Report Properties
    [ObservableProperty] private string? _selectedCalibrationTestName;
    [ObservableProperty] private string? _selectedQcResult = "All";
    public ObservableCollection<CalibrationReportDto> CalibrationReportData { get; } = new();
    public ObservableCollection<string> CalibrationTestNameOptions { get; } = new();
    public ObservableCollection<string> QcResultOptions { get; } = new() { "All", "Passed", "Failed" };

    public ReportsViewModel(IPortalMirageApi apiClient, IAuthService authService, IPdfExportService pdfExportService)
    {
        _apiClient = apiClient;
        _authService = authService;
        _pdfExportService = pdfExportService;

        // REMOVED: _ = LoadFilterOptionsAsync();

        _timer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(1) };
        _timer.Tick += Timer_Tick;
        _timer.Start();
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        foreach (var item in MachineBreakdownReportData.Where(b => !b.IsResolved))
        {
            item.UpdateCalculatedProperties();
        }
    }

    [RelayCommand] // <-- ADDED THIS ATTRIBUTE
    private async System.Threading.Tasks.Task LoadFilterOptionsAsync()
    {
        var authToken = _authService.GetToken();
        if (string.IsNullOrEmpty(authToken)) return;

        try
        {
            var machineNameItems = await _apiClient.GetListItemsByTypeAsync(authToken, "MachineName");

            // REMOVED DEBUG LINES:
            // MessageBox.Show($"API returned {machineNameItems?.Count ?? 0} machine name items.", "Debug API Result");
            // MessageBox.Show($"MachineNames collection now has {MachineNames.Count} items.", "Debug Collection Count");

            MachineNames.Clear();
            MachineNames.Add("All"); // Ensure "All" is always first
            if (machineNameItems != null) // Add null check for safety
            {
                foreach (var item in machineNameItems)
                {
                    if (item.IsActive) // Only add active items
                    {
                        MachineNames.Add(item.ItemValue);
                    }
                }
            }

            var shiftItems = await _apiClient.GetAllShiftsAsync(authToken);
            HandoverShiftOptions.Clear();
            HandoverShiftOptions.Add("All");
            TaskShiftOptions.Clear();
            TaskShiftOptions.Add(new ShiftFilterItem(0, "All"));
            foreach (var shift in shiftItems)
            {
                HandoverShiftOptions.Add(shift.ShiftName);
                TaskShiftOptions.Add(new ShiftFilterItem(shift.ShiftID, shift.ShiftName));
            }

            PriorityOptions.Clear();
            PriorityOptions.Add("All");
            PriorityOptions.Add("Normal");
            PriorityOptions.Add("Urgent");

            var kitNameItems = await _apiClient.GetListItemsByTypeAsync(authToken, "KitName");
            KitNameOptions.Clear();
            KitNameOptions.Add("All");
            foreach (var item in kitNameItems)
                KitNameOptions.Add(item.ItemValue);

            var reasonItems = await _apiClient.GetListItemsByTypeAsync(authToken, "RepeatReason");
            ReasonOptions.Clear();
            ReasonOptions.Add("All");
            foreach (var item in reasonItems)
                ReasonOptions.Add(item.ItemValue);

            var departmentItems = await _apiClient.GetListItemsByTypeAsync(authToken, "Department");
            DepartmentOptions.Clear();
            DepartmentOptions.Add("All");
            foreach (var item in departmentItems)
                DepartmentOptions.Add(item.ItemValue);

            var mediaNameItems = await _apiClient.GetListItemsByTypeAsync(authToken, "MediaName");
            MediaNameOptions.Clear();
            MediaNameOptions.Add("All");
            foreach (var item in mediaNameItems)
                MediaNameOptions.Add(item.ItemValue);

            var testNameItems = await _apiClient.GetListItemsByTypeAsync(authToken, "TestName");
            TestNameOptions.Clear();
            TestNameOptions.Add("All");
            foreach (var item in testNameItems)
                TestNameOptions.Add(item.ItemValue);

            var calibrationTestNameItems = await _apiClient.GetListItemsByTypeAsync(authToken, "TestName");
            CalibrationTestNameOptions.Clear();
            CalibrationTestNameOptions.Add("All");
            foreach (var item in calibrationTestNameItems)
                CalibrationTestNameOptions.Add(item.ItemValue);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to load filter options: {ex.Message}");
        }
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task GenerateReport()
    {
        switch (SelectedReport)
        {
            case "Machine Breakdowns": await GenerateMachineBreakdownReport(); break;
            case "Handover Summary": await GenerateHandoverReport(); break;
            case "Kit Validation Log": await GenerateKitValidationReport(); break;
            case "Repeat Sample Log": await GenerateRepeatSampleReport(); break;
            case "Daily Task Compliance": await GenerateDailyTaskComplianceReport(); break;
            case "Media Sterility Log": await GenerateMediaSterilityReport(); break;
            case "Sample Storage Log": await GenerateSampleStorageReport(); break;
            case "Calibration Log": await GenerateCalibrationReport(); break;
        }
    }

    [RelayCommand]
    private void ClearFilters()
    {
        StartDate = DateTime.Today;
        EndDate = DateTime.Today;

        MachineBreakdownReportData.Clear();
        HandoverReportData.Clear();
        KitValidationReportData.Clear();
        RepeatSampleReportData.Clear();
        DailyTaskReportData.Clear();
        MediaSterilityReportData.Clear();
        SampleStorageReportData.Clear();
        CalibrationReportData.Clear();

        TotalTasks = 0;
        CompletedTasks = 0;
        OnPropertyChanged(nameof(CompletionPercentage));

        SelectedMachineName = "All";
        SelectedBreakdownStatus = "All";
        SelectedShift = "All";
        SelectedPriority = "All";
        SelectedHandoverStatus = "All";
        SelectedKitName = "All";
        SelectedKitStatus = "All";
        SelectedReason = "All";
        SelectedDepartment = "All";
        SelectedTaskShift = TaskShiftOptions.FirstOrDefault();
        SelectedTaskStatus = "All";
        SelectedMediaName = "All";
        SelectedMediaStatus = "All";
        SelectedTestName = "All";
        SelectedSampleStatus = "All";
        SelectedCalibrationTestName = "All";
        SelectedQcResult = "All";
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task PrintReport()
    {
        string reportTitle = SelectedReport;
        string[] headers;
        IEnumerable<object> items;

        // Use a switch to get the correct data and headers for the selected report
        switch (SelectedReport)
        {
            case "Machine Breakdowns":
                headers = new[] { "Reported On", "Machine", "Reason", "Reported By", "Is Resolved", "Resolved On", "Resolved By", "Resolution", "Downtime (Mins)", "Downtime" };
                items = MachineBreakdownReportData;
                break;

            case "Handover Summary":
                headers = new[] { "Given On", "Given By", "Shift", "Priority", "Notes", "Is Received", "Received On", "Received By" };
                items = HandoverReportData;
                break;

            case "Kit Validation Log":
                headers = new[] { "Validated On", "Kit Name", "Lot #", "Expiry", "Status", "Comments", "Validated By" };
                items = KitValidationReportData;
                break;

            case "Repeat Sample Log":
                headers = new[] { "Logged On", "Patient ID", "Patient Name", "Reason", "Department", "Informed", "Logged By" };
                items = RepeatSampleReportData;
                break;

            case "Daily Task Compliance":
                // 1. Define the headers exactly as you want them in the PDF
                headers = new[] { "Date", "Task Name", "Shift", "Status", "Completed On", "Completed By", "Comments" };

                // 2. FIX: Create a "Projection" (Select new {...}) 
                // This strips out LogID, TaskID, and formats the data cleanly.
                items = DailyTaskReportData.Select(x => new
                {
                    Date = x.LogDate.ToString("yyyy-MM-dd"), // Format Date
                    TaskName = x.TaskName,
                    Shift = x.Shift, // Use the helper property Shift (which returns ShiftName)
                    Status = x.Status,
                    CompletedOn = x.CompletedDateTime?.ToString("yyyy-MM-dd HH:mm") ?? "-", // Format DateTime or show dash
                    CompletedBy = x.CompletedByUserName,
                    Comments = x.Comments
                }).ToList();
                break;

            case "Media Sterility Log":
                headers = new[] { "Checked On", "Media Name", "Lot #", "Quantity", "Result 37C", "Result 25C", "Overall Status", "Comments", "Performed By" };
                items = MediaSterilityReportData;
                break;

            case "Sample Storage Log":
                headers = new[] { "Stored On", "Sample ID", "Test Name", "Stored By", "Is Test Done", "Completed On", "Completed By" };
                items = SampleStorageReportData;
                break;

            case "Calibration Log":
                headers = new[] { "Calibrated On", "Test/Instrument", "QC Result", "Reason", "Performed By" };
                items = CalibrationReportData;
                break;

            default:
                MessageBox.Show("Selected report is not recognized.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
        }

        if (!items.Any())
        {
            MessageBox.Show("There is no data to export.", "Export Canceled", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        await _pdfExportService.GenerateReportPdfAsync(reportTitle, headers, items);
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task ExportToCsv()
    {
        string reportTitle = SelectedReport;
        IEnumerable<object> items;

        // Use a switch to get the correct data collection for the selected report
        switch (SelectedReport)
        {
            case "Machine Breakdowns": items = MachineBreakdownReportData; break;
            case "Handover Summary": items = HandoverReportData; break;
            case "Kit Validation Log": items = KitValidationReportData; break;
            case "Repeat Sample Log": items = RepeatSampleReportData; break;
            case "Daily Task Compliance": items = DailyTaskReportData; break;
            case "Media Sterility Log": items = MediaSterilityReportData; break;
            case "Sample Storage Log": items = SampleStorageReportData; break;
            case "Calibration Log": items = CalibrationReportData; break;
            default:
                MessageBox.Show("Selected report is not recognized.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
        }

        if (!items.Any())
        {
            MessageBox.Show("There is no data to export.", "Export Canceled", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        try
        {
            var csvBuilder = new StringBuilder();

            // Get properties from the first item to create the header row
            var properties = items.First().GetType().GetProperties();
            var headers = properties.Select(p => p.Name);
            csvBuilder.AppendLine(string.Join(",", headers));

            // Add the data rows
            foreach (var item in items)
            {
                var values = properties.Select(prop =>
                {
                    var value = prop.GetValue(item);
                    // Sanitize the value for CSV
                    var stringValue = value?.ToString() ?? "";
                    if (stringValue.Contains(',') || stringValue.Contains('"'))
                    {
                        return $"\"{stringValue.Replace("\"", "\"\"")}\"";
                    }
                    return stringValue;
                });
                csvBuilder.AppendLine(string.Join(",", values));
            }

            // Create the file path and save the file
            var today = DateTime.Today;
            var directoryPath = Path.Combine("C:", "MirageReports", today.ToString("yyyy"), today.ToString("MM"), today.ToString("dd"));
            Directory.CreateDirectory(directoryPath);

            var fileName = $"{reportTitle.Replace(' ', '_')}_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
            var filePath = Path.Combine(directoryPath, fileName);

            await File.WriteAllTextAsync(filePath, csvBuilder.ToString());

            MessageBox.Show($"Successfully exported {items.Count()} records to:\n{filePath}", "Export Successful", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"An error occurred during export: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async System.Threading.Tasks.Task GenerateMachineBreakdownReport()
    {
        var authToken = _authService.GetToken();
        if (string.IsNullOrEmpty(authToken)) return;

        if (StartDate > EndDate)
        {
            MessageBox.Show("The start date cannot be after the end date.", "Invalid Date Range", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        IsLoading = true;
        MachineBreakdownReportData.Clear();
        try
        {
            var machineNameFilter = SelectedMachineName == "All" ? null : SelectedMachineName;
            var statusFilter = SelectedBreakdownStatus == "All" ? null : SelectedBreakdownStatus;
            var reportData = await _apiClient.GetMachineBreakdownReportAsync(authToken, StartDate, EndDate, machineNameFilter, statusFilter);
            foreach (var item in reportData)
            {
                MachineBreakdownReportData.Add(item);
            }

            if (!MachineBreakdownReportData.Any())
            {
                MessageBox.Show("No records found for the selected criteria.", "Report Generated", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to generate report: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async System.Threading.Tasks.Task GenerateHandoverReport()
    {
        var authToken = _authService.GetToken();
        if (string.IsNullOrEmpty(authToken)) return;

        if (StartDate > EndDate)
        {
            MessageBox.Show("Start date cannot be after end date.", "Invalid Date Range");
            return;
        }

        IsLoading = true;
        HandoverReportData.Clear();
        try
        {
            var shiftFilter = SelectedShift == "All" ? null : SelectedShift;
            var priorityFilter = SelectedPriority == "All" ? null : SelectedPriority;
            var statusFilter = SelectedHandoverStatus == "All" ? null : SelectedHandoverStatus;
            var reportData = await _apiClient.GetHandoverReportAsync(authToken, StartDate, EndDate, shiftFilter, priorityFilter, statusFilter);
            foreach (var item in reportData) HandoverReportData.Add(item);
            if (!HandoverReportData.Any())
            {
                MessageBox.Show("No records found for the selected criteria.", "Report Generated");
            }
        }
        catch (Exception ex) { MessageBox.Show($"Failed to generate report: {ex.Message}"); }
        finally { IsLoading = false; }
    }

    private async System.Threading.Tasks.Task GenerateKitValidationReport()
    {
        var authToken = _authService.GetToken();
        if (string.IsNullOrEmpty(authToken)) return;

        if (StartDate > EndDate)
        {
            MessageBox.Show("Start date cannot be after end date.", "Invalid Date Range");
            return;
        }

        IsLoading = true;
        KitValidationReportData.Clear();
        try
        {
            var kitNameFilter = SelectedKitName == "All" ? null : SelectedKitName;
            var statusFilter = SelectedKitStatus == "All" ? null : SelectedKitStatus;
            var reportData = await _apiClient.GetKitValidationReportAsync(authToken, StartDate, EndDate, kitNameFilter, statusFilter);
            foreach (var item in reportData) KitValidationReportData.Add(item);
            if (!KitValidationReportData.Any())
            {
                MessageBox.Show("No records found for the selected criteria.", "Report Generated");
            }
        }
        catch (Exception ex) { MessageBox.Show($"Failed to generate report: {ex.Message}"); }
        finally { IsLoading = false; }
    }

    private async System.Threading.Tasks.Task GenerateRepeatSampleReport()
    {
        var authToken = _authService.GetToken();
        if (string.IsNullOrEmpty(authToken)) return;

        if (StartDate > EndDate)
        {
            MessageBox.Show("Start date cannot be after end date.", "Invalid Date Range");
            return;
        }

        IsLoading = true;
        RepeatSampleReportData.Clear();
        try
        {
            var reasonFilter = SelectedReason == "All" ? null : SelectedReason;
            var departmentFilter = SelectedDepartment == "All" ? null : SelectedDepartment;
            var reportData = await _apiClient.GetRepeatSampleReportAsync(authToken, StartDate, EndDate, reasonFilter, departmentFilter);
            foreach (var item in reportData) RepeatSampleReportData.Add(item);
            if (!RepeatSampleReportData.Any())
            {
                MessageBox.Show("No records found for the selected criteria.", "Report Generated");
            }
        }
        catch (Exception ex) { MessageBox.Show($"Failed to generate report: {ex.Message}"); }
        finally { IsLoading = false; }
    }

    private async System.Threading.Tasks.Task GenerateDailyTaskComplianceReport()
    {
        var authToken = _authService.GetToken();
        if (string.IsNullOrEmpty(authToken)) return;

        if (StartDate > EndDate) { MessageBox.Show("Start date cannot be after end date."); return; }

        IsLoading = true;
        DailyTaskReportData.Clear();
        TotalTasks = 0;
        CompletedTasks = 0;
        try
        {
            var shiftIdFilter = SelectedTaskShift?.Id == 0 ? (int?)null : SelectedTaskShift?.Id;
            var statusFilter = SelectedTaskStatus == "All" ? null : SelectedTaskStatus;
            var reportData = await _apiClient.GetDailyTaskComplianceReportAsync(authToken, StartDate, EndDate, shiftIdFilter, statusFilter);
            foreach (var item in reportData.Items) DailyTaskReportData.Add(item);
            TotalTasks = reportData.TotalTasks;
            CompletedTasks = reportData.CompletedTasks;
            OnPropertyChanged(nameof(CompletionPercentage));

            if (!DailyTaskReportData.Any()) { MessageBox.Show("No records found for the selected criteria."); }
        }
        catch (Exception ex) { MessageBox.Show($"Failed to generate report: {ex.Message}"); }
        finally { IsLoading = false; }
    }

    private async System.Threading.Tasks.Task GenerateMediaSterilityReport()
    {
        var authToken = _authService.GetToken();
        if (string.IsNullOrEmpty(authToken)) return;

        if (StartDate > EndDate) { MessageBox.Show("Start date cannot be after end date."); return; }

        IsLoading = true;
        MediaSterilityReportData.Clear();
        try
        {
            var mediaNameFilter = SelectedMediaName == "All" ? null : SelectedMediaName;
            var statusFilter = SelectedMediaStatus == "All" ? null : SelectedMediaStatus;
            var reportData = await _apiClient.GetMediaSterilityReportAsync(authToken, StartDate, EndDate, mediaNameFilter, statusFilter);
            foreach (var item in reportData) MediaSterilityReportData.Add(item);
            if (!MediaSterilityReportData.Any()) { MessageBox.Show("No records found for the selected criteria."); }
        }
        catch (Exception ex) { MessageBox.Show($"Failed to generate report: {ex.Message}"); }
        finally { IsLoading = false; }
    }

    private async System.Threading.Tasks.Task GenerateSampleStorageReport()
    {
        var authToken = _authService.GetToken();
        if (string.IsNullOrEmpty(authToken)) return;

        if (StartDate > EndDate) { MessageBox.Show("Start date cannot be after end date."); return; }

        IsLoading = true;
        SampleStorageReportData.Clear();
        try
        {
            var testNameFilter = SelectedTestName == "All" ? null : SelectedTestName;
            var statusFilter = SelectedSampleStatus == "All" ? null : SelectedSampleStatus;
            var reportData = await _apiClient.GetSampleStorageReportAsync(authToken, StartDate, EndDate, testNameFilter, statusFilter);
            foreach (var item in reportData) SampleStorageReportData.Add(item);
            if (!SampleStorageReportData.Any()) { MessageBox.Show("No records found for the selected criteria."); }
        }
        catch (Exception ex) { MessageBox.Show($"Failed to generate report: {ex.Message}"); }
        finally { IsLoading = false; }
    }

    private async System.Threading.Tasks.Task GenerateCalibrationReport()
    {
        var authToken = _authService.GetToken();
        if (string.IsNullOrEmpty(authToken)) return;

        if (StartDate > EndDate) { MessageBox.Show("Start date cannot be after end date."); return; }

        IsLoading = true;
        CalibrationReportData.Clear();
        try
        {
            var testNameFilter = SelectedCalibrationTestName == "All" ? null : SelectedCalibrationTestName;
            var qcResultFilter = SelectedQcResult == "All" ? null : SelectedQcResult;
            var reportData = await _apiClient.GetCalibrationReportAsync(authToken, StartDate, EndDate, testNameFilter, qcResultFilter);
            foreach (var item in reportData) CalibrationReportData.Add(item);
            if (!CalibrationReportData.Any()) { MessageBox.Show("No records found for the selected criteria."); }
        }
        catch (Exception ex) { MessageBox.Show($"Failed to generate report: {ex.Message}"); }
        finally { IsLoading = false; }
    }
}