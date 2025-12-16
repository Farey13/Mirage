using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mirage.UI.Services;
using PortalMirage.Core.Dtos;
using PortalMirage.Core.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
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
            var userId = _authService.CurrentUser?.UserID ?? 0;

            if (userId <= 0)
            {
                MessageBox.Show("Error: User not identified. Please log out and log in again.");
                return;
            }

            var allModified = MorningTasks.Concat(EveningTasks).Concat(NightTasks)
                                          .Where(t => t.IsDirty).ToList();

            if (!allModified.Any())
            {
                MessageBox.Show("No changes to save.");
                return;
            }

            foreach (var item in allModified)
            {
                await _apiClient.UpdateTaskStatusAsync(token, item.LogID, item.StatusToSave, userId, null);
                item.MarkAsSaved();
            }
            MessageBox.Show("Changes saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to save: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
            var userId = _authService.CurrentUser?.UserID ?? 0;

            if (string.IsNullOrEmpty(token) || userId <= 0) return;

            await _apiClient.UpdateTaskStatusAsync(token, SelectedTaskForNa.LogID, "Not Available", userId, NaReason);

            IsNaFlyoutOpen = false;
            NaReason = string.Empty;
            await LoadTasks();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to update status: {ex.Message}");
        }
        finally
        {
            IsSavingNa = false;
        }
    }
}

public class TaskLogItem : ObservableObject
{
    public TaskLogDetailDto Dto { get; }
    private bool _isChecked;

    public TaskLogItem(TaskLogDetailDto dto)
    {
        Dto = dto;
        _isChecked = dto.Status == "Completed";
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

    public string StatusToSave => IsChecked ? "Completed" : "Pending";

    public void MarkAsSaved() => IsDirty = false;
}