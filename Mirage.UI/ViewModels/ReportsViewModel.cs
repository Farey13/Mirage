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

namespace Mirage.UI.ViewModels;

public partial class ReportsViewModel : ObservableObject
{
    public static string? AuthToken { get; set; }
    private readonly IPortalMirageApi _apiClient;

    // --- NEW: Report Selection ---
    [ObservableProperty]
    private string _selectedReport = "Machine Breakdowns"; // Default to the first report

    public ObservableCollection<string> AvailableReports { get; } = new()
    {
        "Machine Breakdowns",
        "Handover Summary", // We will add the others as we build them
        "Kit Validation Log"
    };

    // --- General Report Filters ---
    [ObservableProperty] private DateTime _startDate = DateTime.Today;
    [ObservableProperty] private DateTime _endDate = DateTime.Today;

    // --- Machine Breakdown Specific Filters ---
    [ObservableProperty] private string? _selectedMachineName;
    [ObservableProperty] private string? _selectedStatus = "All";

    // --- Data & State ---
    [ObservableProperty] private bool _isLoading;
    public ObservableCollection<MachineBreakdownReportDto> MachineBreakdownReportData { get; } = new();

    // --- Filter Options ---
    public ObservableCollection<string> MachineNames { get; } = new();
    public ObservableCollection<string> StatusOptions { get; } = new() { "All", "Pending", "Resolved" };

    public ReportsViewModel()
    {
        _apiClient = RestService.For<IPortalMirageApi>("https://localhost:7210");
        if (string.IsNullOrEmpty(AuthToken)) AuthToken = UserManagementViewModel.AuthToken;

        _ = LoadFilterOptionsAsync();
    }

    private async Task LoadFilterOptionsAsync()
    {
        if (string.IsNullOrEmpty(AuthToken)) return;
        try
        {
            var machineNameItems = await _apiClient.GetListItemsByTypeAsync(AuthToken, "MachineName");

            MachineNames.Clear();
            MachineNames.Add("All");
            foreach (var item in machineNameItems.Select(i => i.ItemValue))
            {
                MachineNames.Add(item);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to load filter options: {ex.Message}", "Error");
        }
    }

    [RelayCommand]
    private async Task GenerateReport()
    {
        // This will eventually have a switch statement for different reports
        await GenerateMachineBreakdownReport();
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
            var statusFilter = SelectedStatus == "All" ? null : SelectedStatus;

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

    [RelayCommand]
    private void ClearFilters()
    {
        StartDate = DateTime.Today;
        EndDate = DateTime.Today;
        SelectedMachineName = null;
        SelectedStatus = "All";
        MachineBreakdownReportData.Clear();
    }

    [RelayCommand]
    private void PrintReport()
    {
        MessageBox.Show("Print functionality will be implemented in a future step.", "Coming Soon");
    }
}