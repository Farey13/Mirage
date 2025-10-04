using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mirage.UI.Services;
using PortalMirage.Core.Dtos;
using Refit;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Mirage.UI.ViewModels;

public partial class ReportsViewModel : ObservableObject
{
    public static string? AuthToken { get; set; }
    private readonly IPortalMirageApi _apiClient;
    private readonly DispatcherTimer _timer;

    [ObservableProperty] private string _selectedReport = "Machine Breakdowns";
    public ObservableCollection<string> AvailableReports { get; } = new() { "Machine Breakdowns", "Handover Summary", "Kit Validation Log", "Repeat Sample Log" };

    [ObservableProperty] private DateTime _startDate = DateTime.Today;
    [ObservableProperty] private DateTime _endDate = DateTime.Today;
    [ObservableProperty] private bool _isLoading;

    // --- Machine Breakdown Properties ---
    [ObservableProperty] private string? _selectedMachineName;
    [ObservableProperty] private string? _selectedBreakdownStatus = "All";
    public ObservableCollection<MachineBreakdownReportDto> MachineBreakdownReportData { get; } = new();
    public ObservableCollection<string> MachineNames { get; } = new();
    public ObservableCollection<string> BreakdownStatusOptions { get; } = new() { "All", "Pending", "Resolved" };

    // --- Handover Report Properties ---
    [ObservableProperty] private string? _selectedShift;
    [ObservableProperty] private string? _selectedPriority;
    [ObservableProperty] private string? _selectedHandoverStatus = "All";
    public ObservableCollection<HandoverReportDto> HandoverReportData { get; } = new();
    public ObservableCollection<string> ShiftOptions { get; } = new();
    public ObservableCollection<string> PriorityOptions { get; } = new();
    public ObservableCollection<string> HandoverStatusOptions { get; } = new() { "All", "Pending", "Received" };

    // --- Kit Validation Report Properties ---
    [ObservableProperty] private string? _selectedKitName;
    [ObservableProperty] private string? _selectedKitStatus = "All";
    public ObservableCollection<KitValidationReportDto> KitValidationReportData { get; } = new();
    public ObservableCollection<string> KitNameOptions { get; } = new();
    public ObservableCollection<string> KitStatusOptions { get; } = new() { "All", "Accepted", "Rejected" };

    // --- NEW: Repeat Sample Report Properties ---
    [ObservableProperty] private string? _selectedReason;
    [ObservableProperty] private string? _selectedDepartment;
    public ObservableCollection<RepeatSampleReportDto> RepeatSampleReportData { get; } = new();
    public ObservableCollection<string> ReasonOptions { get; } = new();
    public ObservableCollection<string> DepartmentOptions { get; } = new();


