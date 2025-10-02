using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mirage.UI.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace Mirage.UI.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private object? _currentView;

    [ObservableProperty]
    private NavigationItem? _selectedMenuItem;

    // ADDED: New property for the bottom menu items (Settings)
    [ObservableProperty]
    private NavigationItem? _selectedOptionsMenuItem;

    private readonly Dictionary<Type, ObservableObject> _viewModelInstances = new();

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

        // ADDED: Options menu items (bottom menu)
        OptionsMenuItems.Add(new NavigationItem("⚙️", "Settings", typeof(SettingsViewModel)));

        // Set the default starting page
        SelectedMenuItem = MenuItems[0];
    }

    // This method handles clicks on the TOP menu items
    partial void OnSelectedMenuItemChanged(NavigationItem? value)
    {
        // When a main item is selected, clear the options item selection
        if (value != null)
        {
            SelectedOptionsMenuItem = null;
            NavigateToView(value);
        }
    }

    // ADDED: This method handles clicks on the BOTTOM menu items
    partial void OnSelectedOptionsMenuItemChanged(NavigationItem? value)
    {
        // When an options item is selected, clear the main item selection
        if (value != null)
        {
            SelectedMenuItem = null;
            NavigateToView(value);
        }
    }

    // REFACTORED: Unified navigation logic for both menu types
    private void NavigateToView(NavigationItem item)
    {
        if (_viewModelInstances.TryGetValue(item.DestinationViewModel, out var vm))
        {
            // Create a new instance of the View
            if (Activator.CreateInstance(item.ViewType) is FrameworkElement view)
            {
                // Set its DataContext to our persistent ViewModel
                view.DataContext = vm;
                CurrentView = view;
            }

            // If the new view is the dashboard, tell its ViewModel to load data.
            if (vm is DashboardViewModel dvm)
            {
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
            SelectedOptionsMenuItem = null;
        }
    }
}