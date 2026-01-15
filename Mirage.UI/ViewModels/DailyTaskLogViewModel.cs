using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mirage.UI.Services;
using PortalMirage.Core.Dtos;
using PortalMirage.Core.Models;
using Refit;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
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
    private bool _showPendingOnly;
    [ObservableProperty]
    private int _pendingCount;
    [ObservableProperty]
    private string _shiftInfoText = "Loading...";

    [ObservableProperty]
    private bool _isAdmin;
    [ObservableProperty]
    private bool _isExtendFlyoutOpen;
    [ObservableProperty]
    private string _extensionReason = string.Empty;
    [ObservableProperty]
    private int _extensionHours = 2;
    [ObservableProperty]
    private bool _isSavingExtension;
    private TaskLogDetailDto? _selectedTaskForExtension;

    [ObservableProperty]
    private bool _isNaFlyoutOpen;
    [ObservableProperty]
    private TaskLogDetailDto? _selectedTaskForNa;
    [ObservableProperty]
    private string _naReason = string.Empty;
    [ObservableProperty]
    private bool _isSavingNa;
    [ObservableProperty]
    private bool _isSaving;

    public ObservableCollection<TaskLogItem> MorningTasks { get; } = new();
    public ObservableCollection<TaskLogItem> EveningTasks { get; } = new();
    public ObservableCollection<TaskLogItem> NightTasks { get; } = new();

    public DailyTaskLogViewModel(IPortalMirageApi apiClient, IAuthService authService)
    {
        _apiClient = apiClient;
        _authService = authService;
        LoadInitialTasks();
    }

    private async void LoadInitialTasks() => await LoadTasks();

    [RelayCommand]
    private void PreviousDay()
    {
        SelectedDate = SelectedDate.AddDays(-1);
        LoadTasksCommand.Execute(null);
    }

    [RelayCommand]
    private void NextDay()
    {
        SelectedDate = SelectedDate.AddDays(1);
        LoadTasksCommand.Execute(null);
    }

    partial void OnShowPendingOnlyChanged(bool value)
    {
        LoadTasksCommand.Execute(null);
    }

    [RelayCommand]
    private async Task LoadTasks()
    {
        if (IsSaving) return;

        try
        {
            IsSaving = true;
            var token = _authService.GetToken();
            if (string.IsNullOrEmpty(token)) return;

            var currentUser = _authService.CurrentUser;
            IsAdmin = currentUser?.Role == "Admin";

            UpdateShiftInfo();

            var tasks = await _apiClient.GetTasksForDateAsync(token, SelectedDate);

            MorningTasks.Clear();
            EveningTasks.Clear();
            NightTasks.Clear();

            int pCount = 0;

            if (tasks != null)
            {
                foreach (var t in tasks)
                {
                    if (ShowPendingOnly && t.Status == "Complete") continue;

                    var item = new TaskLogItem(t, SelectedDate, IsAdmin);

                    if (item.StatusToSave == "Pending") pCount++;

                    if (string.Equals(t.TaskCategory, "Morning", StringComparison.OrdinalIgnoreCase))
                        MorningTasks.Add(item);
                    else if (string.Equals(t.TaskCategory, "Evening", StringComparison.OrdinalIgnoreCase))
                        EveningTasks.Add(item);
                    else
                        NightTasks.Add(item);
                }
            }
            PendingCount = pCount;
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

    private void UpdateShiftInfo()
    {
        if (SelectedDate.Date == DateTime.Today)
            ShiftInfoText = "📅 Today's Log - Grace Periods Active";
        else if (SelectedDate.Date < DateTime.Today)
            ShiftInfoText = "🔒 Past Date - Log Locked (Admin Override Required)";
        else
            ShiftInfoText = "📅 Future Date";
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

            var userId = _authService.CurrentUser?.UserID ?? 0;
            if (userId <= 0) userId = GetUserIdFromToken(token);
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
                if (!item.IsEditable)
                {
                    MessageBox.Show($"Task '{item.TaskName}' is locked and cannot be modified.", "Locked Task", MessageBoxButton.OK, MessageBoxImage.Warning);
                    continue;
                }

                var request = new UpdateTaskStatusRequest
                {
                    Status = item.StatusToSave,
                    UserId = userId,
                    Comment = null
                };

                await _apiClient.UpdateTaskStatusAsync(token, item.LogID, request);
                item.MarkAsSaved();
            }

            MessageBox.Show("Changes saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            await LoadTasks();
        }
        catch (Exception ex)
        {
            string errorMessage = ex.Message;
            if (ex is Refit.ApiException apiEx)
            {
                errorMessage = $"Server Error ({apiEx.StatusCode}):\n{apiEx.Content}";
            }
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
        var item = FindTaskItem(task.LogID);
        if (item != null && !item.IsEditable)
        {
            MessageBox.Show("This task is locked and cannot be marked as N/A.", "Locked Task", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

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
            if (string.IsNullOrEmpty(token)) return;

            var userId = _authService.CurrentUser?.UserID ?? 0;
            if (userId <= 0) userId = GetUserIdFromToken(token);
            if (userId <= 0) userId = 1;

            var request = new UpdateTaskStatusRequest
            {
                Status = "Not Available",
                UserId = userId,
                Comment = NaReason
            };

            await _apiClient.UpdateTaskStatusAsync(token, SelectedTaskForNa.LogID, request);

            IsNaFlyoutOpen = false;
            NaReason = string.Empty;
            await LoadTasks();
        }
        catch (Exception ex)
        {
            string errorMessage = ex.Message;
            if (ex is Refit.ApiException apiEx)
            {
                errorMessage = $"Server Error ({apiEx.StatusCode}):\n{apiEx.Content}";
            }
            MessageBox.Show($"N/A Update Failed: {errorMessage}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsSavingNa = false;
        }
    }

    [RelayCommand]
    private void ShowExtendFlyout(TaskLogDetailDto task)
    {
        if (!IsAdmin) return;
        _selectedTaskForExtension = task;
        ExtensionReason = string.Empty;
        ExtensionHours = 2;
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
            var newDeadline = DateTime.Now.AddHours(ExtensionHours);
            var request = new ExtendTaskDeadlineRequest(newDeadline, ExtensionReason);

            await _apiClient.ExtendTaskDeadlineAsync(token, _selectedTaskForExtension.LogID, request);

            IsExtendFlyoutOpen = false;
            await LoadTasks();
            MessageBox.Show($"Deadline extended until {newDeadline:HH:mm}.", "Success");
        }
        catch (Exception ex)
        {
            string errorMessage = ex.Message;
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

    private TaskLogItem? FindTaskItem(long logId)
    {
        return MorningTasks.Concat(EveningTasks).Concat(NightTasks)
                           .FirstOrDefault(t => t.LogID == logId);
    }

    private int GetUserIdFromToken(string token)
    {
        if (string.IsNullOrEmpty(token)) return 0;
        try
        {
            var parts = token.Split('.');
            if (parts.Length < 2) return 0;

            var payload = parts[1];
            switch (payload.Length % 4)
            {
                case 2: payload += "=="; break;
                case 3: payload += "="; break;
            }

            var jsonBytes = Convert.FromBase64String(payload.Replace('-', '+').Replace('_', '/'));
            using var doc = JsonDocument.Parse(jsonBytes);
            var root = doc.RootElement;
            JsonElement idElement;

            if (root.TryGetProperty("nameid", out idElement) || root.TryGetProperty("sub", out idElement))
            {
                if (idElement.ValueKind == JsonValueKind.Number) return idElement.GetInt32();
                if (idElement.ValueKind == JsonValueKind.String && int.TryParse(idElement.GetString(), out int id)) return id;
            }
        }
        catch { }
        return 0;
    }
}

public class TaskLogItem : ObservableObject
{
    public TaskLogDetailDto Dto { get; }
    private readonly DateTime _logDate;
    private readonly bool _isAdmin;
    private bool _isChecked;

    public TaskLogItem(TaskLogDetailDto dto, DateTime logDate, bool isAdmin)
    {
        Dto = dto;
        _logDate = logDate;
        _isAdmin = isAdmin;
        _isChecked = dto.Status == "Complete";
    }

    public long LogID => Dto.LogID;
    public string TaskName => Dto.TaskName;
    public bool IsDirty { get; private set; }
    public bool IsAdminUser => _isAdmin;

    public bool IsChecked
    {
        get => _isChecked;
        set { if (SetProperty(ref _isChecked, value)) IsDirty = true; }
    }

    public string StatusToSave => IsChecked ? "Complete" : "Pending";

    public bool IsLocked
    {
        get
        {
            bool isPastDate = _logDate.Date < DateTime.Today;
            bool hasOverride = Dto.LockOverrideUntil.HasValue && Dto.LockOverrideUntil.Value > DateTime.Now;

            if (isPastDate && !hasOverride) return true;
            if (Dto.Status == "Incomplete" || Dto.Status == "Not Available") return true;

            return false;
        }
    }

    public bool IsEditable => !IsLocked;

    public string AuditTooltip
    {
        get
        {
            if (Dto.Status == "Complete")
            {
                var who = Dto.CompletedByUsername ?? "Unknown";
                var when = Dto.CompletedDateTime.HasValue ? Dto.CompletedDateTime.Value.ToString("HH:mm") : "?";
                return $"✓ Completed by {who} at {when}";
            }
            if (Dto.Status == "Incomplete") return "❌ Expired / Incomplete";
            if (Dto.Status == "Not Available") return $"⚠️ N/A: {Dto.Comments}";
            return "Pending Completion";
        }
    }

    public void MarkAsSaved() => IsDirty = false;
}