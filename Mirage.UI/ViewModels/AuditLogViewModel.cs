using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mirage.UI.Services;
using PortalMirage.Core.Dtos;
using Refit;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Mirage.UI.ViewModels;

public partial class AuditLogViewModel : ObservableObject
{
    private readonly IPortalMirageApi _apiClient;
    private readonly IAuthService _authService;
    private List<AuditLogDto> _allLogs = new();

    public ObservableCollection<AuditLogDto> Logs { get; } = new();
    public ObservableCollection<string> AvailableModules { get; } = new();
    public ObservableCollection<string> AvailableActions { get; } = new();

    [ObservableProperty] private string? _selectedModule;
    [ObservableProperty] private string? _selectedAction;
    [ObservableProperty] private DateTime _startDate = DateTime.Today.AddDays(-7);
    [ObservableProperty] private DateTime _endDate = DateTime.Today;
    [ObservableProperty] private bool _isLoading;

    public bool HasNoResults => !IsLoading && !Logs.Any();

    public AuditLogViewModel(IPortalMirageApi apiClient, IAuthService authService)
    {
        _apiClient = apiClient;
        _authService = authService;
    }

    // --- THIS COMMAND IS NOW RESTORED ---
    [RelayCommand]
    private async System.Threading.Tasks.Task SearchAsync()
    {
        // This command is for the main "Search" button and calls the main loading method.
        await LoadLogsAsync(null);
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task LoadLogsAsync(string? preset)
    {
        IsLoading = true;
        OnPropertyChanged(nameof(HasNoResults));

        if (!string.IsNullOrEmpty(preset)) { SetDatePreset(preset); }

        if (StartDate > EndDate)
        {
            ShowMessageBox("Start date cannot be after end date.", "Invalid Date Range", MessageBoxImage.Warning);
            IsLoading = false;
            OnPropertyChanged(nameof(HasNoResults));
            return;
        }

        var authToken = _authService.GetToken();
        if (string.IsNullOrEmpty(authToken))
        {
            ShowMessageBox("Authentication token is missing. Please log in again.", "Auth Error", MessageBoxImage.Error);
            IsLoading = false;
            OnPropertyChanged(nameof(HasNoResults));
            return;
        }

        try
        {
            _allLogs = await _apiClient.GetAuditLogsAsync(authToken, StartDate, EndDate);
            PopulateFilters();
            ApplyFilters();
        }
        catch (ApiException ex) { ShowMessageBox($"API Error: {ex.StatusCode}\n{ex.Message}", "Error", MessageBoxImage.Error); }
        catch (Exception ex) { ShowMessageBox($"An unexpected error occurred: {ex.Message}", "Error", MessageBoxImage.Error); }
        finally
        {
            IsLoading = false;
            OnPropertyChanged(nameof(HasNoResults));
        }
    }

    private void SetDatePreset(string? preset)
    {
        switch (preset?.ToLower())
        {
            case "today": StartDate = DateTime.Today; EndDate = DateTime.Today; break;
            case "week": StartDate = DateTime.Today.AddDays(-7); EndDate = DateTime.Today; break;
            case "month": StartDate = DateTime.Today.AddDays(-30); EndDate = DateTime.Today; break;
        }
    }

    private void PopulateFilters()
    {
        var currentModule = SelectedModule;
        var currentAction = SelectedAction;

        AvailableModules.Clear();
        AvailableActions.Clear();
        AvailableModules.Add("All");
        AvailableActions.Add("All");

        if (_allLogs == null || !_allLogs.Any())
        {
            SelectedModule = "All";
            SelectedAction = "All";
            return;
        }

        var modules = _allLogs.Select(l => l.ModuleName).Where(m => m != null).Distinct().OrderBy(m => m);
        foreach (var module in modules) AvailableModules.Add(module!);

        var actions = _allLogs.Select(l => l.ActionType).Distinct().OrderBy(a => a);
        foreach (var action in actions) AvailableActions.Add(action);

        SelectedModule = currentModule != null && AvailableModules.Contains(currentModule) ? currentModule : "All";
        SelectedAction = currentAction != null && AvailableActions.Contains(currentAction) ? currentAction : "All";
    }

    private void ApplyFilters()
    {
        if (_allLogs == null) return;

        IEnumerable<AuditLogDto> filteredLogs = _allLogs;

        if (!string.IsNullOrEmpty(SelectedModule) && SelectedModule != "All")
        {
            filteredLogs = filteredLogs.Where(l => l.ModuleName == SelectedModule);
        }
        if (!string.IsNullOrEmpty(SelectedAction) && SelectedAction != "All")
        {
            filteredLogs = filteredLogs.Where(l => l.ActionType == SelectedAction);
        }

        Logs.Clear();
        foreach (var log in filteredLogs.OrderByDescending(l => l.Timestamp))
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
        SelectedModule = "All";
        SelectedAction = "All";
    }

    // --- UPDATED EXPORT TO CSV METHOD ---
    [RelayCommand]
    private async System.Threading.Tasks.Task ExportToCsv()
    {
        if (!Logs.Any())
        {
            ShowMessageBox("There is no data to export.", "Export", MessageBoxImage.Information);
            return;
        }

        try
        {
            // 1. Build the CSV content as a string
            var csvBuilder = new StringBuilder();
            csvBuilder.AppendLine("Timestamp,User,Module,Action,RecordID,Details"); // Header row

            foreach (var log in Logs)
            {
                // Helper function to safely format text for CSV
                Func<string?, string> sanitize = value =>
                {
                    if (string.IsNullOrEmpty(value)) return "";
                    var sanitized = value.Replace("\"", "\"\""); // Escape any double quotes
                    return $"\"{sanitized}\""; // Enclose the text in double quotes
                };

                var line = string.Join(",",
                    log.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
                    sanitize(log.UserFullName ?? "System"),
                    sanitize(log.ModuleName),
                    sanitize(log.ActionType),
                    log.RecordID,
                    sanitize(log.NewValue)
                );
                csvBuilder.AppendLine(line);
            }

            // 2. Create the folder path (e.g., C:\MirageReports\2025\10\11)
            var today = DateTime.Today;
            var directoryPath = Path.Combine("C:", "MirageReports", today.ToString("yyyy"), today.ToString("MM"), today.ToString("dd"));
            Directory.CreateDirectory(directoryPath); // This safely creates folders if they don't exist

            // 3. Create a unique file name
            var fileName = $"Audit_Log_{DateTime.Now:yyyy-MM-dd_HHmmss}.csv";
            var filePath = Path.Combine(directoryPath, fileName);

            // 4. Write the content to the file
            await File.WriteAllTextAsync(filePath, csvBuilder.ToString());

            // 5. Notify the user of success
            ShowMessageBox($"Successfully exported {Logs.Count} records.\n\nFile saved to:\n{filePath}", "Export Successful", MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            ShowMessageBox($"An error occurred during export: {ex.Message}", "Export Error", MessageBoxImage.Error);
        }
    }

    private void ShowMessageBox(string message, string caption, MessageBoxImage image)
    {
        Application.Current.Dispatcher.Invoke(() => { MessageBox.Show(message, caption, MessageBoxButton.OK, image); });
    }
}