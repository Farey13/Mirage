using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mirage.UI.Services;
using PatientInfo.Api.Sdk;
using PatientInfo.Api.Sdk.Models;
using PortalMirage.Core.Dtos;
using Refit;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace Mirage.UI.ViewModels;

public partial class RepeatSampleViewModel : ObservableObject
{
    private readonly IPortalMirageApi _mirageApiClient;
    private readonly IPatientInfoApi _patientInfoApiClient;
    private readonly IAuthService _authService;

    [ObservableProperty] private string _patientIdCardNumber = string.Empty;
    [ObservableProperty] private string _patientName = string.Empty;
    [ObservableProperty] private string? _selectedReason;
    [ObservableProperty] private string? _informedPerson;
    [ObservableProperty] private string? _selectedDepartment;
    [ObservableProperty] private DateTime _startDate = DateTime.Today;
    [ObservableProperty] private DateTime _endDate = DateTime.Today;
    [ObservableProperty] private bool _isDeleteFlyoutOpen;
    [ObservableProperty] private RepeatSampleResponse? _selectedLogToDelete;
    [ObservableProperty] private string _deactivationReason = string.Empty;

    // DRAFT CONFIGURATION
    private const string DraftFileName = "draft_repeatsample.json";
    [ObservableProperty] private bool _hasUnsavedDraft;

    public ObservableCollection<RepeatSampleResponse> Logs { get; } = new();
    public ObservableCollection<string> Reasons { get; } = new();
    public ObservableCollection<string> Departments { get; } = new();

    public RepeatSampleViewModel(IPortalMirageApi mirageApiClient, IPatientInfoApi patientInfoApiClient, IAuthService authService)
    {
        _mirageApiClient = mirageApiClient;
        _patientInfoApiClient = patientInfoApiClient;
        _authService = authService;

        // REMOVED: Hardcoded reasons and departments

        // ADDED: Load master lists from API
        _ = LoadMasterLists();

        // CHECK FOR DRAFT ON STARTUP
        if (File.Exists(DraftFileName))
        {
            try
            {
                var json = File.ReadAllText(DraftFileName);
                // Verify 'CreateRepeatSampleRequest' matches your actual DTO name
                var draft = JsonSerializer.Deserialize<CreateRepeatSampleRequest>(json);

                if (draft != null)
                {
                    // Map the draft back to your form properties
                    PatientIdCardNumber = draft.PatientIdCardNumber;
                    PatientName = draft.PatientName;
                    SelectedReason = draft.ReasonText; // Note: Your DTO uses ReasonText, not SelectedReason
                    InformedPerson = draft.InformedPerson;
                    SelectedDepartment = draft.Department;

                    HasUnsavedDraft = true;
                    MessageBox.Show("We found an unsaved repeat sample request and restored it.", "Draft Restored", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch
            {
                try { File.Delete(DraftFileName); }
                catch { }
            }
        }
    }

    // ADDED: Method to load master lists from API
    public async Task LoadMasterLists()
    {
        var token = _authService.GetToken();
        if (string.IsNullOrEmpty(token)) return;

        try
        {
            var reasonItems = await _mirageApiClient.GetListItemsByTypeAsync(token, "RepeatReason");
            Reasons.Clear();
            foreach (var item in reasonItems.Where(i => i.IsActive)) Reasons.Add(item.ItemValue);

            var deptItems = await _mirageApiClient.GetListItemsByTypeAsync(token, "Department");
            Departments.Clear();
            foreach (var item in deptItems.Where(i => i.IsActive)) Departments.Add(item.ItemValue);
        }
        catch (Exception) { }
    }

    [RelayCommand]
    private async Task FindPatient()
    {
        if (string.IsNullOrWhiteSpace(PatientIdCardNumber)) return;

        try
        {
            var nationalId = new NationalId(PatientIdCardNumber);
            var patient = await _patientInfoApiClient.GetByNationalIdAsync(nationalId);
            PatientName = patient?.PatientName ?? "Patient Not Found";
        }
        catch (Exception)
        {
            PatientName = "Error finding patient";
        }
    }

    [RelayCommand]
    private async Task LoadLogs()
    {
        var authToken = _authService.GetToken();
        if (string.IsNullOrEmpty(authToken)) return;

        try
        {
            var logs = await _mirageApiClient.GetRepeatSamplesAsync(authToken, StartDate, EndDate);
            Logs.Clear();
            foreach (var log in logs) Logs.Add(log);
        }
        catch (Exception ex) { MessageBox.Show($"Failed to load logs: {ex.Message}"); }
    }

    [RelayCommand]
    private async Task Save()
    {
        var authToken = _authService.GetToken();

        // Validate required fields based on your DTO
        if (string.IsNullOrEmpty(authToken) || string.IsNullOrEmpty(PatientIdCardNumber) || string.IsNullOrEmpty(PatientName) || PatientName == "Patient Not Found")
        {
            MessageBox.Show("A valid Patient ID and Name are required.");
            return;
        }

        // Create Request (Ensure order matches your DTO constructor)
        var request = new CreateRepeatSampleRequest(
            PatientIdCardNumber,
            PatientName,
            SelectedReason,  // Note: Your DTO constructor expects string? reasonText
            InformedPerson,
            SelectedDepartment
        );

        try
        {
            // 1. Try API
            await _mirageApiClient.CreateRepeatSampleAsync(authToken, request);

            // 2. Success - Clear Form
            Clear();
            if (File.Exists(DraftFileName)) File.Delete(DraftFileName);
            HasUnsavedDraft = false;

            await LoadLogs(); // Refresh list
            MessageBox.Show("Repeat sample request saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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
                    "This request has been saved as a draft.\n" +
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
        PatientIdCardNumber = string.Empty;
        PatientName = string.Empty;
        SelectedReason = null;
        InformedPerson = string.Empty;
        SelectedDepartment = null;
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
    private void ShowDeleteFlyout(RepeatSampleResponse log)
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
            var request = new DeactivateRepeatSampleRequest(DeactivationReason);
            await _mirageApiClient.DeactivateRepeatSampleAsync(authToken, SelectedLogToDelete.RepeatID, request);
            IsDeleteFlyoutOpen = false;
            await LoadLogs();
        }
        catch (ApiException ex)
        {
            if (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                MessageBox.Show("You do not have permission to perform this action. Please contact an administrator.", "Authorization Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else { MessageBox.Show($"An error occurred communicating with the server: {ex.StatusCode}"); }
        }
        catch (Exception ex) { MessageBox.Show($"Failed to deactivate entry: {ex.Message}"); }
    }
}