using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mirage.UI.Services;
using Refit;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using PortalMirage.Core.Dtos;

namespace Mirage.UI.ViewModels;

public partial class SampleStorageViewModel : ObservableObject
{
    public static string? AuthToken { get; set; }
    private readonly IPortalMirageApi _apiClient;

    [ObservableProperty]
    private string _newPatientSampleId = string.Empty;

    // ... right below the NewPatientSampleId property ...
    [ObservableProperty]
    private string _newTestName = string.Empty;

    [ObservableProperty]
    private DateTime _startDate = DateTime.Today;

    [ObservableProperty]
    private DateTime _endDate = DateTime.Today;

    public ObservableCollection<SampleStorageResponse> PendingSamples { get; } = new();

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
        catch (Exception ex) { MessageBox.Show($"Failed to load samples: {ex.Message}"); }
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
            var request = new CreateSampleStorageRequest(NewPatientSampleId, NewTestName); // Update this line
            await _apiClient.CreateSampleAsync(AuthToken, request);
            NewPatientSampleId = string.Empty;
            NewTestName = string.Empty; // Add this line to clear the new box
            await LoadPending(); // Refresh the list
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

            // Remove the item from the list visually
            var itemToRemove = PendingSamples.FirstOrDefault(s => s.StorageID == storageId);
            if (itemToRemove != null)
            {
                PendingSamples.Remove(itemToRemove);
            }
        }
        catch (Exception ex) { MessageBox.Show($"Failed to mark as done: {ex.Message}"); }
    }
}