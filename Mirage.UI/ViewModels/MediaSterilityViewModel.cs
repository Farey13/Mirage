using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mirage.UI.Services;
using PortalMirage.Core.Dtos;
using Refit;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace Mirage.UI.ViewModels;

public partial class MediaSterilityViewModel : ObservableObject
{
    public static string? AuthToken { get; set; }
    private readonly IPortalMirageApi _apiClient;

    // Form properties
    [ObservableProperty] private string? _selectedMediaName;
    [ObservableProperty] private string _mediaLotNumber = string.Empty;
    [ObservableProperty] private string? _mediaQuantity;
    [ObservableProperty] private string? _selectedResult37C;
    [ObservableProperty] private string? _selectedResult25C;
    [ObservableProperty] private string? _comments;

    // Search and Data Grid
    [ObservableProperty] private DateTime _startDate = DateTime.Today;
    [ObservableProperty] private DateTime _endDate = DateTime.Today;
    public ObservableCollection<MediaSterilityCheckResponse> Logs { get; } = new();

    // Deactivation Flyout properties
    [ObservableProperty] private bool _isDeleteFlyoutOpen;
    [ObservableProperty] private MediaSterilityCheckResponse? _selectedLogToDelete;
    [ObservableProperty] private string _deactivationReason = string.Empty;

    // Dropdown options
    public ObservableCollection<string> MediaNames { get; } = new();
    public ObservableCollection<string> GrowthResults { get; } = new() { "No Growth", "Growth Seen" };

    public MediaSterilityViewModel()
    {
        _apiClient = RestService.For<IPortalMirageApi>("https://localhost:7210");
        MediaNames.Add("Blood Agar");
        MediaNames.Add("MacConkey Agar");
        MediaNames.Add("Chocolate Agar");
        LoadLogsCommand.Execute(null);
    }

    [RelayCommand]
    private async Task LoadLogs()
    {
        if (string.IsNullOrEmpty(AuthToken)) return;
        try
        {
            var logs = await _apiClient.GetSterilityChecksAsync(AuthToken, StartDate, EndDate);
            Logs.Clear();
            foreach (var log in logs) Logs.Add(log);
        }
        catch (Exception ex) { MessageBox.Show($"Failed to load logs: {ex.Message}"); }
    }

    [RelayCommand]
    private async Task Save()
    {
        if (string.IsNullOrEmpty(AuthToken) || string.IsNullOrEmpty(SelectedMediaName) || string.IsNullOrEmpty(MediaLotNumber) || string.IsNullOrEmpty(SelectedResult25C) || string.IsNullOrEmpty(SelectedResult37C))
        {
            MessageBox.Show("Media Name, Lot Number, and both Results are required.");
            return;
        }
        try
        {
            var request = new CreateMediaSterilityCheckRequest(SelectedMediaName, MediaLotNumber, MediaQuantity, SelectedResult37C, SelectedResult25C, Comments);
            await _apiClient.CreateSterilityCheckAsync(AuthToken, request);
            Clear();
            await LoadLogs();
        }
        catch (Exception ex) { MessageBox.Show($"Failed to save log: {ex.Message}"); }
    }

    [RelayCommand]
    private void Clear()
    {
        SelectedMediaName = null;
        MediaLotNumber = string.Empty;
        MediaQuantity = string.Empty;
        SelectedResult25C = null;
        SelectedResult37C = null;
        Comments = string.Empty;
        OnPropertyChanged(nameof(SelectedMediaName));
        OnPropertyChanged(nameof(SelectedResult25C));
        OnPropertyChanged(nameof(SelectedResult37C));
    }

    [RelayCommand]
    private void ShowDeleteFlyout(MediaSterilityCheckResponse log)
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
            var request = new DeactivateMediaSterilityCheckRequest(DeactivationReason);
            await _apiClient.DeactivateSterilityCheckAsync(AuthToken, SelectedLogToDelete.SterilityCheckID, request);

            IsDeleteFlyoutOpen = false;
            await LoadLogs();
        }
        catch (ApiException ex)
        {
            if (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                MessageBox.Show("You do not have permission to perform this action. Please contact an administrator.", "Authorization Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            else { MessageBox.Show($"An error occurred communicating with the server: {ex.StatusCode}"); }
        }
        catch (Exception ex) { MessageBox.Show($"Failed to deactivate entry: {ex.Message}"); }
    }
}