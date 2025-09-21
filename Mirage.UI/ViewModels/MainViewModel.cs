using System.Collections.ObjectModel;

namespace Mirage.UI.ViewModels;

public class MainViewModel
{
    public ObservableCollection<NavigationItem> MenuItems { get; } = new();
    public ObservableCollection<NavigationItem> OptionsMenuItems { get; } = new();

    public MainViewModel()
    {
        // Main navigation items
        MenuItems.Add(new NavigationItem("🏠", "Dashboard"));
        MenuItems.Add(new NavigationItem("📖", "Logbooks"));
        MenuItems.Add(new NavigationItem("📈", "Reports"));
        MenuItems.Add(new NavigationItem("🛠️", "Admin"));

        // Bottom navigation items
        OptionsMenuItems.Add(new NavigationItem("⚙️", "Settings"));
    }
}