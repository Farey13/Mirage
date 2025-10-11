using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mirage.UI.Services;
using PortalMirage.Core.Dtos;
using Refit;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace Mirage.UI.ViewModels;

public partial class CalibrationLogViewModel : ObservableObject
{
    // 1. Services are now provided by the constructor
    private readonly IPortalMirageApi _apiClient;
    private readonly IAuthService _authService;

    // All [ObservableProperty] fields remain the same
    [ObservableProperty]
    private string? _selectedTestName;
    [ObservableProperty]
    private string? _selectedQcResult;
    [ObservableProperty]
    private string? _reason;
    [ObservableProperty]
    private DateTime _startDate = DateTime.Today;
    [ObservableProperty]
    private DateTime _endDate = DateTime.Today;
    [ObservableProperty]
    private bool _isDeleteFlyoutOpen;
    [ObservableProperty]
    private CalibrationLogResponse? _selectedLogToDelete;
    [ObservableProperty]
    private string _deactivationReason = string.Empty;

    public ObservableCollection<CalibrationLogResponse> Logs { get; } = new();
    public ObservableCollection<string> TestNames { get; } = new();
    public ObservableCollection<string> QcResults { get; } = new();

    // 2. The constructor now accepts the services
    public CalibrationLogViewModel(IPortalMirageApi apiClient, IAuthService authService)
    {
        _apiClient = apiClient;
        _authService = authService;

        TestNames.Add("Vitros 250");
        TestNames.Add("Abbott Architect");
        TestNames.Add("Glucose Meter QC");
        QcResults.Add("Passed");
        QcResults.Add("Failed");

        // We still call this to preserve the auto-load functionality
        LoadLogsCommand.Execute(null);
    }

    [RelayCommand]
    private async Task LoadLogs()
    {
        // 3. Get the token from the service before every API call
        var authToken = _authService.GetToken();
        if (string.IsNullOrEmpty(authToken)) return;

        try
        {
            var logs = await _apiClient.GetCalibrationLogsAsync(authToken, StartDate, EndDate);
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
        var authToken = _authService.GetToken();
        if (string.IsNullOrEmpty(authToken) || string.IsNullOrEmpty(SelectedTestName) || string.IsNullOrEmpty(SelectedQcResult))
        {
            MessageBox.Show("Test Name and QC Result are required.");
            return;
        }

        try
        {
            var request = new CreateCalibrationLogRequest(SelectedTestName, SelectedQcResult, Reason);
            await _apiClient.CreateCalibrationLogAsync(authToken, request);
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

        var authToken = _authService.GetToken();
        if (string.IsNullOrEmpty(authToken)) return;

        try
        {
            var request = new DeactivateCalibrationLogRequest(DeactivationReason);
            await _apiClient.DeactivateCalibrationLogAsync(authToken, SelectedLogToDelete.CalibrationID, request);

            IsDeleteFlyoutOpen = false;
            await LoadLogs();
        }
        // This important error handling is preserved
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