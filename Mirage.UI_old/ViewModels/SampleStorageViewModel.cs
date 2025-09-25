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

    [ObservableProperty] private string _activeView = "Pending"; // To control which grid is visible

    public ObservableCollection<SampleStorageResponse> PendingSamples { get; } = new();
    public ObservableCollection<SampleStorageResponse> CompletedSamples { get; } = new();

    public SampleStorageViewModel()
    {
        _apiClient = RestService.For<IPortalMirageApi>("https://localhost:7210");
        LoadPendingCommand.Execute(null);
    }

    [RelayCommand]
    private async Task LoadPending()
    {
        if (string.IsNullOrEmpty(AuthToken)) return;
        try
        {
            var samples = await _apiClient.GetPendingSamplesAsync(AuthToken, StartDate, EndDate);
            PendingSamples.Clear();
            foreach (var sample in samples) PendingSamples.Add(sample);
        }
        catch (Exception ex) { MessageBox.Show($"Failed to load pending samples: {ex.Message}"); }
    }

    [RelayCommand]
    private async Task LoadCompleted()
    {
        if (string.IsNullOrEmpty(AuthToken)) return;
        try
        {
            var samples = await _apiClient.GetCompletedSamplesAsync(AuthToken, StartDate, EndDate);
            CompletedSamples.Clear();
            foreach (var sample in samples) CompletedSamples.Add(sample);
        }
        catch (Exception ex) { MessageBox.Show($"Failed to load completed samples: {ex.Message}"); }
    }

    [RelayCommand]
    private async Task Add()
    {
        // ... (Add method remains the same)
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
            await LoadPending();
        }
        catch (Exception ex) { MessageBox.Show($"Failed to add sample: {ex.Message}"); }
    }

    [RelayCommand]
    private async Task MarkAsDone(int storageId)
    {
        // ... (MarkAsDone method remains the same)
        if (string.IsNullOrEmpty(AuthToken)) return;
        try
        {
            await _apiClient.MarkSampleAsDoneAsync(AuthToken, storageId);
            var itemToRemove = PendingSamples.FirstOrDefault(s => s.StorageID == storageId);
            if (itemToRemove != null) PendingSamples.Remove(itemToRemove);
        }
        catch (Exception ex) { MessageBox.Show($"Failed to mark as done: {ex.Message}"); }
    }

    [RelayCommand]
    private async Task Delete(int storageId)
    {
        if (string.IsNullOrEmpty(AuthToken)) return;
        if (MessageBox.Show("Are you sure you want to delete this entry? This action will be logged.", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
        {
            return;
        }
        try
        {
            await _apiClient.DeactivateSampleAsync(AuthToken, storageId);
            // Refresh whichever list is active
            if (ActiveView == "Pending") await LoadPending();
            else await LoadCompleted();
        }
        catch (Exception ex) { MessageBox.Show($"Failed to delete entry: {ex.Message}"); }
    }
}