using CommunityToolkit.Mvvm.ComponentModel;
using ControlzEx.Theming;
using MahApps.Metro.Theming;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Mirage.UI.Properties;

namespace Mirage.UI.ViewModels;

// We create a simple helper record to display the color names and their visual swatch in the UI.
public record AccentColor(string Name, Brush ColorBrush);

public partial class SettingsViewModel : ObservableObject
{
    [ObservableProperty]
    private AccentColor? _selectedAccent;

    [ObservableProperty]
    private string? _selectedTheme;

    public ObservableCollection<AccentColor> AccentColors { get; }
    public List<string> AppThemes { get; } = new() { "Light", "Dark" };

    public SettingsViewModel()
    {
        // Populate the list of available accent colors from the MahApps ThemeManager
        AccentColors = new ObservableCollection<AccentColor>(
            ThemeManager.Current.Themes
                .GroupBy(x => x.ColorScheme)
                .OrderBy(a => a.Key)
                .Select(a => new AccentColor(a.Key, a.First().ShowcaseBrush))
        );

        // Detect the application's current theme and set the properties so the UI is in sync
        var currentTheme = ThemeManager.Current.DetectTheme(Application.Current);
        if (currentTheme != null)
        {
            _selectedTheme = currentTheme.BaseColorScheme;
            _selectedAccent = AccentColors.FirstOrDefault(a => a.Name == currentTheme.ColorScheme);
        }
    }

    // This method is automatically called by the MVVM Toolkit when the SelectedAccent property changes.
    partial void OnSelectedAccentChanged(AccentColor? value)
    {
        if (value != null)
        {
            // Get the current theme and apply the new accent color
            var currentTheme = ThemeManager.Current.DetectTheme(Application.Current);
            if (currentTheme != null)
            {
                ThemeManager.Current.ChangeTheme(Application.Current, $"{currentTheme.BaseColorScheme}.{value.Name}");
            }
        }
    }

    // This property gets and sets the font size directly from the application settings
    public double AppFontSize
    {
        get => Properties.Settings.Default.AppFontSize;
        set
        {
            if (Properties.Settings.Default.AppFontSize != value)
            {
                Properties.Settings.Default.AppFontSize = value;
                OnPropertyChanged(); // Notify the UI that the value has changed
                Properties.Settings.Default.Save(); // Save the setting to disk
            }
        }
    }

    // This method is automatically called when the SelectedTheme property changes.
    partial void OnSelectedThemeChanged(string? value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            // Get the current theme and apply the new base theme (Light/Dark)
            var currentTheme = ThemeManager.Current.DetectTheme(Application.Current);
            if (currentTheme != null)
            {
                ThemeManager.Current.ChangeTheme(Application.Current, $"{value}.{currentTheme.ColorScheme}");
            }
        }
    }
}