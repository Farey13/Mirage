using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Mirage.UI.Services;
using Mirage.UI.Views;
using PortalMirage.Core.Dtos;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace Mirage.UI.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private object? _currentView;

    [ObservableProperty]
    private NavigationItem? _selectedMenuItem;

    [ObservableProperty]
    private NavigationItem? _selectedOptionsMenuItem;

    private readonly Dictionary<Type, object> _viewModelInstances = new();
    private readonly IAuthService _authService;
    private readonly IInactivityService _inactivityService;

    public ObservableCollection<NavigationItem> MenuItems { get; } = new();
    public ObservableCollection<NavigationItem> OptionsMenuItems { get; } = new();

    [ObservableProperty]
    private UserResponse? _currentUser;

    public MainViewModel(
        IAuthService authService,
        IInactivityService inactivityService,
        DashboardViewModel dashboardViewModel,
        LogbooksViewModel logbooksViewModel,
        ReportsViewModel reportsViewModel,
        AdminViewModel adminViewModel,
        SettingsViewModel settingsViewModel,
        RepeatSampleViewModel repeatSampleViewModel,
        KitValidationViewModel kitValidationViewModel,
        CalibrationLogViewModel calibrationLogViewModel,
        SampleStorageViewModel sampleStorageViewModel,
        HandoverViewModel handoverViewModel,
        MachineBreakdownViewModel machineBreakdownViewModel,
        MediaSterilityViewModel mediaSterilityViewModel,
        DailyTaskLogViewModel dailyTaskLogViewModel)
    {
        // --- ADD THESE LINES AT THE TOP OF THE CONSTRUCTOR ---
        _authService = authService;
        _inactivityService = inactivityService;
        _inactivityService.OnInactive += Logout;
        // ----------------------------------------------------

        // Add ALL ViewModels to our dictionary
        _viewModelInstances.Add(typeof(DashboardViewModel), dashboardViewModel);
        _viewModelInstances.Add(typeof(LogbooksViewModel), logbooksViewModel);
        _viewModelInstances.Add(typeof(ReportsViewModel), reportsViewModel);
        _viewModelInstances.Add(typeof(AdminViewModel), adminViewModel);
        _viewModelInstances.Add(typeof(SettingsViewModel), settingsViewModel);
        _viewModelInstances.Add(typeof(RepeatSampleViewModel), repeatSampleViewModel);
        _viewModelInstances.Add(typeof(KitValidationViewModel), kitValidationViewModel);
        _viewModelInstances.Add(typeof(CalibrationLogViewModel), calibrationLogViewModel);
        _viewModelInstances.Add(typeof(SampleStorageViewModel), sampleStorageViewModel);
        _viewModelInstances.Add(typeof(HandoverViewModel), handoverViewModel);
        _viewModelInstances.Add(typeof(MachineBreakdownViewModel), machineBreakdownViewModel);
        _viewModelInstances.Add(typeof(MediaSterilityViewModel), mediaSterilityViewModel);
        _viewModelInstances.Add(typeof(DailyTaskLogViewModel), dailyTaskLogViewModel);

        // --- Menu Items ---
        // These emojis should be universally available
        MenuItems.Add(new NavigationItem("🖼️", "Dashboard", typeof(DashboardViewModel)));
        MenuItems.Add(new NavigationItem("📚", "Logbooks", typeof(LogbooksViewModel)));
        MenuItems.Add(new NavigationItem("📊", "Reports", typeof(ReportsViewModel)));
        MenuItems.Add(new NavigationItem("⚙️", "Admin", typeof(AdminViewModel)));

        OptionsMenuItems.Add(new NavigationItem("🔧", "Settings", typeof(SettingsViewModel)));

        SelectedMenuItem = MenuItems[0];
    }

    partial void OnSelectedMenuItemChanged(NavigationItem? value)
    {
        if (value != null)
        {
            SelectedOptionsMenuItem = null;
            NavigateToView(value);
        }
    }

    partial void OnSelectedOptionsMenuItemChanged(NavigationItem? value)
    {
        if (value != null)
        {
            SelectedMenuItem = null;
            NavigateToView(value);
        }
    }

    private void NavigateToView(NavigationItem item)
    {
        if (_viewModelInstances.TryGetValue(item.DestinationViewModel, out var vm))
        {
            var viewType = Type.GetType(item.DestinationViewModel.FullName!.Replace("ViewModel", "View"));
            if (viewType != null && Activator.CreateInstance(viewType) is FrameworkElement view)
            {
                view.DataContext = vm;
                CurrentView = view;

                if (vm is DashboardViewModel dvm)
                {
                    _ = dvm.LoadSummaryAsync();
                }
            }
        }
    }

    [RelayCommand]
    private void NavigateTo(Type? viewType)
    {
        if (viewType is null) return;

        // --- THIS IS THE CORRECTED LOGIC ---
        // It correctly converts "Mirage.UI.Views.HandoverView"
        // to "Mirage.UI.ViewModels.HandoverViewModel"
        // without the old bug.
        var tempName = viewType.FullName!.Replace(".Views.", ".ViewModels.");
        var viewModelTypeName = tempName.Substring(0, tempName.Length - "View".Length) + "ViewModel";
        // ------------------------------------

        var viewModelType = Type.GetType(viewModelTypeName);

        if (viewModelType != null && _viewModelInstances.TryGetValue(viewModelType, out var vm))
        {
            if (Activator.CreateInstance(viewType) is FrameworkElement view)
            {
                view.DataContext = vm;
                CurrentView = view;

                SelectedMenuItem = null;
                SelectedOptionsMenuItem = null;
            }
        }
    }

    private void Logout()
    {
        // Make sure we run this on the UI thread
        Application.Current.Dispatcher.Invoke(() =>
        {
            // 1. Stop the timer so it doesn't fire again
            _inactivityService.StopTimer();

            // 2. Clear the authentication token
            _authService.SetToken(null);

            // 3. Open a new login window
            var loginView = App.ServiceProvider?.GetRequiredService<LoginView>();
            loginView?.Show();

            // 4. Find and close the current main window
            var currentMainWindow = Application.Current.Windows.OfType<MainWindow>().FirstOrDefault();
            currentMainWindow?.Close();
        });
    }
}