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

    // Properties for the "Create" form
    [ObservableProperty]
    private string _newHandoverNotes = string.Empty;
    [ObservableProperty]
    private string _selectedPriority = "Normal";
    [ObservableProperty]
    private string _selectedShift = "Morning";

    // Properties for searching and view state
    [ObservableProperty]
    private DateTime _startDate = DateTime.Today;
    [ObservableProperty]
    private DateTime _endDate = DateTime.Today;

    // Properties for the "Deactivate" Flyout
    [ObservableProperty]
    private bool _isDeleteFlyoutOpen;

    [ObservableProperty]
    private HandoverResponse? _selectedHandoverToDelete;

    [ObservableProperty]
    private string _deactivationReason = string.Empty;

    private string _activeView = "Pending";
    public bool IsPendingViewActive
    {
        get => _activeView == "Pending";
        set
        {
            if (value)
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
            if (value)
            {
                _activeView = "Completed";
                OnPropertyChanged(nameof(IsPendingViewActive));
                OnPropertyChanged(nameof(IsCompletedViewActive));
                SearchCommand.Execute(null);
            }
        }
    }

    // Collections for the data grids
    public ObservableCollection<HandoverResponse> PendingHandovers { get; } = new();
    public ObservableCollection<HandoverResponse> CompletedHandovers { get; } = new();

    // Collections for the dropdown lists
    public ObservableCollection<string> Priorities { get; } = new() { "Normal", "Urgent" };
    public ObservableCollection<string> Shifts { get; } = new() { "Morning", "Evening", "Night" };

    public HandoverViewModel()
    {
        _apiClient = RestService.For<IPortalMirageApi>("https://localhost:7210");
        SearchCommand.Execute(null);
    }

    [RelayCommand]
    private async Task Search()
    {
        if (string.IsNullOrEmpty(AuthToken)) return;
        try
        {
            if (_activeView == "Pending")
            {
                var handovers = await _apiClient.GetPendingHandoversAsync(AuthToken, StartDate, EndDate);
                PendingHandovers.Clear();
                foreach (var handover in handovers) PendingHandovers.Add(handover);
            }
            else // Completed
            {
                var handovers = await _apiClient.GetCompletedHandoversAsync(AuthToken, StartDate, EndDate);
                CompletedHandovers.Clear();
                foreach (var handover in handovers) CompletedHandovers.Add(handover);
            }
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
            await Search();
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

    [RelayCommand]
    private void ShowDeleteFlyout(HandoverResponse handover)
    {
        SelectedHandoverToDelete = handover;
        DeactivationReason = string.Empty;
        IsDeleteFlyoutOpen = true;
    }

    [RelayCommand]
    private async Task ConfirmDeactivation()
    {
        if (SelectedHandoverToDelete is null || string.IsNullOrWhiteSpace(DeactivationReason))
        {
            MessageBox.Show("A reason is required for deactivation.");
            return;
        }
        if (string.IsNullOrEmpty(AuthToken)) return;
        try
        {
            var request = new DeactivateHandoverRequest(DeactivationReason);
            await _apiClient.DeactivateHandoverAsync(AuthToken, SelectedHandoverToDelete.HandoverID, request);

            IsDeleteFlyoutOpen = false;
            await Search();
        }
        catch (ApiException ex)
        {
            // Check for FORBIDDEN (403) OR the INTERNAL SERVER ERROR (500)
            if (ex.StatusCode == System.Net.HttpStatusCode.Forbidden ||
                ex.StatusCode == System.Net.HttpStatusCode.InternalServerError)
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