using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mirage.UI.Services;
using PortalMirage.Core.Dtos;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace Mirage.UI.ViewModels;

public partial class SystemSettingsViewModel : ObservableObject
{
    private readonly IPortalMirageApi _apiClient;
    private readonly IAuthService _authService;
    private int _settingId; // To store the ID for updates

    [ObservableProperty]
    private int _inactivityTimeoutMinutes;

    public SystemSettingsViewModel(IPortalMirageApi apiClient, IAuthService authService)
    {
        _apiClient = apiClient;
        _authService = authService;
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task LoadSettingsAsync()
    {
        var authToken = _authService.GetToken();
        if (string.IsNullOrEmpty(authToken)) return;

        try
        {
            var setting = await _apiClient.GetSettingAsync(authToken, "InactivityTimeoutMinutes");
            if (int.TryParse(setting.Description, out int timeout))
            {
                InactivityTimeoutMinutes = timeout;
                _settingId = setting.ItemID;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to load system settings: {ex.Message}", "Error");
        }
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task SaveSettingsAsync()
    {
        var authToken = _authService.GetToken();
        if (string.IsNullOrEmpty(authToken)) return;

        try
        {
            var request = new UpdateAdminListItemRequest(_settingId, "InactivityTimeoutMinutes", InactivityTimeoutMinutes.ToString(), true);
            await _apiClient.UpdateListItemAsync(authToken, _settingId, request);
            MessageBox.Show("Settings saved successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to save settings: {ex.Message}", "Error");
        }
    }
}