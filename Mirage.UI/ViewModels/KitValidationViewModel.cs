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

public partial class KitValidationViewModel : ObservableObject
{
    public static string? AuthToken { get; set; }
    private readonly IPortalMirageApi _apiClient;

    // Form properties
    [ObservableProperty] private string _kitName = string.Empty;
    [ObservableProperty] private string _kitLotNumber = string.Empty;
    [ObservableProperty] private DateTime? _kitExpiryDate = DateTime.Today.AddMonths(6);
    [ObservableProperty] private string? _selectedValidationStatus;
    [ObservableProperty] private string? _comments;

    // Search properties
    [ObservableProperty] private DateTime _startDate = DateTime.Today;
    [ObservableProperty] private DateTime _endDate = DateTime.Today;
    public ObservableCollection<KitValidationResponse> Logs { get; } = new();
    public ObservableCollection<string> ValidationStatuses { get; } = new();

    // Properties for the "Deactivate" Flyout
    [ObservableProperty] private bool _isDeleteFlyoutOpen;
    [ObservableProperty] private KitValidationResponse? _selectedLogToDelete;
    [ObservableProperty] private string _deactivationReason = string.Empty;

    public KitValidationViewModel()
    {
        _apiClient = RestService.For<IPortalMirageApi>("https://localhost:7210");
        ValidationStatuses.Add("Accepted");
        ValidationStatuses.Add("Rejected");
        LoadLogsCommand.Execute(null);
    }

    [RelayCommand]
    private async Task LoadLogs()
    {
        if (string.IsNullOrEmpty(AuthToken)) return;
        try
        {
            var logs = await _apiClient.GetKitValidationsAsync(AuthToken, StartDate, EndDate);
            Logs.Clear();
            foreach (var log in logs) Logs.Add(log);
        }
        catch (Exception ex) { MessageBox.Show($"Failed to load logs: {ex.Message}"); }
    }

    [RelayCommand]
    private async Task Save()
    {
        if (string.IsNullOrEmpty(AuthToken) || string.IsNullOrEmpty(KitName) || string.IsNullOrEmpty(KitLotNumber) || KitExpiryDate is null || string.IsNullOrEmpty(SelectedValidationStatus))
        {
            MessageBox.Show("All fields except comments are required.");
            return;
        }
        try
        {
            var request = new CreateKitValidationRequest(KitName, KitLotNumber, KitExpiryDate.Value, SelectedValidationStatus, Comments);
            await _apiClient.CreateKitValidationAsync(AuthToken, request);
            Clear();
            await LoadLogs();
        }
        catch (Exception ex) { MessageBox.Show($"Failed to save log: {ex.Message}"); }
    }

    [RelayCommand]
    private void Clear()
    {
        KitName = string.Empty;
        KitLotNumber = string.Empty;
        KitExpiryDate = DateTime.Today.AddMonths(6);
        SelectedValidationStatus = null;
        Comments = string.Empty;
    }

    [RelayCommand]
    private void ShowDeleteFlyout(KitValidationResponse log)
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
            var request = new DeactivateKitValidationRequest(DeactivationReason);
            await _apiClient.DeactivateKitValidationAsync(AuthToken, SelectedLogToDelete.ValidationID, request);

            IsDeleteFlyoutOpen = false;
            await LoadLogs(); // Refresh the list
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
}