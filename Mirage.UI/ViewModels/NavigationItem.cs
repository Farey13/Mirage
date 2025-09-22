using System;

namespace Mirage.UI.ViewModels;

public record NavigationItem(string Icon, string Label, Type DestinationViewModel);