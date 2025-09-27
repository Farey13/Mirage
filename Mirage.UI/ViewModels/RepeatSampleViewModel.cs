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
    public static string? AuthToken { get; set; }
    private readonly IPortalMirageApi _mirageApiClient;
    private readonly IPatientInfoApi _patientInfoApiClient;

    // Form properties
    [ObservableProperty] private string _patientIdCardNumber = string.Empty;
    [ObservableProperty] private string _patientName = string.Empty;
    [ObservableProperty] private string? _selectedReason;
    [ObservableProperty] private string? _informedPerson;
    [ObservableProperty] private string? _selectedDepartment;

    // Search and Data Grid
    [ObservableProperty] private DateTime _startDate = DateTime.Today;
    [ObservableProperty] private DateTime _endDate = DateTime.Today;
    public ObservableCollection<RepeatSampleResponse> Logs { get; } = new();

    // Deactivation Flyout properties
    [ObservableProperty] private bool _isDeleteFlyoutOpen;
    [ObservableProperty] private RepeatSampleResponse? _selectedLogToDelete;
    [ObservableProperty] private string _deactivationReason = string.Empty;

    // Dropdown options
    public ObservableCollection<string> Reasons { get; } = new();
    public ObservableCollection<string> Departments { get; } = new() { "OPD", "IPD" };

    public RepeatSampleViewModel()
    {
        _mirageApiClient = RestService.For<IPortalMirageApi>("https://localhost:7210");
        _patientInfoApiClient = RestService.For<IPatientInfoApi>("https://localhost:7124"); // NOTE: Port may be different

        // This list will be loaded from the API in the Admin section later
        Reasons.Add("Hemolysis");
        Reasons.Add("Clotted");
        Reasons.Add("Insufficient Quantity");
        Reasons.Add("Instrument Error");

        LoadLogsCommand.Execute(null);
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
        catch (Exception ex) { PatientName = "Error finding patient"; }
    }

    [RelayCommand]
    private async Task LoadLogs()
    {
        if (string.IsNullOrEmpty(AuthToken)) return;
        try
        {
            var logs = await _mirageApiClient.GetRepeatSamplesAsync(AuthToken, StartDate, EndDate);
            Logs.Clear();
            foreach (var log in logs) Logs.Add(log);
        }
        catch (Exception ex) { MessageBox.Show($"Failed to load logs: {ex.Message}"); }
    }

    [RelayCommand]
    private async Task Save()
    {
        if (string.IsNullOrEmpty(AuthToken) || string.IsNullOrEmpty(PatientIdCardNumber) || string.IsNullOrEmpty(PatientName) || PatientName == "Patient Not Found")
        {
            MessageBox.Show("A valid Patient ID and Name are required.");
            return;
        }
        try
        {
            var request = new CreateRepeatSampleRequest(PatientIdCardNumber, PatientName, SelectedReason, InformedPerson, SelectedDepartment);
            await _mirageApiClient.CreateRepeatSampleAsync(AuthToken, request);
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
        OnPropertyChanged(nameof(SelectedReason));
        OnPropertyChanged(nameof(SelectedDepartment));
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
        if (string.IsNullOrEmpty(AuthToken)) return;
        try
        {
            var request = new DeactivateRepeatSampleRequest(DeactivationReason);
            await _mirageApiClient.DeactivateRepeatSampleAsync(AuthToken, SelectedLogToDelete.RepeatID, request);

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