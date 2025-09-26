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

public partial class CalibrationLogViewModel : ObservableObject
{
    // This will hold our token for API calls
    // In a real app, this would be stored more securely.
    public static string? AuthToken { get; set; }

    private readonly IPortalMirageApi _apiClient;

    // Properties for the data entry form
    [ObservableProperty]
    private string? _selectedTestName;

    [ObservableProperty]
    private string? _selectedQcResult;

    [ObservableProperty]
    private string? _reason;

    // Properties for the search and data grid
    [ObservableProperty]
    private DateTime _startDate = DateTime.Today;

    [ObservableProperty]
    private DateTime _endDate = DateTime.Today;

    public ObservableCollection<CalibrationLogResponse> Logs { get; } = new();

    // Options for the ComboBoxes
    public ObservableCollection<string> TestNames { get; } = new();
    public ObservableCollection<string> QcResults { get; } = new();

    // Properties for the "Deactivate" Flyout
    [ObservableProperty]
    private bool _isDeleteFlyoutOpen;

    [ObservableProperty]
    private CalibrationLogResponse? _selectedLogToDelete;

    [ObservableProperty]
    private string _deactivationReason = string.Empty;

    public CalibrationLogViewModel()
    {
        _apiClient = RestService.For<IPortalMirageApi>("https://localhost:7210");

        // Populate the dropdown lists with options
        // Later, this could come from the API
        TestNames.Add("Vitros 250");
        TestNames.Add("Abbott Architect");
        TestNames.Add("Glucose Meter QC");

        QcResults.Add("Passed");
        QcResults.Add("Failed");

        // Load today's logs automatically
        LoadLogsCommand.Execute(null);
    }

    [RelayCommand]
    private async Task LoadLogs()
    {
        if (string.IsNullOrEmpty(AuthToken)) return;
        try
        {
            var logs = await _apiClient.GetCalibrationLogsAsync(AuthToken, StartDate, EndDate);
            Logs.Clear();
            foreach (var log in logs)
            {
                Logs.Add(log);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to load logs: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task Save()
    {
        if (string.IsNullOrEmpty(AuthToken) || string.IsNullOrEmpty(SelectedTestName) || string.IsNullOrEmpty(SelectedQcResult))
        {
            MessageBox.Show("Test Name and QC Result are required.");
            return;
        }

        try
        {
            var request = new CreateCalibrationLogRequest(SelectedTestName, SelectedQcResult, Reason);
            await _apiClient.CreateCalibrationLogAsync(AuthToken, request);

            // Success! Clear the form and reload the grid
            Clear();
            await LoadLogs();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to save log: {ex.Message}");
        }
    }

    [RelayCommand]
    private void Clear()
    {
        SelectedTestName = null;
        SelectedQcResult = null;
        Reason = string.Empty;
    }

    [RelayCommand]
    private void ShowDeleteFlyout(CalibrationLogResponse log)
    {
        SelectedLogToDelete = log;
        DeactivationReason = string.Empty;
        IsDeleteFlyoutOpen = true;
    }

    [RelayCommand]
    private async Task ConfirmDeactivation()
    {
        if (SelectedLogToDelete is null || string.IsNullOrWhiteSpace(DeactivationReason))
        {
            MessageBox.Show("A reason is required for deactivation.");
            return;
        }
        if (string.IsNullOrEmpty(AuthToken)) return;
        try
        {
            var request = new DeactivateCalibrationLogRequest(DeactivationReason);
            await _apiClient.DeactivateCalibrationLogAsync(AuthToken, SelectedLogToDelete.CalibrationID, request);

            IsDeleteFlyoutOpen = false;
            await LoadLogs(); // Refresh the list
        }
        catch (ApiException ex)
        {
            if (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                MessageBox.Show("You do not have permission to perform this action. Please contact an administrator.", "Authorization Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                MessageBox.Show($"An error occurred communicating with the server: {ex.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"An error occurred: {ex.Message}");
        }
    }
}