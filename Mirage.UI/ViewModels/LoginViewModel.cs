using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mirage.UI.Services;
using Mirage.UI.Views;
using Microsoft.Extensions.DependencyInjection;
using PortalMirage.Core.Dtos;
using Refit;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Mirage.UI.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IPortalMirageApi _apiClient;
    private readonly IAuthService _authService; // It now uses our service
    private readonly MainViewModel _mainViewModel; // Added for setting current user

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isLoginInProgress;

    // The constructor now receives the services it needs
    public LoginViewModel(IPortalMirageApi apiClient, IAuthService authService, MainViewModel mainViewModel)
    {
        _apiClient = apiClient;
        _authService = authService;
        _mainViewModel = mainViewModel; // Store the MainViewModel reference
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task LoginAsync(object? parameter)
    {
        if (parameter is not System.Windows.Controls.PasswordBox passwordBox) return;
        var password = passwordBox.Password;

        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(password))
        {
            ErrorMessage = "Username and password are required.";
            return;
        }

        IsLoginInProgress = true;
        ErrorMessage = string.Empty;
        try
        {
            var loginRequest = new LoginRequest(Username, password);
            var loginResponse = await _apiClient.LoginAsync(loginRequest);

            // 1. Set the token and user info first
            _authService.SetToken($"Bearer {loginResponse.Token}");
            _mainViewModel.CurrentUser = loginResponse.User;

            // 2. Get the MainWindow and show it so the UI appears instantly
            var mainWindow = App.ServiceProvider?.GetRequiredService<MainWindow>();
            mainWindow?.Show();

            // 3. NOW, get the DashboardViewModel and tell it to load its data
            var dashboardViewModel = App.ServiceProvider?.GetRequiredService<DashboardViewModel>();
            if (dashboardViewModel != null)
            {
                // We don't wait for this to finish; the UI is already visible and the dashboard will update when ready
                _ = dashboardViewModel.LoadSummaryAsync();
            }

            // 4. Close the login window
            var loginWindow = Application.Current.Windows.OfType<LoginView>().FirstOrDefault();
            loginWindow?.Close();
        }
        catch (ApiException ex)
        {
            ErrorMessage = $"Login failed. Please check your credentials. (Error: {ex.StatusCode})";
        }
        catch (Exception ex)
        {
            ErrorMessage = $"An error occurred: {ex.Message}";
        }
        finally
        {
            IsLoginInProgress = false;
        }
    }
}