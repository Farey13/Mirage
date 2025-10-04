using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mirage.UI.Services;
using PortalMirage.Core.Dtos;
using PortalMirage.Core.Models; // Add this for AdminListItem
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

    [ObservableProperty] private DateTime _startDate = DateTime.Today;
    [ObservableProperty] private DateTime _endDate = DateTime.Today;
    [ObservableProperty] private string? _selectedMachineName;
    [ObservableProperty] private string? _selectedStatus = "All"; // Default to "All"

    [ObservableProperty] private bool _isLoading;
    public ObservableCollection<MachineBreakdownReportDto> MachineBreakdownReportData { get; } = new();

    public ObservableCollection<string> MachineNames { get; } = new();
    public ObservableCollection<string> StatusOptions { get; } = new() { "All", "Pending", "Resolved" };

    public ReportsViewModel()
    {
        _apiClient = RestService.For<IPortalMirageApi>("https://localhost:7210");
        if (string.IsNullOrEmpty(AuthToken)) AuthToken = UserManagementViewModel.AuthToken;

        // Call the new async method to load dropdown options from the API
        _ = LoadFilterOptionsAsync();
    }

    private async System.Threading.Tasks.Task LoadFilterOptionsAsync()
    {
        if (string.IsNullOrEmpty(AuthToken)) return;
        try
        {
            // Fetch the list of machine names from the database
            var machineNameItems = await _apiClient.GetListItemsByTypeAsync(AuthToken, "MachineName");

            MachineNames.Clear();
            MachineNames.Add("All"); // Add the "All" option for filtering
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
    private async System.Threading.Tasks.Task GenerateMachineBreakdownReport()
    {
        if (string.IsNullOrEmpty(AuthToken)) return;

        // Added validation for the date range
        if (StartDate > EndDate)
        {
            MessageBox.Show("The start date cannot be after the end date.", "Invalid Date Range", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        IsLoading = true;
        MachineBreakdownReportData.Clear();
        try
        {
            // Handle "All" filter option by sending null to the API
            var machineNameFilter = SelectedMachineName == "All" ? null : SelectedMachineName;
            var statusFilter = SelectedStatus == "All" ? null : SelectedStatus;

            var reportData = await _apiClient.GetMachineBreakdownReportAsync(AuthToken, StartDate, EndDate, machineNameFilter, statusFilter);
            foreach (var item in reportData)
            {
                MachineBreakdownReportData.Add(item);
            }

            // Added better user feedback
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
        // Now resets dates as well
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