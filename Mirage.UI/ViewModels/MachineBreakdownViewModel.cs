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

public partial class MachineBreakdownViewModel : ObservableObject
{
    private readonly IPortalMirageApi _apiClient;
    private readonly IAuthService _authService;

    [ObservableProperty] private string? _selectedMachineName;
    [ObservableProperty] private string _breakdownReason = string.Empty;
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
                OnPropertyChanged(nameof(IsResolvedViewActive));
                SearchCommand.Execute(null);
            }
        }
    }
    public bool IsResolvedViewActive
    {
        get => _activeView == "Resolved";
        set
        {
            if (value && _activeView != "Resolved")
            {
                _activeView = "Resolved";
                OnPropertyChanged(nameof(IsPendingViewActive));
                OnPropertyChanged(nameof(IsResolvedViewActive));
                SearchCommand.Execute(null);
            }
        }
    }

    [ObservableProperty] private bool _isResolveFlyoutOpen;
    [ObservableProperty] private MachineBreakdownResponse? _selectedBreakdownToResolve;
    [ObservableProperty] private string _resolutionNotes = string.Empty;
    [ObservableProperty] private bool _isDeleteFlyoutOpen;
    [ObservableProperty] private MachineBreakdownResponse? _selectedBreakdownToDelete;
    [ObservableProperty] private string _deactivationReason = string.Empty;

    public ObservableCollection<MachineBreakdownResponse> PendingBreakdowns { get; } = new();
    public ObservableCollection<MachineBreakdownResponse> ResolvedBreakdowns { get; } = new();
    public ObservableCollection<string> MachineNames { get; } = new();

    public MachineBreakdownViewModel(IPortalMirageApi apiClient, IAuthService authService)
    {
        _apiClient = apiClient;
        _authService = authService;
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task LoadMachineNamesAsync()
    {
        var authToken = _authService.GetToken();
        if (string.IsNullOrEmpty(authToken)) return;

        try
        {
            var machineNameItems = await _apiClient.GetListItemsByTypeAsync(authToken, "MachineName");

            MachineNames.Clear();
            foreach (var item in machineNameItems)
            {
                if (item.IsActive) // Only add active items to the dropdown
                {
                    MachineNames.Add(item.ItemValue);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to load machine names: {ex.Message}", "Error");
        }
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
                var breakdowns = await _apiClient.GetPendingBreakdownsAsync(authToken, StartDate, EndDate);
                PendingBreakdowns.Clear();
                foreach (var b in breakdowns) PendingBreakdowns.Add(b);
            }
            else // Resolved
            {
                var breakdowns = await _apiClient.GetResolvedBreakdownsAsync(authToken, StartDate, EndDate);
                ResolvedBreakdowns.Clear();
                foreach (var b in breakdowns) ResolvedBreakdowns.Add(b);
            }
        }
        catch (Exception ex) { MessageBox.Show($"Failed to load breakdowns: {ex.Message}"); }
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task Submit()
    {
        var authToken = _authService.GetToken();
        if (string.IsNullOrEmpty(authToken) || string.IsNullOrEmpty(SelectedMachineName) || string.IsNullOrEmpty(BreakdownReason))
        {
            MessageBox.Show("Machine Name and Reason are required.");
            return;
        }
        try
        {
            var request = new CreateMachineBreakdownRequest(SelectedMachineName, BreakdownReason);
            await _apiClient.CreateBreakdownAsync(authToken, request);
            SelectedMachineName = null;
            BreakdownReason = string.Empty;
            IsPendingViewActive = true;
        }
        catch (Exception ex) { MessageBox.Show($"Failed to submit breakdown: {ex.Message}"); }
    }

    [RelayCommand]
    private void ShowResolveFlyout(MachineBreakdownResponse breakdown)
    {
        SelectedBreakdownToResolve = breakdown;
        ResolutionNotes = string.Empty;
        IsResolveFlyoutOpen = true;
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task ConfirmResolve()
    {
        if (SelectedBreakdownToResolve is null || string.IsNullOrWhiteSpace(ResolutionNotes))
        {
            MessageBox.Show("Resolution notes cannot be empty.");
            return;
        }

        var authToken = _authService.GetToken();
        if (string.IsNullOrEmpty(authToken)) return;

        try
        {
            var request = new ResolveBreakdownRequest(ResolutionNotes);
            await _apiClient.MarkBreakdownAsResolvedAsync(authToken, SelectedBreakdownToResolve.BreakdownID, request);
            PendingBreakdowns.Remove(SelectedBreakdownToResolve);
            IsResolveFlyoutOpen = false;
        }
        catch (Exception ex) { MessageBox.Show($"Failed to resolve breakdown: {ex.Message}"); }
    }

    [RelayCommand]
    private void ShowDeleteFlyout(MachineBreakdownResponse breakdown)
    {
        SelectedBreakdownToDelete = breakdown;
        DeactivationReason = string.Empty;
        IsDeleteFlyoutOpen = true;
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task ConfirmDeactivation()
    {
        if (SelectedBreakdownToDelete is null || string.IsNullOrWhiteSpace(DeactivationReason))
        {
            MessageBox.Show("A reason is required for deactivation.");
            return;
        }

        var authToken = _authService.GetToken();
        if (string.IsNullOrEmpty(authToken)) return;

        try
        {
            var request = new DeactivateMachineBreakdownRequest(DeactivationReason);
            await _apiClient.DeactivateBreakdownAsync(authToken, SelectedBreakdownToDelete.BreakdownID, request);
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