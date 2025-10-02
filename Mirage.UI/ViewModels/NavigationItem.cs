using System;

namespace Mirage.UI.ViewModels;

public record NavigationItem(string Icon, string Label, Type DestinationViewModel)
{
    // This helper property automatically finds the correct View that matches the ViewModel
    // e.g., DashboardViewModel -> DashboardView
    public Type ViewType => Type.GetType(DestinationViewModel.FullName!.Replace("ViewModel", "View"))!;
}