using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mirage.UI.Services;
using Refit;
using System;
using System.Threading.Tasks;
using System.Windows;
using PortalMirage.Core.Dtos;

namespace Mirage.UI.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    [ObservableProperty]
    private string _username = string.Empty;

    [ObservableProperty]
    private string _errorMessage = string.Empty;

    public Action<Window>? CloseWindowAction { get; set; }

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
            var apiClient = RestService.For<IPortalMirageApi>("https://localhost:7210");
            var loginRequest = new LoginRequest(Username, password);
            var loginResponse = await apiClient.LoginAsync(loginRequest);
            CalibrationLogViewModel.AuthToken = $"Bearer {loginResponse.Token}";
            KitValidationViewModel.AuthToken = $"Bearer {loginResponse.Token}"; // Add this line
            SampleStorageViewModel.AuthToken = $"Bearer {loginResponse.Token}"; // Add this line
            HandoverViewModel.AuthToken = $"Bearer {loginResponse.Token}"; // Add this line
            MachineBreakdownViewModel.AuthToken = $"Bearer {loginResponse.Token}"; // Add this line
            MediaSterilityViewModel.AuthToken = $"Bearer {loginResponse.Token}";
            RepeatSampleViewModel.AuthToken = $"Bearer {loginResponse.Token}";

            // If we get here, login was successful!
            // We'll store the token and user info later.

            // Open the main window
            var mainWindow = new MainWindow();
            mainWindow.Show();

            // Close the login window
            var loginWindow = Application.Current.Windows[0];
            CloseWindowAction?.Invoke(loginWindow);

        }
        catch (ApiException ex)
        {
            // Handle API errors (like 401 Unauthorized)
            ErrorMessage = $"Login failed. Please check your credentials. (Error: {ex.StatusCode})";
        }
        catch (Exception ex)
        {
            // Handle other errors (like network issues)
            ErrorMessage = $"An error occurred: {ex.Message}";
        }
    }
}