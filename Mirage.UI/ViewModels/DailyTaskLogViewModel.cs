using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mirage.UI.Services;
using PortalMirage.Core.Dtos;
using PortalMirage.Core.Models;
using Refit;
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

    public ObservableCollection<TaskLogDetailDto> MorningTasks { get; } = new();
    public ObservableCollection<TaskLogDetailDto> EveningTasks { get; } = new();

    public DailyTaskLogViewModel(IPortalMirageApi apiClient, IAuthService authService)
    {
        _apiClient = apiClient;
        _authService = authService;
        
    }

    private async void LoadInitialTasks() => await LoadTasksAsync();

    [RelayCommand]
    private async System.Threading.Tasks. Task LoadTasksAsync()
    {
        var authToken = _authService.GetToken();
        if (string.IsNullOrEmpty(authToken)) return;

        try
        {
            var tasks = await _apiClient.GetTasksForDateAsync(authToken, SelectedDate);
            MorningTasks.Clear();
            EveningTasks.Clear();
            foreach (var task in tasks.Where(t => t.TaskCategory == "Morning")) MorningTasks.Add(task);
            foreach (var task in tasks.Where(t => t.TaskCategory == "Evening")) EveningTasks.Add(task);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to load tasks: {ex.Message}");
        }
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task ToggleCompletedStatus(TaskLogDetailDto task)
    {
        var authToken = _authService.GetToken();
        if (string.IsNullOrEmpty(authToken) || task is null) return;

        var newStatus = task.Status == "Completed" ? "Pending" : "Completed";

        try
        {
            var request = new UpdateTaskStatusRequest(newStatus, task.Comments);
            await _apiClient.UpdateTaskStatusAsync(authToken, task.LogID, request);
            await LoadTasksAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to update status: {ex.Message}");
        }
    }

    [RelayCommand]
    private void ShowNaFlyout(TaskLogDetailDto task)
    {
        SelectedTaskForNa = task;
        NaReason = string.Empty;
        IsNaFlyoutOpen = true;
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task ConfirmNa()
    {
        if (SelectedTaskForNa is null || string.IsNullOrWhiteSpace(NaReason))
        {
            MessageBox.Show("A reason is required to mark as N/A.");
            return;
        }

        var authToken = _authService.GetToken();
        if (string.IsNullOrEmpty(authToken)) return;

        try
        {
            var request = new UpdateTaskStatusRequest("Not Available", NaReason);
            await _apiClient.UpdateTaskStatusAsync(authToken, SelectedTaskForNa.LogID, request);
            IsNaFlyoutOpen = false;
            await LoadTasksAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to update status: {ex.Message}");
        }
    }
}