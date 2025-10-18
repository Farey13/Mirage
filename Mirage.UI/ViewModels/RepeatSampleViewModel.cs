using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mirage.UI.Services;
using PatientInfo.Api.Sdk;
using PatientInfo.Api.Sdk.Models;
using PortalMirage.Core.Dtos;
using Refit;
using System;
using System.Collections.ObjectModel;
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

    public ObservableCollection<RepeatSampleResponse> Logs { get; } = new();
    public ObservableCollection<string> Reasons { get; } = new();
    public ObservableCollection<string> Departments { get; } = new() { "OPD", "IPD" };

    public RepeatSampleViewModel(IPortalMirageApi mirageApiClient, IPatientInfoApi patientInfoApiClient, IAuthService authService)
    {
        _mirageApiClient = mirageApiClient;
        _patientInfoApiClient = patientInfoApiClient;
        _authService = authService;

        Reasons.Add("Hemolysis");
        Reasons.Add("Clotted");
        Reasons.Add("Insufficient Quantity");
        Reasons.Add("Instrument Error");
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task FindPatient()
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
    private async System.Threading.Tasks.Task LoadLogs()
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
    private async System.Threading.Tasks.Task Save()
    {
        var authToken = _authService.GetToken();
        if (string.IsNullOrEmpty(authToken) || string.IsNullOrEmpty(PatientIdCardNumber) || string.IsNullOrEmpty(PatientName) || PatientName == "Patient Not Found")
        {
            MessageBox.Show("A valid Patient ID and Name are required.");
            return;
        }
        try
        {
            var request = new CreateRepeatSampleRequest(PatientIdCardNumber, PatientName, SelectedReason, InformedPerson, SelectedDepartment);
            await _mirageApiClient.CreateRepeatSampleAsync(authToken, request);
            Clear();
            await LoadLogs();
        }
        catch (Exception ex) { MessageBox.Show($"Failed to save log: {ex.Message}"); }
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
    private void ShowDeleteFlyout(RepeatSampleResponse log)
    {
        SelectedLogToDelete = log;
        DeactivationReason = string.Empty;
        IsDeleteFlyoutOpen = true;
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task ConfirmDeactivation()
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