    public ReportsViewModel()
    {
        _apiClient = RestService.For<IPortalMirageApi>("https://localhost:7210");
        if (string.IsNullOrEmpty(AuthToken)) AuthToken = UserManagementViewModel.AuthToken;

        _ = LoadFilterOptionsAsync();

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

    private async Task LoadFilterOptionsAsync()
    {
        if (string.IsNullOrEmpty(AuthToken)) return;
        try
        {
            // Machine Names
            var machineNameItems = await _apiClient.GetListItemsByTypeAsync(AuthToken, "MachineName");
            MachineNames.Clear(); MachineNames.Add("All");
            foreach (var item in machineNameItems) MachineNames.Add(item.ItemValue);

            // Shifts
            var shiftItems = await _apiClient.GetAllShiftsAsync(AuthToken);
            ShiftOptions.Clear(); ShiftOptions.Add("All");
            foreach (var shift in shiftItems) ShiftOptions.Add(shift.ShiftName);

            // Priorities
            PriorityOptions.Clear(); PriorityOptions.Add("All"); PriorityOptions.Add("Normal"); PriorityOptions.Add("Urgent");

            // Kit Names
            var kitNameItems = await _apiClient.GetListItemsByTypeAsync(AuthToken, "KitName");
            KitNameOptions.Clear(); KitNameOptions.Add("All");
            foreach (var item in kitNameItems) KitNameOptions.Add(item.ItemValue);

            // NEW: Repeat Sample Filters
            var reasonItems = await _apiClient.GetListItemsByTypeAsync(AuthToken, "RepeatReason");
            ReasonOptions.Clear(); ReasonOptions.Add("All");
            foreach (var item in reasonItems) ReasonOptions.Add(item.ItemValue);

            var departmentItems = await _apiClient.GetListItemsByTypeAsync(AuthToken, "Department");
            DepartmentOptions.Clear(); DepartmentOptions.Add("All");
            foreach (var item in departmentItems) DepartmentOptions.Add(item.ItemValue);
        }
        catch (Exception ex) { MessageBox.Show($"Failed to load filter options: {ex.Message}"); }
    }

    [RelayCommand]
    private async Task GenerateReport()
    {
        switch (SelectedReport)
        {
            case "Machine Breakdowns": await GenerateMachineBreakdownReport(); break;
            case "Handover Summary": await GenerateHandoverReport(); break;
            case "Kit Validation Log": await GenerateKitValidationReport(); break;
            case "Repeat Sample Log": await GenerateRepeatSampleReport(); break;
        }
    }

    [RelayCommand]
    private void ClearFilters()
    {
        StartDate = DateTime.Today;
        EndDate = DateTime.Today;
        // Clear all filters and data
        SelectedMachineName = null; SelectedBreakdownStatus = "All"; MachineBreakdownReportData.Clear();
        SelectedShift = null; SelectedPriority = null; SelectedHandoverStatus = "All"; HandoverReportData.Clear();
        SelectedKitName = null; SelectedKitStatus = "All"; KitValidationReportData.Clear();
        SelectedReason = null; SelectedDepartment = null; RepeatSampleReportData.Clear();
    }

    private async Task GenerateMachineBreakdownReport()
    {
        if (string.IsNullOrEmpty(AuthToken)) return;

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

            var reportData = await _apiClient.GetMachineBreakdownReportAsync(AuthToken, StartDate, EndDate, machineNameFilter, statusFilter);
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

    private async Task GenerateHandoverReport()
    {
        if (string.IsNullOrEmpty(AuthToken)) return;
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

            var reportData = await _apiClient.GetHandoverReportAsync(AuthToken, StartDate, EndDate, shiftFilter, priorityFilter, statusFilter);
            foreach (var item in reportData) HandoverReportData.Add(item);

            if (!HandoverReportData.Any())
            {
                MessageBox.Show("No records found for the selected criteria.", "Report Generated");
            }
        }
        catch (Exception ex) { MessageBox.Show($"Failed to generate report: {ex.Message}"); }
        finally { IsLoading = false; }
    }

    private async Task GenerateKitValidationReport()
    {
        if (string.IsNullOrEmpty(AuthToken)) return;
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

            var reportData = await _apiClient.GetKitValidationReportAsync(AuthToken, StartDate, EndDate, kitNameFilter, statusFilter);
            foreach (var item in reportData) KitValidationReportData.Add(item);

            if (!KitValidationReportData.Any())
            {
                MessageBox.Show("No records found for the selected criteria.", "Report Generated");
            }
        }
        catch (Exception ex) { MessageBox.Show($"Failed to generate report: {ex.Message}"); }
        finally { IsLoading = false; }
    }

    // NEW: Method for Repeat Sample Report
    private async Task GenerateRepeatSampleReport()
    {
        if (string.IsNullOrEmpty(AuthToken)) return;
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

            var reportData = await _apiClient.GetRepeatSampleReportAsync(AuthToken, StartDate, EndDate, reasonFilter, departmentFilter);
            foreach (var item in reportData) RepeatSampleReportData.Add(item);

            if (!RepeatSampleReportData.Any())
            {
                MessageBox.Show("No records found for the selected criteria.", "Report Generated");
            }
        }
        catch (Exception ex) { MessageBox.Show($"Failed to generate report: {ex.Message}"); }
        finally { IsLoading = false; }
    }

    [RelayCommand]
    private void PrintReport()
    {
        MessageBox.Show("Print functionality will be implemented in a future step.", "Coming Soon");
    }
}