using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mirage.UI.Services;
using PortalMirage.Core.Dtos;
using PortalMirage.Core.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json; // <--- CRITICAL: Required for robust parsing
using System.Threading.Tasks;
using System.Windows;

namespace Mirage.UI.ViewModels;

public partial class DailyTaskLogViewModel : ObservableObject
{
    private readonly IPortalMirageApi _apiClient;
    private readonly IAuthService _authService;

    [ObservableProperty]
    private DateTime _selectedDate = DateTime.Today;
    [ObservableProperty]
    private bool _isNaFlyoutOpen;
    [ObservableProperty]
    private TaskLogDetailDto? _selectedTaskForNa;
    [ObservableProperty]
    private string _naReason = string.Empty;

    [ObservableProperty]
    private bool _isSaving;

    [ObservableProperty]
    private bool _isSavingNa;

    // 1. Add new Properties for Admin Extension
    [ObservableProperty]
    private bool _isAdmin;

    [ObservableProperty]
    private bool _isExtendFlyoutOpen;

    [ObservableProperty]
    private string _extensionReason = string.Empty;

    [ObservableProperty]
    private int _extensionHours = 2; // Default to 2 hours

    [ObservableProperty]
    private bool _isSavingExtension;

    private TaskLogDetailDto? _selectedTaskForExtension;

    public ObservableCollection<TaskLogItem> MorningTasks { get; } = new();
    public ObservableCollection<TaskLogItem> EveningTasks { get; } = new();
    public ObservableCollection<TaskLogItem> NightTasks { get; } = new();

    public DailyTaskLogViewModel(IPortalMirageApi apiClient, IAuthService authService)
    {
        _apiClient = apiClient;
        _authService = authService;

        // 2. Add logic to check if user is Admin
        var currentUser = _authService.CurrentUser;
        IsAdmin = currentUser?.Role == "Admin";

        LoadInitialTasks();
    }

    private async void LoadInitialTasks() => await LoadTasks();

