using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace Mirage.UI.ViewModels;

// Helper record to define a tab
public record AdminTabItem(string Header, string Icon, object ViewModel);

public partial class AdminViewModel : ObservableObject
{
    public ObservableCollection<AdminTabItem> TabItems { get; } = new();

    [ObservableProperty]
    private AdminTabItem? _selectedTab;

    // The constructor now receives all the ViewModels for the tabs it will manage
    public AdminViewModel(
        UserManagementViewModel userManagementViewModel,
        ShiftManagementViewModel shiftManagementViewModel,
        MasterListViewModel masterListViewModel,
        AuditLogViewModel auditLogViewModel)
    {
        // Create the list of tabs to be displayed
        TabItems.Add(new AdminTabItem("Users", "👥", userManagementViewModel));
        TabItems.Add(new AdminTabItem("Shifts", "🕒", shiftManagementViewModel));
        TabItems.Add(new AdminTabItem("Master Lists", "📋", masterListViewModel));
        TabItems.Add(new AdminTabItem("Audit Log", "📜", auditLogViewModel));

        // Select the first tab by default
        _selectedTab = TabItems[0];
    }
}