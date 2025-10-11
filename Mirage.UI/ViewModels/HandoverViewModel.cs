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
    private readonly IPortalMirageApi _apiClient;
    private readonly IAuthService _authService;

    [ObservableProperty]
    private string _newHandoverNotes = string.Empty;
    [ObservableProperty]
    private string _selectedPriority = "Normal";
    [ObservableProperty]
    private string _selectedShift = "Morning";
    [ObservableProperty]
    private DateTime _startDate = DateTime.Today;
    [ObservableProperty]
    private DateTime _endDate = DateTime.Today;
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

    public ObservableCollection<HandoverResponse> PendingHandovers { get; } = new();
    public ObservableCollection<HandoverResponse> CompletedHandovers { get; } = new();
    public ObservableCollection<string> Priorities { get; } = new() { "Normal", "Urgent" };
    public ObservableCollection<string> Shifts { get; } = new() { "Morning", "Evening", "Night" };

    public HandoverViewModel(IPortalMirageApi apiClient, IAuthService authService)
    {
        _apiClient = apiClient;
        _authService = authService;
        SearchCommand.Execute(null);
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task Search()
    {
        var authToken = _authService.GetToken();
        if (string.IsNullOrEmpty(authToken)) return;

        try
        {
            if (_activeView == "Pending")
            {
                var handovers = await _apiClient.GetPendingHandoversAsync(authToken, StartDate, EndDate);
                PendingHandovers.Clear();
                foreach (var handover in handovers) PendingHandovers.Add(handover);
            }
            else // Completed
            {
                var handovers = await _apiClient.GetCompletedHandoversAsync(authToken, StartDate, EndDate);
                CompletedHandovers.Clear();
                foreach (var handover in handovers) CompletedHandovers.Add(handover);
            }
        }
        catch (Exception ex) { MessageBox.Show($"Failed to load handovers: {ex.Message}"); }
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task Submit()
    {
        var authToken = _authService.GetToken();
        if (string.IsNullOrEmpty(authToken) || string.IsNullOrEmpty(NewHandoverNotes))
        {
            MessageBox.Show("Handover notes cannot be empty.");
            return;
        }
        try
        {
            var request = new CreateHandoverRequest(NewHandoverNotes, SelectedPriority, SelectedShift);
            await _apiClient.CreateHandoverAsync(authToken, request);
            NewHandoverNotes = string.Empty;
            await Search();
        }
        catch (Exception ex) { MessageBox.Show($"Failed to submit handover: {ex.Message}"); }
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task Receive(int handoverId)
    {
        var authToken = _authService.GetToken();
        if (string.IsNullOrEmpty(authToken)) return;

        try
        {
            await _apiClient.MarkHandoverAsReceivedAsync(authToken, handoverId);
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
    private async System.Threading.Tasks.Task ConfirmDeactivation()
    {
        if (SelectedHandoverToDelete is null || string.IsNullOrWhiteSpace(DeactivationReason))
        {
            MessageBox.Show("A reason is required for deactivation.");
            return;
        }

        var authToken = _authService.GetToken();
        if (string.IsNullOrEmpty(authToken)) return;

        try
        {
            var request = new DeactivateHandoverRequest(DeactivationReason);
            await _apiClient.DeactivateHandoverAsync(authToken, SelectedHandoverToDelete.HandoverID, request);

            IsDeleteFlyoutOpen = false;
            await Search();
        }
        catch (ApiException ex)
        {
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