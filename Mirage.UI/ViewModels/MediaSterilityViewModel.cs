using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mirage.UI.Services;
using PortalMirage.Core.Dtos;
using Refit;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace Mirage.UI.ViewModels;

public partial class MediaSterilityViewModel : ObservableObject
{
    private readonly IPortalMirageApi _apiClient;
    private readonly IAuthService _authService;

    [ObservableProperty] private string? _selectedMediaName;
    [ObservableProperty] private string _mediaLotNumber = string.Empty;
    [ObservableProperty] private string? _mediaQuantity;
    [ObservableProperty] private string? _selectedResult37C;
    [ObservableProperty] private string? _selectedResult25C;
    [ObservableProperty] private string? _comments;
    [ObservableProperty] private DateTime _startDate = DateTime.Today;
    [ObservableProperty] private DateTime _endDate = DateTime.Today;

    // DRAFT CONFIGURATION
    private const string DraftFileName = "draft_mediasterility.json";
    [ObservableProperty] private bool _hasUnsavedDraft;

    public ObservableCollection<MediaSterilityCheckResponse> Logs { get; } = new();

    [ObservableProperty] private bool _isDeleteFlyoutOpen;
    [ObservableProperty] private MediaSterilityCheckResponse? _selectedLogToDelete;
    [ObservableProperty] private string _deactivationReason = string.Empty;

    public ObservableCollection<string> MediaNames { get; } = new();
    public ObservableCollection<string> GrowthResults { get; } = new() { "No Growth", "Growth Seen" };

    public MediaSterilityViewModel(IPortalMirageApi apiClient, IAuthService authService)
    {
        _apiClient = apiClient;
        _authService = authService;

        // REMOVED: Hardcoded media names

        // ADDED: Load master lists from API
        _ = LoadMasterLists();

        // CHECK FOR DRAFT ON STARTUP
        if (File.Exists(DraftFileName))
        {
            try
            {
                var json = File.ReadAllText(DraftFileName);
                // Correct DTO name: CreateMediaSterilityCheckRequest
                var draft = JsonSerializer.Deserialize<CreateMediaSterilityCheckRequest>(json);

                if (draft != null)
                {
                    // Map the draft back to your form properties
                    SelectedMediaName = draft.MediaName;
                    MediaLotNumber = draft.MediaLotNumber;
                    MediaQuantity = draft.MediaQuantity;
                    SelectedResult37C = draft.Result37C;
                    SelectedResult25C = draft.Result25C;
                    Comments = draft.Comments;

                    HasUnsavedDraft = true;
                    MessageBox.Show("We found an unsaved sterility check and restored it.", "Draft Restored", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch
            {
                try { File.Delete(DraftFileName); }
                catch { }
            }
        }
    }

    // ADDED: Method to load master lists from API
    public async Task LoadMasterLists()
    {
        var token = _authService.GetToken();
        if (string.IsNullOrEmpty(token)) return;

        try
        {
            var items = await _apiClient.GetListItemsByTypeAsync(token, "MediaName");
            MediaNames.Clear();
            foreach (var item in items.Where(i => i.IsActive)) MediaNames.Add(item.ItemValue);
        }
        catch (Exception) { }
    }

    [RelayCommand]
    private async Task LoadLogs()
    {
        var authToken = _authService.GetToken();
        if (string.IsNullOrEmpty(authToken)) return;

        try
        {
            var logs = await _apiClient.GetSterilityChecksAsync(authToken, StartDate, EndDate);
            Logs.Clear();
            foreach (var log in logs) Logs.Add(log);
        }
        catch (Exception ex) { MessageBox.Show($"Failed to load logs: {ex.Message}"); }
    }

    [RelayCommand]
    private async Task Save()
    {
        var authToken = _authService.GetToken();

        // Validate required fields
        if (string.IsNullOrEmpty(authToken) || string.IsNullOrEmpty(SelectedMediaName) || string.IsNullOrEmpty(MediaLotNumber) || string.IsNullOrEmpty(SelectedResult25C) || string.IsNullOrEmpty(SelectedResult37C))
        {
            MessageBox.Show("Media Name, Lot Number, and both Results are required.");
            return;
        }

        // Create Request using the correct DTO structure
        var request = new CreateMediaSterilityCheckRequest(
            SelectedMediaName,
            MediaLotNumber,
            MediaQuantity,
            SelectedResult37C,
            SelectedResult25C,
            Comments
        );

        try
        {
            // 1. Try API
            await _apiClient.CreateSterilityCheckAsync(authToken, request);

            // 2. Success
            Clear();
            if (File.Exists(DraftFileName)) File.Delete(DraftFileName);
            HasUnsavedDraft = false;

            await LoadLogs(); // Refresh list
            MessageBox.Show("Sterility record saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            // 3. Failure: Save Draft
            try
            {
                var json = JsonSerializer.Serialize(request);
                await File.WriteAllTextAsync(DraftFileName, json);
                HasUnsavedDraft = true;

                MessageBox.Show(
                    $"Connection failed: {ex.Message}\n\n" +
                    "This record has been saved as a draft.\n" +
                    "Please submit it again when the connection is restored.",
                    "Network Error - Draft Saved",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
            catch (Exception fileEx)
            {
                MessageBox.Show($"Critical Error: Could not save draft.\n{fileEx.Message}", "Error");
            }
        }
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
    }

    [RelayCommand]
    private async Task ClearDraft()
    {
        try
        {
            if (File.Exists(DraftFileName))
            {
                File.Delete(DraftFileName);
                Clear();
                HasUnsavedDraft = false;
                MessageBox.Show("Draft cleared successfully.", "Draft Cleared", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to clear draft: {ex.Message}", "Error");
        }
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

        var authToken = _authService.GetToken();
        if (string.IsNullOrEmpty(authToken)) return;

        try
        {
            var request = new DeactivateMediaSterilityCheckRequest(DeactivationReason);
            await _apiClient.DeactivateSterilityCheckAsync(authToken, SelectedLogToDelete.SterilityCheckID, request);

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