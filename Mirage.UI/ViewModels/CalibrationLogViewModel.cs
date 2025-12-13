using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mirage.UI.Services;
using PortalMirage.Core.Dtos;
using Refit;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
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

    // DRAFT CONFIGURATION
    private const string DraftFileName = "draft_calibration.json";
    [ObservableProperty] private bool _hasUnsavedDraft;

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

        // CHECK FOR DRAFT ON STARTUP
        if (File.Exists(DraftFileName))
        {
            try
            {
                var json = File.ReadAllText(DraftFileName);
                // Verify this matches your actual DTO name (e.g., CreateCalibrationRequest)
                var draft = JsonSerializer.Deserialize<CreateCalibrationLogRequest>(json);

                if (draft != null)
                {
                    // Map the draft back to your form properties
                    // UPDATE THESE NAMES to match your actual variables!
                    SelectedTestName = draft.TestName;
                    SelectedQcResult = draft.QcResult;
                    Reason = draft.Reason;

                    HasUnsavedDraft = true;
                    MessageBox.Show("We found an unsaved calibration record and restored it.", "Draft Restored", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch
            {
                try { File.Delete(DraftFileName); }
                catch { }
            }
        }
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

        // Validate required fields
        if (string.IsNullOrEmpty(authToken) || string.IsNullOrEmpty(SelectedTestName) || string.IsNullOrEmpty(SelectedQcResult))
        {
            MessageBox.Show("Test Name and QC Result are required.");
            return;
        }

        // Create the Request Object
        var request = new CreateCalibrationLogRequest(SelectedTestName, SelectedQcResult, Reason);

        try
        {
            // 1. Try API
            await _apiClient.CreateCalibrationLogAsync(authToken, request);

            // 2. Success: Clear Form & Delete Draft
            Clear();
            if (File.Exists(DraftFileName)) File.Delete(DraftFileName);
            HasUnsavedDraft = false;

            await LoadLogs(); // Refresh the list
            MessageBox.Show("Calibration record saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            // 3. Failure: Save Draft
            try
            {
                var json = JsonSerializer.Serialize(request);
                await File.WriteAllTextAsync(DraftFileName, json);
                HasUnsavedDraft = true;

                MessageBox.Show(
                    $"Connection failed: {ex.Message}\n\n" +
                    "This calibration record has been saved as a draft.\n" +
                    "Please submit it again when the connection is restored.",
                    "Network Error - Draft Saved",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            catch (Exception fileEx)
            {
                MessageBox.Show($"Critical Error: Could not save draft.\n{fileEx.Message}", "Error");
            }
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
    private async Task ClearDraft()
    {
        try
        {
            if (File.Exists(DraftFileName))
            {
                File.Delete(DraftFileName);
                Clear();
                HasUnsavedDraft = false;
                MessageBox.Show("Draft cleared successfully.", "Draft Cleared", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to clear draft: {ex.Message}", "Error");
        }
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