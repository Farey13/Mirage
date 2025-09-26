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
    public static string? AuthToken { get; set; }
    private readonly IPortalMirageApi _apiClient;

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

    // Properties for the "Deactivate" Flyout
    [ObservableProperty]
    private bool _isDeleteFlyoutOpen;

    [ObservableProperty]
    private MachineBreakdownResponse? _selectedBreakdownToDelete;

    [ObservableProperty]
    private string _deactivationReason = string.Empty;

    public ObservableCollection<MachineBreakdownResponse> PendingBreakdowns { get; } = new();
    public ObservableCollection<MachineBreakdownResponse> ResolvedBreakdowns { get; } = new();
    public ObservableCollection<string> MachineNames { get; } = new();

    public MachineBreakdownViewModel()
    {
        _apiClient = RestService.For<IPortalMirageApi>("https://localhost:7210");
        MachineNames.Add("Vitros 250");
        MachineNames.Add("Abbott Architect");
        MachineNames.Add("Sysmex XN-1000");
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
                var breakdowns = await _apiClient.GetPendingBreakdownsAsync(AuthToken, StartDate, EndDate);
                PendingBreakdowns.Clear();
                foreach (var b in breakdowns) PendingBreakdowns.Add(b);
            }
            else // Resolved
            {
                var breakdowns = await _apiClient.GetResolvedBreakdownsAsync(AuthToken, StartDate, EndDate);
                ResolvedBreakdowns.Clear();
                foreach (var b in breakdowns) ResolvedBreakdowns.Add(b);
            }
        }
        catch (Exception ex) { MessageBox.Show($"Failed to load breakdowns: {ex.Message}"); }
    }

    [RelayCommand]
    private async Task Submit()
    {
        if (string.IsNullOrEmpty(AuthToken) || string.IsNullOrEmpty(SelectedMachineName) || string.IsNullOrEmpty(BreakdownReason))
        {
            MessageBox.Show("Machine Name and Reason are required.");
            return;
        }
        try
        {
            var request = new CreateMachineBreakdownRequest(SelectedMachineName, BreakdownReason);
            await _apiClient.CreateBreakdownAsync(AuthToken, request);

            // Clear the form
            SelectedMachineName = null;
            BreakdownReason = string.Empty;

            // Ensure we are on the pending view and refresh it
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
    private async Task ConfirmResolve()
    {
        if (SelectedBreakdownToResolve is null || string.IsNullOrWhiteSpace(ResolutionNotes))
        {
            MessageBox.Show("Resolution notes cannot be empty.");
            return;
        }
        if (string.IsNullOrEmpty(AuthToken)) return;
        try
        {
            var request = new ResolveBreakdownRequest(ResolutionNotes);
            await _apiClient.MarkBreakdownAsResolvedAsync(AuthToken, SelectedBreakdownToResolve.BreakdownID, request);

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
    private async Task ConfirmDeactivation()
    {
        if (SelectedBreakdownToDelete is null || string.IsNullOrWhiteSpace(DeactivationReason))
        {
            MessageBox.Show("A reason is required for deactivation.");
            return;
        }
        if (string.IsNullOrEmpty(AuthToken)) return;
        try
        {
            var request = new DeactivateMachineBreakdownRequest(DeactivationReason);
            await _apiClient.DeactivateBreakdownAsync(AuthToken, SelectedBreakdownToDelete.BreakdownID, request);

            IsDeleteFlyoutOpen = false;
            await Search(); // Refresh the current list
        }
        catch (ApiException ex)
        {
            if (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                MessageBox.Show("You do not have permission to perform this action. Please contact an administrator.", "Authorization Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else
            {
                MessageBox.Show($"An error occurred: {ex.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"An error occurred: {ex.Message}");
        }
    }
}