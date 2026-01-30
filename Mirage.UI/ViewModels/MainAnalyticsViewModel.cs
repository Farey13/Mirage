using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mirage.UI.Services;
using PortalMirage.Core.Dtos;
using System.Collections.ObjectModel;

namespace Mirage.UI.ViewModels;

public partial class MainAnalyticsViewModel : ObservableObject
{
    private readonly IPortalMirageApi _apiClient;
    private readonly IAuthService _authService;

    [ObservableProperty] private DateTime _startDate = DateTime.Today.AddDays(-30);
    [ObservableProperty] private DateTime _endDate = DateTime.Today;
    [ObservableProperty] private string _selectedLogbook = "Daily Task Log";
    [ObservableProperty] private bool _isLoading;

    public ObservableCollection<AnalyticsSummaryDto> Kpis { get; } = new();
    public List<string> LogbookOptions { get; } = new()
    {
        "Daily Task Log",
        "Machine Breakdown",
        "Shift Handover",
        "Sample Storage"
    };

    // This event tells the View (XAML) when new data is ready to be drawn
    public event Action<AnalyticsReportDto>? RequestChartUpdate;

    public MainAnalyticsViewModel(IPortalMirageApi apiClient, IAuthService authService)
    {
        _apiClient = apiClient;
        _authService = authService;
        _ = RefreshDataAsync();
    }

    [RelayCommand]
    public async Task RefreshDataAsync()
    {
        IsLoading = true;
        try
        {
            var token = _authService.GetToken();
            if (string.IsNullOrEmpty(token)) return;

            var data = SelectedLogbook switch
            {
                "Daily Task Log" => await _apiClient.GetDailyTaskCompletionAsync(token, StartDate, EndDate),
                "Machine Breakdown" => await _apiClient.GetMachineDowntimeAsync(token, StartDate, EndDate),
                "Shift Handover" => await _apiClient.GetShiftHandoverAnalyticsAsync(token, StartDate, EndDate),
                "Sample Storage" => await _apiClient.GetSampleStorageAnalyticsAsync(token, StartDate, EndDate),
                _ => null
            };

            if (data != null)
            {
                Kpis.Clear();
                foreach (var kpi in data.Kpis) Kpis.Add(kpi);

                // Signal to the View to render the chart
                RequestChartUpdate?.Invoke(data);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Analytics Error: {ex.Message}");
        }
        finally { IsLoading = false; }
    }

    partial void OnSelectedLogbookChanged(string value) => _ = RefreshDataAsync();
}