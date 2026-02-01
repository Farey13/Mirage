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

public partial class KitValidationViewModel : ObservableObject
{
    private readonly IPortalMirageApi _apiClient;
    private readonly IAuthService _authService;

    [ObservableProperty] private string _kitName = string.Empty;
    [ObservableProperty] private string _kitLotNumber = string.Empty;
    [ObservableProperty] private DateTime? _kitExpiryDate = DateTime.Today.AddMonths(6);
    [ObservableProperty] private string? _selectedValidationStatus;
    [ObservableProperty] private string? _comments;
    [ObservableProperty] private DateTime _startDate = DateTime.Today;
    [ObservableProperty] private DateTime _endDate = DateTime.Today;
    [ObservableProperty] private bool _isDeleteFlyoutOpen;
    [ObservableProperty] private KitValidationResponse? _selectedLogToDelete;
    [ObservableProperty] private string _deactivationReason = string.Empty;

    // DRAFT CONFIGURATION
    private const string DraftFileName = "draft_kitvalidation.json";
    [ObservableProperty] private bool _hasUnsavedDraft;

    public ObservableCollection<KitValidationResponse> Logs { get; } = new();
    public ObservableCollection<string> ValidationStatuses { get; } = new();

    // ADDED: Kit name options property
    public ObservableCollection<string> KitNameOptions { get; } = new();

    public KitValidationViewModel(IPortalMirageApi apiClient, IAuthService authService)
    {
        _apiClient = apiClient;
        _authService = authService;
        ValidationStatuses.Add("Accepted");
        ValidationStatuses.Add("Rejected");

        // ADDED: Load master lists from API
        _ = LoadMasterLists();

        // CHECK FOR DRAFT ON STARTUP
        if (File.Exists(DraftFileName))
        {
            try
            {
                var json = File.ReadAllText(DraftFileName);
                // Verify 'CreateKitValidationRequest' matches your DTO name
                var draft = JsonSerializer.Deserialize<CreateKitValidationRequest>(json);

                if (draft != null)
                {
                    // Map the draft back to your form properties
                    // USING THE CORRECT PROPERTY NAMES FROM CreateKitValidationRequest
                    KitName = draft.KitName;
                    KitLotNumber = draft.KitLotNumber; // Fixed: Use actual property name
                    KitExpiryDate = draft.KitExpiryDate; // Fixed: Use actual property name
                    SelectedValidationStatus = draft.ValidationStatus; // Fixed: Use actual property name
                    Comments = draft.Comments;

                    HasUnsavedDraft = true;
                    MessageBox.Show("We found an unsaved kit validation entry and restored it.", "Draft Restored", MessageBoxButton.OK, MessageBoxImage.Information);
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
            var items = await _apiClient.GetListItemsByTypeAsync(token, "KitName");
            KitNameOptions.Clear();
            foreach (var item in items.Where(i => i.IsActive)) KitNameOptions.Add(item.ItemValue);
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
            var logs = await _apiClient.GetKitValidationsAsync(authToken, StartDate, EndDate);
            Logs.Clear();
            foreach (var log in logs) Logs.Add(log);
        }
        catch (Exception ex) { MessageBox.Show($"Failed to load logs: {ex.Message}"); }
    }

    [RelayCommand]
    private async Task Save()
    {
        var authToken = _authService.GetToken();

        if (string.IsNullOrEmpty(authToken) || string.IsNullOrEmpty(KitName) || string.IsNullOrEmpty(KitLotNumber) || KitExpiryDate is null || string.IsNullOrEmpty(SelectedValidationStatus))
        {
            MessageBox.Show("All fields except comments are required.");
            return;
        }

        // Create Request - Using the correct property names that match your DTO
        var request = new CreateKitValidationRequest(KitName, KitLotNumber, KitExpiryDate.Value, SelectedValidationStatus, Comments);

        try
        {
            // 1. Try API
            await _apiClient.CreateKitValidationAsync(authToken, request);

            // 2. Success
            Clear();
            if (File.Exists(DraftFileName)) File.Delete(DraftFileName);
            HasUnsavedDraft = false;

            await LoadLogs(); // Refresh the list
            MessageBox.Show("Validation record saved successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
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
                    "This validation record has been saved as a draft.\n" +
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
        KitName = string.Empty;
        KitLotNumber = string.Empty;
        KitExpiryDate = DateTime.Today.AddMonths(6);
        SelectedValidationStatus = null;
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

        var authToken = _authService.GetToken();
        if (string.IsNullOrEmpty(authToken)) return;

        try
        {
            var request = new DeactivateKitValidationRequest(DeactivationReason);
            await _apiClient.DeactivateKitValidationAsync(authToken, SelectedLogToDelete.ValidationID, request);
            IsDeleteFlyoutOpen = false;
            await LoadLogs();
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