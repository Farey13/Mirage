using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mirage.UI.Services;
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

public partial class SampleStorageViewModel : ObservableObject
{
    private readonly IPortalMirageApi _apiClient;
    private readonly IAuthService _authService;

    [ObservableProperty] private string _newPatientSampleId = string.Empty;
    [ObservableProperty] private string _newTestName = string.Empty;
    [ObservableProperty] private DateTime _startDate = DateTime.Today;
    [ObservableProperty] private DateTime _endDate = DateTime.Today;

    // DRAFT CONFIGURATION
    private const string DraftFileName = "draft_samplestorage.json";
    [ObservableProperty] private bool _hasUnsavedDraft;

    private string _activeView = "Pending";
    public bool IsPendingViewActive
    {
        get => _activeView == "Pending";
        set
        {
            if (value && _activeView != "Pending")
            {
                _activeView = "Pending";
                OnPropertyChanged(nameof(IsPendingViewActive));
                OnPropertyChanged(nameof(IsCompletedViewActive));
                SearchCommand.Execute(null);
            }
        }
    }
    public bool IsCompletedViewActive
    {
        get => _activeView == "Completed";
        set
        {
            if (value && _activeView != "Completed")
            {
                _activeView = "Completed";
                OnPropertyChanged(nameof(IsPendingViewActive));
                OnPropertyChanged(nameof(IsCompletedViewActive));
                SearchCommand.Execute(null);
            }
        }
    }

    [ObservableProperty] private bool _isDeleteFlyoutOpen;
    [ObservableProperty] private SampleStorageResponse? _selectedSampleToDelete;
    [ObservableProperty] private string _deactivationReason = string.Empty;

    public ObservableCollection<SampleStorageResponse> PendingSamples { get; } = new();
    public ObservableCollection<SampleStorageResponse> CompletedSamples { get; } = new();

    public SampleStorageViewModel(IPortalMirageApi apiClient, IAuthService authService)
    {
        _apiClient = apiClient;
        _authService = authService;

        // CHECK FOR DRAFT ON STARTUP
        if (File.Exists(DraftFileName))
        {
            try
            {
                var json = File.ReadAllText(DraftFileName);
                var draft = JsonSerializer.Deserialize<CreateSampleStorageRequest>(json);

                if (draft != null)
                {
                    // Map the draft back to your form properties
                    NewPatientSampleId = draft.PatientSampleID;
                    NewTestName = draft.TestName;

                    HasUnsavedDraft = true;
                    MessageBox.Show("We found an unsaved sample entry and restored it.", "Draft Restored", MessageBoxButton.OK, MessageBoxImage.Information);
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
    private async Task Search()
    {
        var authToken = _authService.GetToken();
        if (string.IsNullOrEmpty(authToken)) return;

        if (_activeView == "Pending")
        {
            try
            {
                var samples = await _apiClient.GetPendingSamplesAsync(authToken, StartDate, EndDate);
                PendingSamples.Clear();
                foreach (var sample in samples) PendingSamples.Add(sample);
            }
            catch (Exception ex) { MessageBox.Show($"Failed to load pending samples: {ex.Message}"); }
        }
        else // Completed
        {
            try
            {
                var samples = await _apiClient.GetCompletedSamplesAsync(authToken, StartDate, EndDate);
                CompletedSamples.Clear();
                foreach (var sample in samples) CompletedSamples.Add(sample);
            }
            catch (Exception ex) { MessageBox.Show($"Failed to load completed samples: {ex.Message}"); }
        }
    }

    [RelayCommand]
    private async Task Add()
    {
        var authToken = _authService.GetToken();
        if (string.IsNullOrEmpty(authToken) || string.IsNullOrEmpty(NewPatientSampleId) || string.IsNullOrEmpty(NewTestName))
        {
            MessageBox.Show("Patient Sample ID and Test Name are required.");
            return;
        }

        // Create the Request Object
        var request = new CreateSampleStorageRequest(NewPatientSampleId, NewTestName);

        try
        {
            // 1. Try API
            await _apiClient.CreateSampleAsync(authToken, request);

            // 2. Success: Clear Form & Delete Draft
            NewPatientSampleId = string.Empty;
            NewTestName = string.Empty;
            if (File.Exists(DraftFileName)) File.Delete(DraftFileName);
            HasUnsavedDraft = false;

            await Search(); // Refresh the list
            MessageBox.Show("Sample added successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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
                    "Your entry has been saved as a draft.\n" +
                    "Click 'Add' again when the connection is restored.",
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
    private async Task MarkAsDone(int storageId)
    {
        var authToken = _authService.GetToken();
        if (string.IsNullOrEmpty(authToken)) return;

        try
        {
            await _apiClient.MarkSampleAsDoneAsync(authToken, storageId);
            var itemToRemove = PendingSamples.FirstOrDefault(s => s.StorageID == storageId);
            if (itemToRemove != null)
            {
                PendingSamples.Remove(itemToRemove);
            }
        }
        catch (Exception ex) { MessageBox.Show($"Failed to mark as done: {ex.Message}"); }
    }

    [RelayCommand]
    private void ShowDeleteFlyout(SampleStorageResponse sample)
    {
        SelectedSampleToDelete = sample;
        DeactivationReason = string.Empty;
        IsDeleteFlyoutOpen = true;
    }

    [RelayCommand]
    private async Task ConfirmDeactivation()
    {
        if (SelectedSampleToDelete is null || string.IsNullOrWhiteSpace(DeactivationReason))
        {
            MessageBox.Show("A reason is required for deletion.");
            return;
        }

        var authToken = _authService.GetToken();
        if (string.IsNullOrEmpty(authToken)) return;

        try
        {
            var request = new DeactivateSampleStorageRequest(DeactivationReason);
            await _apiClient.DeactivateSampleAsync(authToken, SelectedSampleToDelete.StorageID, request);

            IsDeleteFlyoutOpen = false;
            await Search();
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

    [RelayCommand]
    private void ClearDraft()
    {
        try
        {
            if (File.Exists(DraftFileName))
            {
                File.Delete(DraftFileName);
                NewPatientSampleId = string.Empty;
                NewTestName = string.Empty;
                HasUnsavedDraft = false;
                MessageBox.Show("Draft cleared successfully.", "Draft Cleared", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to clear draft: {ex.Message}", "Error");
        }
    }
}