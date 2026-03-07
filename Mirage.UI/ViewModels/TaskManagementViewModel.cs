using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mirage.UI.Services;
using PortalMirage.Core.Dtos;
using PortalMirage.Core.Models; // This contains your TaskModel and Shift
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Mirage.UI.ViewModels;

public partial class TaskManagementViewModel : ObservableObject
{
    private readonly IPortalMirageApi _apiClient;
    private readonly IAuthService _authService;

    // Collections for UI Binding
    public ObservableCollection<TaskModel> Tasks { get; } = new();
    public ObservableCollection<ShiftResponse> Shifts { get; } = new();

    public ObservableCollection<string> ScheduleTypes { get; } = new() { "Daily", "Weekly", "Monthly" };
    public ObservableCollection<string> DaysOfWeek { get; } = new()
    {
        "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday"
    };

    // Form Properties for Add
    [ObservableProperty]
    private string _newTaskName = string.Empty;

    [ObservableProperty]
    private ShiftResponse? _selectedShift;

    [ObservableProperty]
    private string _selectedScheduleType = "Daily";

    [ObservableProperty]
    private string? _scheduleValue;

    // Edit Panel Properties
    [ObservableProperty]
    private TaskModel? _selectedTask;

    [ObservableProperty]
    private bool _isEditMode;

    [ObservableProperty]
    private string _editTaskName = string.Empty;

    [ObservableProperty]
    private ShiftResponse? _editSelectedShift;

    [ObservableProperty]
    private string _editScheduleType = "Daily";

    [ObservableProperty]
    private string? _editScheduleValue;

    [ObservableProperty]
    private bool _editIsActive = true;

