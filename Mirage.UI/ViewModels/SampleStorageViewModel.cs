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

public partial class SampleStorageViewModel : ObservableObject
{
    public static string? AuthToken { get; set; }
    private readonly IPortalMirageApi _apiClient;

    [ObservableProperty] private string _newPatientSampleId = string.Empty;
    [ObservableProperty] private string _newTestName = string.Empty;
    [ObservableProperty] private DateTime _startDate = DateTime.Today;
    [ObservableProperty] private DateTime _endDate = DateTime.Today;

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

    // Properties for the "Delete" Flyout
    [ObservableProperty] private bool _isDeleteFlyoutOpen;
    [ObservableProperty] private SampleStorageResponse? _selectedSampleToDelete;
    [ObservableProperty] private string _deactivationReason = string.Empty;

    public ObservableCollection<SampleStorageResponse> PendingSamples { get; } = new();
    public ObservableCollection<SampleStorageResponse> CompletedSamples { get; } = new();

    public SampleStorageViewModel()
    {
        _apiClient = RestService.For<IPortalMirageApi>("https://localhost:7210");
        SearchCommand.Execute(null);
    }

    [RelayCommand]
    private async Task Search()
    {
        if (string.IsNullOrEmpty(AuthToken)) return;
        if (_activeView == "Pending")
        {
            try
            {
                var samples = await _apiClient.GetPendingSamplesAsync(AuthToken, StartDate, EndDate);
                PendingSamples.Clear();
                foreach (var sample in samples) PendingSamples.Add(sample);
            }
            catch (Exception ex) { MessageBox.Show($"Failed to load pending samples: {ex.Message}"); }
        }
        else // Completed
        {
            try
            {
                var samples = await _apiClient.GetCompletedSamplesAsync(AuthToken, StartDate, EndDate);
                CompletedSamples.Clear();
                foreach (var sample in samples) CompletedSamples.Add(sample);
            }
            catch (Exception ex) { MessageBox.Show($"Failed to load completed samples: {ex.Message}"); }
        }
    }

    [RelayCommand]
    private async Task Add()
    {
        if (string.IsNullOrEmpty(AuthToken) || string.IsNullOrEmpty(NewPatientSampleId) || string.IsNullOrEmpty(NewTestName))
        {
            MessageBox.Show("Patient Sample ID and Test Name are required.");
            return;
        }
        try
        {
            var request = new CreateSampleStorageRequest(NewPatientSampleId, NewTestName);
            await _apiClient.CreateSampleAsync(AuthToken, request);
            NewPatientSampleId = string.Empty;
            NewTestName = string.Empty;
            await Search();
        }
        catch (Exception ex) { MessageBox.Show($"Failed to add sample: {ex.Message}"); }
    }

    [RelayCommand]
    private async Task MarkAsDone(int storageId)
    {
        if (string.IsNullOrEmpty(AuthToken)) return;
        try
        {
            await _apiClient.MarkSampleAsDoneAsync(AuthToken, storageId);
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
        if (string.IsNullOrEmpty(AuthToken)) return;
        try
        {
            var request = new DeactivateSampleStorageRequest(DeactivationReason);
            await _apiClient.DeactivateSampleAsync(AuthToken, SelectedSampleToDelete.StorageID, request);

            IsDeleteFlyoutOpen = false;
            await Search(); // Refresh the current list
        }
        catch (Exception ex) { MessageBox.Show($"Failed to delete entry: {ex.Message}"); }
    }
}