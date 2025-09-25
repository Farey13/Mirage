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

public partial class HandoverViewModel : ObservableObject
{
    public static string? AuthToken { get; set; }
    private readonly IPortalMirageApi _apiClient;

    [ObservableProperty] private string _newHandoverNotes = string.Empty;
    [ObservableProperty] private string _selectedPriority = "Normal";
    [ObservableProperty] private string _selectedShift = "Morning";

    public ObservableCollection<HandoverResponse> PendingHandovers { get; } = new();
    public ObservableCollection<string> Priorities { get; } = new() { "Normal", "Urgent" };
    public ObservableCollection<string> Shifts { get; } = new() { "Morning", "Evening", "Night" };

    public HandoverViewModel()
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
            var handovers = await _apiClient.GetPendingHandoversAsync(AuthToken);
            PendingHandovers.Clear();
            foreach (var handover in handovers) PendingHandovers.Add(handover);
        }
        catch (Exception ex) { MessageBox.Show($"Failed to load handovers: {ex.Message}"); }
    }

    [RelayCommand]
    private async Task Submit()
    {
        if (string.IsNullOrEmpty(AuthToken) || string.IsNullOrEmpty(NewHandoverNotes))
        {
            MessageBox.Show("Handover notes cannot be empty.");
            return;
        }
        try
        {
            var request = new CreateHandoverRequest(NewHandoverNotes, SelectedPriority, SelectedShift);
            await _apiClient.CreateHandoverAsync(AuthToken, request);
            NewHandoverNotes = string.Empty;
            await LoadPending();
        }
        catch (Exception ex) { MessageBox.Show($"Failed to submit handover: {ex.Message}"); }
    }

    [RelayCommand]
    private async Task Receive(int handoverId)
    {
        if (string.IsNullOrEmpty(AuthToken)) return;
        try
        {
            await _apiClient.MarkHandoverAsReceivedAsync(AuthToken, handoverId);
            var itemToRemove = PendingHandovers.FirstOrDefault(h => h.HandoverID == handoverId);
            if (itemToRemove != null) PendingHandovers.Remove(itemToRemove);
        }
        catch (Exception ex) { MessageBox.Show($"Failed to receive handover: {ex.Message}"); }
    }
}