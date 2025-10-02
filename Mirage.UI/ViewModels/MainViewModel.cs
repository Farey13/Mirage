using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mirage.UI.Views;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
namespace Mirage.UI.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private object? _currentView;

    private readonly Dictionary<Type, ObservableObject> _viewModelInstances = new();

    [ObservableProperty]
    private NavigationItem? _selectedMenuItem;

    public ObservableCollection<NavigationItem> MenuItems { get; } = new();
    public ObservableCollection<NavigationItem> OptionsMenuItems { get; } = new();

    public MainViewModel()
    {
        // Instantiate all our main ViewModels once and store them
        _viewModelInstances.Add(typeof(DashboardViewModel), new DashboardViewModel());
        _viewModelInstances.Add(typeof(LogbooksViewModel), new LogbooksViewModel());
        _viewModelInstances.Add(typeof(ReportsViewModel), new ReportsViewModel());
        _viewModelInstances.Add(typeof(AdminViewModel), new AdminViewModel());
        _viewModelInstances.Add(typeof(SettingsViewModel), new SettingsViewModel());

        // Create the menu items with your preferred emoji icons
        MenuItems.Add(new NavigationItem("🏠", "Dashboard", typeof(DashboardViewModel)));
        MenuItems.Add(new NavigationItem("📖", "Logbooks", typeof(LogbooksViewModel)));
        MenuItems.Add(new NavigationItem("📈", "Reports", typeof(ReportsViewModel)));
        MenuItems.Add(new NavigationItem("🛠️", "Admin", typeof(AdminViewModel)));

        OptionsMenuItems.Add(new NavigationItem("⚙️", "Settings", typeof(SettingsViewModel)));

        // Set the default starting page
        SelectedMenuItem = MenuItems[0];
    }

    // This method is called whenever a menu item is clicked.
    partial void OnSelectedMenuItemChanged(NavigationItem? value)
    {
        if (value?.ViewType != null)
        {
            // This is the simple and correct way: create a new instance of the View.
            CurrentView = Activator.CreateInstance(value.ViewType);

            // If the view we just created is the Dashboard, we tell its new ViewModel to load its data.
            // This ensures the dashboard refreshes every time you navigate to it.
            if (CurrentView is DashboardView dashboardView && dashboardView.DataContext is DashboardViewModel dvm)
            {
                // We use _ = to call the async method without waiting for it to finish.
                _ = dvm.LoadSummaryAsync();
            }
        }
    }

    [RelayCommand]
    private void NavigateTo(Type viewType)
    {
        if (viewType != null)
        {
            CurrentView = Activator.CreateInstance(viewType);
            SelectedMenuItem = null;
        }
    }
}