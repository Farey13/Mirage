using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mirage.UI.Services;
using PortalMirage.Core.Dtos;
using Refit;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace Mirage.UI.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    public static string? AuthToken { get; set; }
    private readonly IPortalMirageApi _apiClient;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private int _pendingHandoversCount;

    [ObservableProperty]
    private int _unresolvedBreakdownsCount;

    [ObservableProperty]
    private int _pendingDailyTasksCount;

    public DashboardViewModel()
    {
        _apiClient = RestService.For<IPortalMirageApi>("https://localhost:7210");
        if (string.IsNullOrEmpty(AuthToken))
        {
            // Fallback to get token from another ViewModel if not set directly
            AuthToken = UserManagementViewModel.AuthToken;
        }
    }

    [RelayCommand]
    private async Task LoadSummaryAsync()
    {
        if (string.IsNullOrEmpty(AuthToken)) return;

        IsLoading = true;
        try
        {
            var summary = await _apiClient.GetDashboardSummaryAsync(AuthToken);
            PendingHandoversCount = summary.PendingHandoversCount;
            UnresolvedBreakdownsCount = summary.UnresolvedBreakdownsCount;
            PendingDailyTasksCount = summary.PendingDailyTasksCount;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to load dashboard summary: {ex.Message}", "Error");
        }
        finally
        {
            IsLoading = false;
        }
    }
}