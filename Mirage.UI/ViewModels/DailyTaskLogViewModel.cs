using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mirage.UI.Services;
using PortalMirage.Core.Dtos;
using PortalMirage.Core.Models;
using Refit;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace Mirage.UI.ViewModels;

public partial class DailyTaskLogViewModel : ObservableObject
{
    private readonly IPortalMirageApi _apiClient;
    private readonly IAuthService _authService;

    // DRAFT CONFIGURATION
    private const string DraftFileName = "draft_dailytask.json";

    [ObservableProperty]
    private bool _hasUnsavedDraft;
    [ObservableProperty]
    private DateTime _selectedDate = DateTime.Today;
    [ObservableProperty]
    private bool _isNaFlyoutOpen;
    [ObservableProperty]
    private TaskLogDetailDto? _selectedTaskForNa;
    [ObservableProperty]
    private string _naReason = string.Empty;

    // Add these properties for draft restoration
    [ObservableProperty]
    private string _draftComment = string.Empty;
    [ObservableProperty]
    private string _draftStatus = string.Empty;
    [ObservableProperty]
    private TaskLogDetailDto? _selectedTask;

    public ObservableCollection<TaskLogDetailDto> MorningTasks { get; } = new();
    public ObservableCollection<TaskLogDetailDto> EveningTasks { get; } = new();

    public DailyTaskLogViewModel(IPortalMirageApi apiClient, IAuthService authService)
    {
        _apiClient = apiClient;
        _authService = authService;

        // CHECK FOR DRAFT ON STARTUP
        if (File.Exists(DraftFileName))
        {
            try
            {
                var json = File.ReadAllText(DraftFileName);
                var draft = JsonSerializer.Deserialize<UpdateTaskStatusRequest>(json);

                if (draft != null)
                {
                    DraftStatus = draft.Status;
                    DraftComment = draft.Comment;

                    HasUnsavedDraft = true;
                    MessageBox.Show("We found an unsaved task update and restored the text.\nPlease select the correct task and submit again.", "Draft Restored", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch
            {
                try { File.Delete(DraftFileName); } catch { }
            }
        }

        LoadInitialTasks();
    }

    private async void LoadInitialTasks() => await LoadTasksAsync();

    [RelayCommand]
    private async System.Threading.Tasks.Task LoadTasksAsync()
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
    private async System.Threading.Tasks.Task SubmitUpdate()
    {
        // Validate required fields (Ensure a task is selected)
        if (SelectedTask == null || string.IsNullOrEmpty(DraftStatus))
        {
            MessageBox.Show("Please select a task and a status.");
            return;
        }

        // Create Request
        var request = new UpdateTaskStatusRequest(DraftStatus, DraftComment);
        var authToken = _authService.GetToken();
        if (string.IsNullOrEmpty(authToken)) return;

        try
        {
            // 1. Try API
            await _apiClient.UpdateTaskStatusAsync(authToken, SelectedTask.LogID, request);

            // 2. Success - Clear Form
            DraftComment = string.Empty;
            DraftStatus = string.Empty;
            SelectedTask = null;

            if (File.Exists(DraftFileName)) File.Delete(DraftFileName);
            HasUnsavedDraft = false;

            // Refresh List
            await LoadTasksAsync();
            MessageBox.Show("Task status updated successfully.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception)
        {
            // 3. Failure: Save Draft
            try
            {
                var json = JsonSerializer.Serialize(request);
                await File.WriteAllTextAsync(DraftFileName, json);
                HasUnsavedDraft = true;

                MessageBox.Show(
                    "Connection failed.\n\n" +
                    "This update has been saved as a draft.\n" +
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