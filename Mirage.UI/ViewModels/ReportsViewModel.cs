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
    public ObservableCollection<string> AvailableReports { get; } = new() { "Machine Breakdowns", "Handover Summary" };

    [ObservableProperty] private DateTime _startDate = DateTime.Today;
    [ObservableProperty] private DateTime _endDate = DateTime.Today;
    [ObservableProperty] private bool _isLoading;

    // --- Machine Breakdown Properties ---
    [ObservableProperty] private string? _selectedMachineName;
    [ObservableProperty] private string? _selectedBreakdownStatus = "All";
    public ObservableCollection<MachineBreakdownReportDto> MachineBreakdownReportData { get; } = new();
    public ObservableCollection<string> MachineNames { get; } = new();
    public ObservableCollection<string> BreakdownStatusOptions { get; } = new() { "All", "Pending", "Resolved" };

    // --- NEW: Handover Report Properties ---
    [ObservableProperty] private string? _selectedShift;
    [ObservableProperty] private string? _selectedPriority;
    [ObservableProperty] private string? _selectedHandoverStatus = "All";
    public ObservableCollection<HandoverReportDto> HandoverReportData { get; } = new();
    public ObservableCollection<string> ShiftOptions { get; } = new();
    public ObservableCollection<string> PriorityOptions { get; } = new();
    public ObservableCollection<string> HandoverStatusOptions { get; } = new() { "All", "Pending", "Received" };

    public ReportsViewModel()
    {
        _apiClient = RestService.For<IPortalMirageApi>("https://localhost:7210");
        if (string.IsNullOrEmpty(AuthToken)) AuthToken = UserManagementViewModel.AuthToken;

        _ = LoadFilterOptionsAsync();

        // Initialize and start the timer
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMinutes(1)
        };
        _timer.Tick += Timer_Tick;
        _timer.Start();
    }

    private void Timer_Tick(object? sender, EventArgs e)
    {
        // Every minute, loop through the unresolved items and call the public update method.
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
            foreach (var item in machineNameItems.Select(i => i.ItemValue)) MachineNames.Add(item);

            // NEW: Load Handover filter options
            var shiftItems = await _apiClient.GetAllShiftsAsync(AuthToken);
            ShiftOptions.Clear();
            ShiftOptions.Add("All");
            foreach (var shift in shiftItems) ShiftOptions.Add(shift.ShiftName);

            PriorityOptions.Clear();
            PriorityOptions.Add("All");
            PriorityOptions.Add("Normal");
            PriorityOptions.Add("Urgent");
        }
        catch (Exception ex) { MessageBox.Show($"Failed to load filter options: {ex.Message}"); }
    }

    [RelayCommand]
    private async Task GenerateReport()
    {
        // NEW: Switch to call the correct method based on selection
        switch (SelectedReport)
        {
            case "Machine Breakdowns":
                await GenerateMachineBreakdownReport();
                break;
            case "Handover Summary":
                await GenerateHandoverReport();
                break;
        }
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

    // NEW: Method to generate the Handover Report
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

    [RelayCommand]
    private void ClearFilters()
    {
        StartDate = DateTime.Today;
        EndDate = DateTime.Today;
        SelectedMachineName = null;
        SelectedBreakdownStatus = "All";
        SelectedShift = null;
        SelectedPriority = null;
        SelectedHandoverStatus = "All";
        MachineBreakdownReportData.Clear();
        HandoverReportData.Clear();
    }

    [RelayCommand]
    private void PrintReport()
    {
        MessageBox.Show("Print functionality will be implemented in a future step.", "Coming Soon");
    }
}