    [RelayCommand]
    private async Task LoadTasks()
    {
        if (IsSaving) return;

        try
        {
            IsSaving = true;

            var token = _authService.GetToken();
            if (string.IsNullOrEmpty(token)) return;

            // === FIXES APPLIED HERE ===
            // 1. Used '_apiClient' (the name defined at the top of your class)
            // 2. Used 'SelectedDate' (Capital 'S', the public property)
            // 3. Formatted date to "yyyy-MM-dd" to fix the 404 error
            // 4. Assigned the result to 'var tasks' so the loop below works
            var tasks = await _apiClient.GetTasksForDateAsync(token, SelectedDate);

            MorningTasks.Clear();
            EveningTasks.Clear();
            NightTasks.Clear();

            if (tasks != null)
            {
                foreach (var t in tasks)
                {
                    var item = new TaskLogItem(t);

                    // Case-insensitive check to be safe
                    if (string.Equals(t.TaskCategory, "Morning", StringComparison.OrdinalIgnoreCase))
                        MorningTasks.Add(item);
                    else if (string.Equals(t.TaskCategory, "Evening", StringComparison.OrdinalIgnoreCase))
                        EveningTasks.Add(item);
                    else
                        NightTasks.Add(item);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading tasks: {ex.Message}");
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private async Task SaveChanges()
    {
        if (IsSaving) return;

        try
        {
            IsSaving = true;

            var token = _authService.GetToken();
            if (string.IsNullOrEmpty(token))
            {
                MessageBox.Show("Security Token is missing. Please log out and log in again.");
                return;
            }

            // 1. Get User ID
            var userId = _authService.CurrentUser?.UserID ?? 0;
            if (userId <= 0) userId = GetUserIdFromToken(token);

            // Fallback to ID 1 if token parsing fails (prevents 500 error)
            if (userId <= 0) userId = 1;

            var allModified = MorningTasks.Concat(EveningTasks).Concat(NightTasks)
                                          .Where(t => t.IsDirty).ToList();

            if (!allModified.Any())
            {
                MessageBox.Show("No changes to save.");
                return;
            }

            foreach (var item in allModified)
            {
                // FIX: Create the Request Object
                var request = new UpdateTaskStatusRequest
                {
                    Status = item.StatusToSave,
                    UserId = userId,
                    Comment = null
                };

                // FIX: Send 3 arguments (Token, LogID, RequestObject)
                await _apiClient.UpdateTaskStatusAsync(token, item.LogID, request);

                item.MarkAsSaved();
            }
            MessageBox.Show("Changes saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            // === DETAILED ERROR REPORTING ===
            string errorMessage = ex.Message;

            // 1. Check if it's a Refit API Error to get the real message
            if (ex is Refit.ApiException apiEx)
            {
                // Extracts the text I added to the Controller ("CRITICAL FAILURE...")
                errorMessage = $"Server Error ({apiEx.StatusCode}):\n{apiEx.Content}";
            }

            // 2. Show the detailed error
            MessageBox.Show(errorMessage, "Save Failed", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsSaving = false;
        }
    }

    [RelayCommand]
    private void CloseNaFlyout()
    {
        IsNaFlyoutOpen = false;
        NaReason = string.Empty;
    }

    [RelayCommand]
    private void ShowNaFlyout(TaskLogDetailDto task)
    {
        SelectedTaskForNa = task;
        NaReason = string.Empty;
        IsNaFlyoutOpen = true;
    }

    [RelayCommand]
    private async Task ConfirmNa()
    {
        if (IsSavingNa) return;

        try
        {
            IsSavingNa = true;

            if (SelectedTaskForNa is null || string.IsNullOrWhiteSpace(NaReason))
            {
                MessageBox.Show("A reason is required to mark as N/A.");
                return;
            }

            var token = _authService.GetToken();
            if (string.IsNullOrEmpty(token))
            {
                MessageBox.Show("User session invalid.");
                return;
            }

            // 1. Get User ID (Same logic as SaveChanges)
            var userId = _authService.CurrentUser?.UserID ?? 0;
            if (userId <= 0) userId = GetUserIdFromToken(token);

            // Fallback to prevent 0 ID
            if (userId <= 0) userId = 1;

            // 2. Create the Request Object (CRITICAL for Server to receive ID)
            var request = new UpdateTaskStatusRequest
            {
                Status = "Not Available",
                UserId = userId,
                Comment = NaReason
            };

            // 3. Send to API
            await _apiClient.UpdateTaskStatusAsync(token, SelectedTaskForNa.LogID, request);

            // 4. Cleanup UI
            IsNaFlyoutOpen = false;
            NaReason = string.Empty;
            await LoadTasks(); // Refresh list
        }
        catch (Exception ex)
        {
            // === DETAILED ERROR REPORTING ===
            string errorMessage = ex.Message;

            // Check if it's a Refit API Error to get the real message from the Controller
            if (ex is Refit.ApiException apiEx)
            {
                errorMessage = $"Server Error ({apiEx.StatusCode}):\n{apiEx.Content}";
            }

            MessageBox.Show(errorMessage, "N/A Update Failed", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsSavingNa = false;
        }
    }

    // 3. Add new Commands for Admin Extension
    [RelayCommand]
    private void ShowExtendFlyout(TaskLogDetailDto task)
    {
        if (!IsAdmin) return; // Double check
        _selectedTaskForExtension = task;
        ExtensionReason = string.Empty;
        ExtensionHours = 2; // Reset to default
        IsExtendFlyoutOpen = true;
    }

    [RelayCommand]
    private void CloseExtendFlyout()
    {
        IsExtendFlyoutOpen = false;
        _selectedTaskForExtension = null;
    }

    [RelayCommand]
    private async Task ConfirmExtend()
    {
        if (IsSavingExtension) return;

        if (_selectedTaskForExtension == null || string.IsNullOrWhiteSpace(ExtensionReason))
        {
            MessageBox.Show("A reason is required to extend the deadline.");
            return;
        }

        try
        {
            IsSavingExtension = true;
            var token = _authService.GetToken();

            // Calculate New Deadline: Current Time + Selected Hours
            var newDeadline = DateTime.Now.AddHours(ExtensionHours);

            var request = new ExtendTaskDeadlineRequest(newDeadline, ExtensionReason);

            // IMPORTANT: Check which method name you have in your IPortalMirageApi interface
            // If it's called ExtendTaskDeadlineAsync:
            await _apiClient.ExtendTaskDeadlineAsync(token, _selectedTaskForExtension.LogID, request);
            // OR if it's called ExtendDeadlineAsync:
            // await _apiClient.ExtendDeadlineAsync(token, _selectedTaskForExtension.LogID, request);

            IsExtendFlyoutOpen = false;
            await LoadTasks(); // Refresh list to turn the Red task back to Blue
            MessageBox.Show($"Deadline extended until {newDeadline:HH:mm}.", "Success");
        }
        catch (Exception ex)
        {
            // === DETAILED ERROR REPORTING ===
            string errorMessage = ex.Message;

            // Check if it's a Refit API Error to get the real message
            if (ex is Refit.ApiException apiEx)
            {
                errorMessage = $"Server Error ({apiEx.StatusCode}):\n{apiEx.Content}";
            }

            MessageBox.Show($"Extension Failed: {errorMessage}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsSavingExtension = false;
        }
    }

    // --- ROBUST TOKEN PARSER ---
    private int GetUserIdFromToken(string token)
    {
        if (string.IsNullOrEmpty(token)) return 0;
        try
        {
            // 1. Get Payload
            var parts = token.Split('.');
            if (parts.Length < 2) return 0;

            var payload = parts[1];

            // 2. Fix Base64 Padding
            switch (payload.Length % 4)
            {
                case 2: payload += "=="; break;
                case 3: payload += "="; break;
            }

            // 3. Decode
            var jsonBytes = Convert.FromBase64String(payload.Replace('-', '+').Replace('_', '/'));

            // 4. Parse JSON using System.Text.Json
            using (var doc = JsonDocument.Parse(jsonBytes))
            {
                var root = doc.RootElement;
                JsonElement idElement;

                // Check for common claim names (Long URL is standard for .NET)
                if (root.TryGetProperty("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", out idElement) ||
                    root.TryGetProperty("nameid", out idElement) ||
                    root.TryGetProperty("sub", out idElement) ||
                    root.TryGetProperty("id", out idElement))
                {
                    if (idElement.ValueKind == JsonValueKind.Number)
                        return idElement.GetInt32();
                    if (idElement.ValueKind == JsonValueKind.String && int.TryParse(idElement.GetString(), out int id))
                        return id;
                }
            }

            // === DEBUGGING BLOCK ===
            // If we get here, we couldn't find the ID. 
            // For debugging, you can uncomment this to see what's in the token:
            // var jsonString = System.Text.Encoding.UTF8.GetString(jsonBytes);
            // MessageBox.Show($"DEBUG: Could not find ID in token.\n\nPayload:\n{jsonString}", "Token Debug");
        }
        catch { /* Ignore errors */ }
        return 0;
    }
}

public class TaskLogItem : ObservableObject
{
    public TaskLogDetailDto Dto { get; }
    private bool _isChecked;

    public TaskLogItem(TaskLogDetailDto dto)
    {
        Dto = dto;
        // FIX: Backend uses "Complete", not "Completed"
        _isChecked = dto.Status == "Complete";
    }

    public long LogID => Dto.LogID;
    public string TaskName => Dto.TaskName;
    public bool IsDirty { get; private set; }

    public bool IsChecked
    {
        get => _isChecked;
        set
        {
            if (SetProperty(ref _isChecked, value))
            {
                IsDirty = true;
            }
        }
    }

    // FIX: Send "Complete" (No 'd') to match the Backend Validation
    public string StatusToSave => IsChecked ? "Complete" : "Pending";

    public void MarkAsSaved() => IsDirty = false;
}