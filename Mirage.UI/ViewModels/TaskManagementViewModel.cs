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

    // Form Properties
    [ObservableProperty]
    private string _newTaskName = string.Empty;

    [ObservableProperty]
    private ShiftResponse? _selectedShift;

    [ObservableProperty]
    private string _selectedScheduleType = "Daily";

    [ObservableProperty]
    private string? _scheduleValue; // Stores "Monday" or "15" (day of month)

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
}