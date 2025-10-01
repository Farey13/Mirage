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

// A new class to represent a single dashboard widget
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

    [ObservableProperty]
    private string? _subtitle;

    [ObservableProperty]
    private string? _trendIcon;

    [ObservableProperty]
    private Brush _trendColor;

    [ObservableProperty]
    private bool _hasTrend;

    public DashboardItem(string label, string icon, Brush accentColor, Type targetView)
    {
        _label = label;
        _icon = icon;
        _accentColor = accentColor;
        _targetView = targetView;
        _trendColor = Brushes.Gray;
        _hasTrend = false;
    }
}

public partial class DashboardViewModel : ObservableObject
{
    public static string? AuthToken { get; set; }
    private readonly IPortalMirageApi _apiClient;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private DateTime _lastUpdated;

    // Keep individual properties for backward compatibility if needed
    [ObservableProperty]
    private int _pendingHandoversCount;

    [ObservableProperty]
    private int _unresolvedBreakdownsCount;

    [ObservableProperty]
    private int _pendingDailyTasksCount;

    public ObservableCollection<DashboardItem> DashboardItems { get; } = new();

    public DashboardViewModel()
    {
        _apiClient = RestService.For<IPortalMirageApi>("https://localhost:7210");
        if (string.IsNullOrEmpty(AuthToken))
        {
            // Fallback to get token from another ViewModel if not set directly
            AuthToken = UserManagementViewModel.AuthToken;
        }

        InitializeDashboardItems();
    }

    private void InitializeDashboardItems()
    {
        DashboardItems.Add(new DashboardItem("Pending Handovers", "\uE8AB", new SolidColorBrush((Color)ColorConverter.ConvertFromString("#007AFF")), typeof(HandoverView)));
        DashboardItems.Add(new DashboardItem("Unresolved Breakdowns", "\uE99A", new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF9500")), typeof(MachineBreakdownView)));
        DashboardItems.Add(new DashboardItem("Pending Daily Tasks", "\uECC8", new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF3B30")), typeof(DailyTaskLogView)));
    }

    [RelayCommand]
    private async Task LoadSummaryAsync()
    {
        if (string.IsNullOrEmpty(AuthToken)) return;

        IsLoading = true;
        try
        {
            var summary = await _apiClient.GetDashboardSummaryAsync(AuthToken);

            // Update both individual properties and dashboard items for flexibility
            PendingHandoversCount = summary.PendingHandoversCount;
            UnresolvedBreakdownsCount = summary.UnresolvedBreakdownsCount;
            PendingDailyTasksCount = summary.PendingDailyTasksCount;

            // Update the counts on the existing dashboard items
            if (DashboardItems.Count >= 3)
            {
                DashboardItems[0].Count = summary.PendingHandoversCount;
                DashboardItems[1].Count = summary.UnresolvedBreakdownsCount;
                DashboardItems[2].Count = summary.PendingDailyTasksCount;

                // Optional: Add trend indicators based on previous data
                // You could implement logic here to compare with previous values
                // and set trend icons (↑ ↓) and colors (Green/Red)
            }

            LastUpdated = DateTime.Now;
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