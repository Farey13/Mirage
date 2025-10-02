using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mirage.UI.Services;
using Mirage.UI.Views;
using PortalMirage.Core.Dtos;
using Refit;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Mirage.UI.ViewModels;

public partial class DashboardItem : ObservableObject
{
    [ObservableProperty]
    private string _label;

    [ObservableProperty]
    private int _count;

    [ObservableProperty]
    private string _icon;

    [ObservableProperty]
    private Brush _accentColor;

    [ObservableProperty]
    private Type _targetView;

    public DashboardItem(string label, string icon, Brush accentColor, Type targetView)
    {
        _label = label;
        _icon = icon;
        _accentColor = accentColor;
        _targetView = targetView;
    }
}

public partial class DashboardViewModel : ObservableObject
{
    public static string? AuthToken { get; set; }
    private readonly IPortalMirageApi _apiClient;

    [ObservableProperty]
    private bool _isLoading;

    public ObservableCollection<DashboardItem> DashboardItems { get; } = new();

    public DashboardViewModel()
    {
        _apiClient = RestService.For<IPortalMirageApi>("https://localhost:7210");
        if (string.IsNullOrEmpty(AuthToken))
        {
            AuthToken = UserManagementViewModel.AuthToken;
        }

        InitializeDashboardItems();

        // The event subscription and initial load call are REMOVED from here.
    }

    private void InitializeDashboardItems()
    {
        DashboardItems.Add(new DashboardItem("Pending Handovers", "\uE8AB", new SolidColorBrush((Color)ColorConverter.ConvertFromString("#007AFF")), typeof(HandoverView)));
        DashboardItems.Add(new DashboardItem("Unresolved Breakdowns", "\uE99A", new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF9500")), typeof(MachineBreakdownView)));
        DashboardItems.Add(new DashboardItem("Pending Daily Tasks", "\uECC8", new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF3B30")), typeof(DailyTaskLogView)));
    }

    public async Task LoadSummaryAsync()
    {
        if (string.IsNullOrEmpty(AuthToken)) return;

        IsLoading = true;
        try
        {
            var summary = await _apiClient.GetDashboardSummaryAsync(AuthToken);

            // Update dashboard items - UI will auto-update due to ObservableObject
            if (DashboardItems.Count >= 3)
            {
                DashboardItems[0].Count = summary.PendingHandoversCount;
                DashboardItems[1].Count = summary.UnresolvedBreakdownsCount;
                DashboardItems[2].Count = summary.PendingDailyTasksCount;
            }
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
