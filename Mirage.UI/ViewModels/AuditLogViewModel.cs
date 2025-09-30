using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mirage.UI.Services;
using PortalMirage.Core.Dtos;
using Refit;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Mirage.UI.ViewModels;

public partial class AuditLogViewModel : ObservableObject
{
    public static string? AuthToken { get; set; }
    private readonly IPortalMirageApi _apiClient;
    private List<AuditLogDto> _allLogs = new();

    [ObservableProperty]
    private DateTime _startDate = DateTime.Today.AddDays(-7);

    [ObservableProperty]
    private DateTime _endDate = DateTime.Today;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string? _selectedModule;

    [ObservableProperty]
    private string? _selectedAction;

    public ObservableCollection<string> AvailableModules { get; } = new();
    public ObservableCollection<string> AvailableActions { get; } = new();
    public ObservableCollection<AuditLogDto> Logs { get; } = new();

    public bool HasNoResults => !IsLoading && !Logs.Any();

    public AuditLogViewModel()
    {
        _apiClient = RestService.For<IPortalMirageApi>("https://localhost:7210");

        // Enhanced token handling
        if (string.IsNullOrEmpty(AuthToken))
        {
            AuthToken = UserManagementViewModel.AuthToken;
        }

        // Set default dates - data will load when the view loads via the interaction trigger
        if (!string.IsNullOrEmpty(AuthToken))
        {
            SetDatePreset("week"); // Set the default date range only
        }
    }

    // COMMAND 1: For the "Search" button and initial load (no parameters)
    [RelayCommand]
    private async Task LoadLogsAsync()
    {
        if (string.IsNullOrEmpty(AuthToken))
        {
            ShowMessageBox("Authentication token is missing. Please log in again.", "Authentication Error", MessageBoxImage.Warning);
            return;
        }

        if (StartDate > EndDate)
        {
            ShowMessageBox("Start date cannot be after end date.", "Invalid Date Range", MessageBoxImage.Warning);
            return;
        }

        IsLoading = true;
        OnPropertyChanged(nameof(HasNoResults));

        try
        {
            _allLogs = await _apiClient.GetAuditLogsAsync(AuthToken, StartDate, EndDate);
            PopulateFilters();
            ApplyFilters();
        }
        catch (ApiException ex)
        {
            ShowMessageBox($"API Error: {ex.StatusCode}\n{ex.Message}", "Error", MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            ShowMessageBox($"An unexpected error occurred: {ex.Message}", "Error", MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
            OnPropertyChanged(nameof(HasNoResults));
        }
    }

    // COMMAND 2: For the "Today", "Last 7 Days", "Last 30 Days" buttons
    [RelayCommand]
    private async Task SetDatePresetAsync(string? preset)
    {
        SetDatePreset(preset);

        // Small delay to ensure property changes propagate to UI
        await Task.Delay(50);

        // After setting the dates, immediately execute the main load command
        await LoadLogsAsync();
    }

    private void SetDatePreset(string? preset)
    {
        if (string.IsNullOrEmpty(preset)) return;

        // Use null-conditional operator for safer string handling
        switch (preset?.ToLower())
        {
            case "today":
                StartDate = DateTime.Today;
                EndDate = DateTime.Today;
                break;
            case "week":
                StartDate = DateTime.Today.AddDays(-7);
                EndDate = DateTime.Today;
                break;
            case "month":
                StartDate = DateTime.Today.AddDays(-30);
                EndDate = DateTime.Today;
                break;
            default:
                // Handle unknown preset gracefully
                return;
        }

        // Force property change notifications
        OnPropertyChanged(nameof(StartDate));
        OnPropertyChanged(nameof(EndDate));
    }

    // Helper method to safely show message boxes from any thread
    private void ShowMessageBox(string message, string caption, MessageBoxImage image)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            MessageBox.Show(message, caption, MessageBoxButton.OK, image);
        });
    }

    // Add these partial methods to handle property changes
    partial void OnStartDateChanged(DateTime value)
    {
        // Optional: Add any logic needed when StartDate changes
    }

    partial void OnEndDateChanged(DateTime value)
    {
        // Optional: Add any logic needed when EndDate changes
    }

    private void PopulateFilters()
    {
        // Don't try to preserve selections; just repopulate the available options.
        AvailableModules.Clear();
        AvailableActions.Clear();

        var modules = _allLogs.Select(l => l.ModuleName).Where(m => m != null).Distinct().OrderBy(m => m);
        var actions = _allLogs.Select(l => l.ActionType).Distinct().OrderBy(a => a);

        foreach (var module in modules) AvailableModules.Add(module!);
        foreach (var action in actions) AvailableActions.Add(action);
    }

    private void ApplyFilters()
    {
        IEnumerable<AuditLogDto> filteredLogs = _allLogs;

        if (!string.IsNullOrEmpty(SelectedModule))
        {
            filteredLogs = filteredLogs.Where(l => l.ModuleName == SelectedModule);
        }
        if (!string.IsNullOrEmpty(SelectedAction))
        {
            filteredLogs = filteredLogs.Where(l => l.ActionType == SelectedAction);
        }

        Logs.Clear();
        foreach (var log in filteredLogs)
        {
            Logs.Add(log);
        }
        OnPropertyChanged(nameof(HasNoResults));
    }

    partial void OnSelectedModuleChanged(string? value) => ApplyFilters();
    partial void OnSelectedActionChanged(string? value) => ApplyFilters();

    [RelayCommand]
    private void ClearFilters()
    {
        SelectedModule = null;
        SelectedAction = null;
        // The OnSelectedModuleChanged and OnSelectedActionChanged partial methods
        // will automatically call ApplyFilters() when these properties are set.
    }

    [RelayCommand]
    private void ExportToCsv()
    {
        ShowMessageBox("Export to CSV feature will be implemented later.", "Coming Soon", MessageBoxImage.Information);
    }
}