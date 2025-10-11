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

    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    // The constructor now receives the services it needs
    public LoginViewModel(IPortalMirageApi apiClient, IAuthService authService)
    {
        _apiClient = apiClient;
        _authService = authService;
    }

    [RelayCommand]
    private async Task Login(object? parameter)
    {
        if (parameter is not System.Windows.Controls.PasswordBox passwordBox) return;
        var password = passwordBox.Password;

        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(password))
        {
            ErrorMessage = "Username and password are required.";
            return;
        }

        ErrorMessage = string.Empty;
        try
        {
            var loginRequest = new LoginRequest(Username, password);
            var loginResponse = await _apiClient.LoginAsync(loginRequest);

            // Here is the magic: we use the service to store the token centrally
            _authService.SetToken($"Bearer {loginResponse.Token}");

            // Open the main window using the service provider
            var mainWindow = App.ServiceProvider?.GetRequiredService<MainWindow>();
            mainWindow?.Show();

            // Close the login window
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
    }
}