    public TaskManagementViewModel(IPortalMirageApi apiClient, IAuthService authService)
    {
        _apiClient = apiClient;
        _authService = authService;

        // Load data immediately upon creation
        _ = LoadDataAsync();
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task LoadDataAsync()
    {
        var token = _authService.GetToken();
        if (string.IsNullOrEmpty(token)) return;

        try
        {
            // Load Tasks and Shifts in parallel
            var tasksTask = _apiClient.GetAllTasksAsync(token);
            var shiftsTask = _apiClient.GetAllShiftsAsync(token);

            await System.Threading.Tasks.Task.WhenAll(tasksTask, shiftsTask);

            // Update UI Lists
            Tasks.Clear();
            foreach (var t in await tasksTask) Tasks.Add(t);

            Shifts.Clear();
            foreach (var s in await shiftsTask) Shifts.Add(s);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading data: {ex.Message}");
        }
    }


    [RelayCommand]
    private async Task DeleteTask(TaskModel task)
    {
        if (task == null) return;

        var confirm = MessageBox.Show($"Are you sure you want to delete '{task.TaskName}'?",
                                      "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);

        if (confirm == MessageBoxResult.Yes)
        {
            var token = _authService.GetToken();
            if (string.IsNullOrEmpty(token)) return;

            try
            {
                await _apiClient.DeactivateTaskAsync(token, task.TaskID);
                await LoadDataAsync(); // Refresh list
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to delete task: {ex.Message}");
            }
        }
    }


    [RelayCommand]
    private async System.Threading.Tasks.Task SaveTask()
    {
        if (string.IsNullOrWhiteSpace(NewTaskName))
        {
            MessageBox.Show("Task Name is required.");
            return;
        }

        if (SelectedShift == null)
        {
            MessageBox.Show("Please select a Shift.");
            return;
        }

        var token = _authService.GetToken();
        if (string.IsNullOrEmpty(token)) return;

        try
        {
            // Create object based on PortalMirage.Core.Models.TaskModel
            var newTask = new TaskModel
            {
                TaskID = 0, // DB handles ID
                TaskName = NewTaskName,
                ShiftID = SelectedShift.ShiftID,
                ScheduleType = SelectedScheduleType,
                ScheduleValue = ScheduleValue,
                IsActive = true
            };

            await _apiClient.CreateTaskAsync(token, newTask);

            // Clear inputs
            NewTaskName = string.Empty;
            ScheduleValue = null;

            // Refresh list
            await LoadDataAsync();
            MessageBox.Show("Task created successfully!");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to create task: {ex.Message}");
        }
    }

    [RelayCommand]
    private void SelectTask(TaskModel task)
    {
        if (task == null) return;

        SelectedTask = task;
        EditTaskName = task.TaskName;
        EditScheduleType = task.ScheduleType;
        EditScheduleValue = task.ScheduleValue;
        EditIsActive = task.IsActive;

        // Find the matching shift
        EditSelectedShift = Shifts.FirstOrDefault(s => s.ShiftID == task.ShiftID);

        IsEditMode = true;
    }

    [RelayCommand]
    private async Task SaveTaskUpdate()
    {
        if (SelectedTask == null) return;

        if (string.IsNullOrWhiteSpace(EditTaskName))
        {
            MessageBox.Show("Task Name is required.");
            return;
        }

        if (EditSelectedShift == null)
        {
            MessageBox.Show("Please select a Shift.");
            return;
        }

        var token = _authService.GetToken();
        if (string.IsNullOrEmpty(token)) return;

        try
        {
            var updatedTask = new TaskModel
            {
                TaskID = SelectedTask.TaskID,
                TaskName = EditTaskName,
                ShiftID = EditSelectedShift.ShiftID,
                ScheduleType = EditScheduleType,
                ScheduleValue = EditScheduleValue,
                IsActive = EditIsActive
            };

            await _apiClient.UpdateTaskAsync(token, updatedTask.TaskID, updatedTask);

            await LoadDataAsync();
            MessageBox.Show("Task updated successfully!");

            // Clear edit mode
            ClearEditMode();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to update task: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task SoftDeleteTask()
    {
        if (SelectedTask == null) return;

        var confirm = MessageBox.Show(
            $"Are you sure you want to soft delete '{SelectedTask.TaskName}'?\nThis action can be undone by restoring the task.",
            "Confirm Soft Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);

        if (confirm != MessageBoxResult.Yes) return;

        var token = _authService.GetToken();
        if (string.IsNullOrEmpty(token)) return;

        try
        {
            await _apiClient.SoftDeleteTaskAsync(token, SelectedTask.TaskID);
            await LoadDataAsync();
            MessageBox.Show("Task soft deleted successfully!");
            ClearEditMode();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to soft delete task: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task RestoreTask(TaskModel task)
    {
        if (task == null) return;

        var token = _authService.GetToken();
        if (string.IsNullOrEmpty(token)) return;

        try
        {
            await _apiClient.RestoreTaskAsync(token, task.TaskID);
            await LoadDataAsync();
            MessageBox.Show("Task restored successfully!");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to restore task: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task ToggleActive(TaskModel task)
    {
        if (task == null) return;

        var token = _authService.GetToken();
        if (string.IsNullOrEmpty(token)) return;

        try
        {
            var updatedTask = new TaskModel
            {
                TaskID = task.TaskID,
                TaskName = task.TaskName,
                ShiftID = task.ShiftID,
                ScheduleType = task.ScheduleType,
                ScheduleValue = task.ScheduleValue,
                IsActive = !task.IsActive
            };

            await _apiClient.UpdateTaskAsync(token, updatedTask.TaskID, updatedTask);
            await LoadDataAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to toggle task status: {ex.Message}");
        }
    }

    [RelayCommand]
    private void CancelEdit()
    {
        ClearEditMode();
    }

    private void ClearEditMode()
    {
        SelectedTask = null;
        IsEditMode = false;
        EditTaskName = string.Empty;
        EditSelectedShift = null;
        EditScheduleType = "Daily";
        EditScheduleValue = null;
        EditIsActive = true;
    }
}