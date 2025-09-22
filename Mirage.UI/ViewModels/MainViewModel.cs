using CommunityToolkit.Mvvm.ComponentModel;
using Mirage.UI.Views;
using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.Input;

namespace Mirage.UI.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private object? _currentView;

    [ObservableProperty]
    private NavigationItem? _selectedMenuItem;

    public ObservableCollection<NavigationItem> MenuItems { get; } = new();
    public ObservableCollection<NavigationItem> OptionsMenuItems { get; } = new();

    public MainViewModel()
    {
        // Main navigation items
        MenuItems.Add(new NavigationItem("🏠", "Dashboard", typeof(DashboardView)));
        MenuItems.Add(new NavigationItem("📖", "Logbooks", typeof(LogbooksView)));
        MenuItems.Add(new NavigationItem("📈", "Reports", typeof(ReportsView)));
        MenuItems.Add(new NavigationItem("🛠️", "Admin", typeof(AdminView)));

        // Bottom navigation items
        OptionsMenuItems.Add(new NavigationItem("⚙️", "Settings", typeof(SettingsView)));

        // Set the default starting page
        SelectedMenuItem = MenuItems[0];
    }

    partial void OnSelectedMenuItemChanged(NavigationItem? value)
    {
        if (value?.DestinationViewModel is not null)
        {
            // Create an instance of the page and set it as the current view
            CurrentView = Activator.CreateInstance(value.DestinationViewModel);
        }
    }

    [RelayCommand]
    private void NavigateTo(Type destination)
    {
        if (destination is not null)
        {
            CurrentView = Activator.CreateInstance(destination);
        }
    }
}
