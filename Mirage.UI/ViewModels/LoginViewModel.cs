using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mirage.UI.Services;
using Mirage.UI.Views;
using Microsoft.Extensions.DependencyInjection;
using PortalMirage.Core.Dtos;
using PortalMirage.Core.Models;
using Refit;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Mirage.UI.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IPortalMirageApi _apiClient;
    private readonly IAuthService _authService;
    private readonly MainViewModel _mainViewModel;
    private readonly IInactivityService _inactivityService;

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    [ObservableProperty]
    private bool _isLoginInProgress;

    public LoginViewModel(IPortalMirageApi apiClient, IAuthService authService,
                         MainViewModel mainViewModel, IInactivityService inactivityService)
    {
        _apiClient = apiClient;
        _authService = authService;
        _mainViewModel = mainViewModel;
        _inactivityService = inactivityService;
    }

    [RelayCommand]
    private async Task LoginAsync(object? parameter)
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

            // 1. Set the Token
            _authService.SetToken($"Bearer {loginResponse.Token}");

            // 2. Map User Response to User Model
            var userModel = new User
            {
                // Fix: Check if your DTO has UserId or UserID
                UserID = loginResponse.User.UserId, // or .UserID depending on your DTO
                Username = loginResponse.User.Username,
                FullName = loginResponse.User.FullName,
                IsActive = true,
                // Fix: Satisfies 'required string PasswordHash' if applicable
                PasswordHash = string.Empty
            };

            // 3. Set the User in AuthService (Role extraction happens here)
            _authService.SetCurrentUser(userModel);

            // 4. Update MainViewModel
            _mainViewModel.CurrentUser = loginResponse.User;

            // 5. Show Main Window
            var mainWindow = App.ServiceProvider?.GetRequiredService<MainWindow>();
            mainWindow?.Show();

            InitializeInactivityTimer();

            var dashboardViewModel = App.ServiceProvider?.GetRequiredService<DashboardViewModel>();
            if (dashboardViewModel != null)
            {
                _ = dashboardViewModel.LoadSummaryAsync();
            }

            var loginWindow = Application.Current.Windows.OfType<LoginView>().FirstOrDefault();
            loginWindow?.Close();
        }
        catch (ApiException ex)
        {
            ErrorMessage = $"Login failed. (Error: {ex.StatusCode})";
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

    private async void InitializeInactivityTimer()
    {
        try
        {
            var authToken = _authService.GetToken();
            if (!string.IsNullOrEmpty(authToken))
            {
                var setting = await _apiClient.GetSettingAsync(authToken, "InactivityTimeoutMinutes");
                if (int.TryParse(setting.Description, out int timeoutMinutes) && timeoutMinutes > 0)
                {
                    _inactivityService.StartTimer(timeoutMinutes);
                    return;
                }
            }
        }
        catch { }
        _inactivityService.StartTimer(15);
    }
}