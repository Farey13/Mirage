using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mirage.UI.Services;
using PortalMirage.Core.Dtos;
using PortalMirage.Core.Models; // Required for TaskLogDetailDto
using Refit;
using System;
using System.Collections.ObjectModel;
using System.Linq;
// No need to add 'using System.Threading.Tasks;' as we will fully qualify it
using System.Windows;

namespace Mirage.UI.ViewModels;

public partial class DailyTaskLogViewModel : ObservableObject
{
    public static string? AuthToken { get; set; }
    private readonly IPortalMirageApi _apiClient;

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

    public DailyTaskLogViewModel()
    {
        _apiClient = RestService.For<IPortalMirageApi>("https://localhost:7210");
        LoadInitialTasks();
    }

    private async void LoadInitialTasks() => await LoadTasksAsync();

    [RelayCommand]
    private async System.Threading.Tasks.Task LoadTasksAsync()
    {
        if (string.IsNullOrEmpty(AuthToken)) return;
        try
        {
            var tasks = await _apiClient.GetTasksForDateAsync(AuthToken, SelectedDate);
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
        if (string.IsNullOrEmpty(AuthToken) || task is null) return;
        var newStatus = task.Status == "Completed" ? "Pending" : "Completed";
        try
        {
            var request = new UpdateTaskStatusRequest(newStatus, task.Comments);
            await _apiClient.UpdateTaskStatusAsync(AuthToken, task.LogID, request);
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
        if (string.IsNullOrEmpty(AuthToken)) return;
        try
        {
            var request = new UpdateTaskStatusRequest("NotApplicable", NaReason); // Corrected status to "NotApplicable"
            await _apiClient.UpdateTaskStatusAsync(AuthToken, SelectedTaskForNa.LogID, request);
            IsNaFlyoutOpen = false;
            await LoadTasksAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to update status: {ex.Message}");
        }
    }
}