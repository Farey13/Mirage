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
    public ObservableCollection<string> AvailableReports { get; } = new() { "Machine Breakdowns", "Handover Summary", "Kit Validation Log" };

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

    // --- NEW: Kit Validation Report Properties ---
    [ObservableProperty] private string? _selectedKitName;
    [ObservableProperty] private string? _selectedKitStatus = "All";
    public ObservableCollection<KitValidationReportDto> KitValidationReportData { get; } = new();
    public ObservableCollection<string> KitNameOptions { get; } = new();
    public ObservableCollection<string> KitStatusOptions { get; } = new() { "All", "Accepted", "Rejected" };

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
            var machineNameItems = await _apiClient.GetListItemsByTypeAsync(AuthToken, "MachineName");
            MachineNames.Clear();
            MachineNames.Add("All");
            foreach (var item in machineNameItems) MachineNames.Add(item.ItemValue);

            var shiftItems = await _apiClient.GetAllShiftsAsync(AuthToken);
            ShiftOptions.Clear();
            ShiftOptions.Add("All");
            foreach (var shift in shiftItems) ShiftOptions.Add(shift.ShiftName);

            PriorityOptions.Clear();
            PriorityOptions.Add("All");
            PriorityOptions.Add("Normal");
            PriorityOptions.Add("Urgent");

            // NEW: Load Kit Name options
            var kitNameItems = await _apiClient.GetListItemsByTypeAsync(AuthToken, "KitName");
            KitNameOptions.Clear();
            KitNameOptions.Add("All");
            foreach (var item in kitNameItems) KitNameOptions.Add(item.ItemValue);
        }
        catch (Exception ex) { MessageBox.Show($"Failed to load filter options: {ex.Message}"); }
    }

    [RelayCommand]
    private async Task GenerateReport()
    {
        switch (SelectedReport)
        {
            case "Machine Breakdowns":
                await GenerateMachineBreakdownReport();
                break;
            case "Handover Summary":
                await GenerateHandoverReport();
                break;
            case "Kit Validation Log":
                await GenerateKitValidationReport();
                break;
        }
    }

    [RelayCommand]
    private void ClearFilters()
    {
        StartDate = DateTime.Today;
        EndDate = DateTime.Today;
        // Clear all filter types
        SelectedMachineName = null;
        SelectedBreakdownStatus = "All";
        SelectedShift = null;
        SelectedPriority = null;
        SelectedHandoverStatus = "All";
        SelectedKitName = null;
        SelectedKitStatus = "All";
        // Clear all data grids
        MachineBreakdownReportData.Clear();
        HandoverReportData.Clear();
        KitValidationReportData.Clear();
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

    // NEW: Method for Kit Validation Report
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

    [RelayCommand]
    private void PrintReport()
    {
        MessageBox.Show("Print functionality will be implemented in a future step.", "Coming Soon");
    }
}