using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mirage.UI.Services;
using Mirage.UI.Views;
using PortalMirage.Core.Dtos;
using Refit;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Mirage.UI.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    // REMOVED: The static event is no longer needed
    // public static event EventHandler? LoginSuccessful;

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

            // Set all the AuthTokens as before
            CalibrationLogViewModel.AuthToken = $"Bearer {loginResponse.Token}";
            KitValidationViewModel.AuthToken = $"Bearer {loginResponse.Token}";
            SampleStorageViewModel.AuthToken = $"Bearer {loginResponse.Token}";
            HandoverViewModel.AuthToken = $"Bearer {loginResponse.Token}";
            MachineBreakdownViewModel.AuthToken = $"Bearer {loginResponse.Token}";
            MediaSterilityViewModel.AuthToken = $"Bearer {loginResponse.Token}";
            RepeatSampleViewModel.AuthToken = $"Bearer {loginResponse.Token}";
            DailyTaskLogViewModel.AuthToken = $"Bearer {loginResponse.Token}";
            UserManagementViewModel.AuthToken = $"Bearer {loginResponse.Token}";
            ShiftManagementViewModel.AuthToken = $"Bearer {loginResponse.Token}";
            MasterListViewModel.AuthToken = $"Bearer {loginResponse.Token}";
            AuditLogViewModel.AuthToken = $"Bearer {loginResponse.Token}";
            DashboardViewModel.AuthToken = $"Bearer {loginResponse.Token}"; // Also set the dashboard token

            // Open the main window
            var mainWindow = new MainWindow();
            mainWindow.Show();

            // --- NEW: Explicitly tell the new dashboard to refresh ---
            if (mainWindow.DataContext is MainViewModel mainVM && mainVM.CurrentView is DashboardViewModel dashboardVM)
            {
                // Execute the command to load data now that the token is set and the window is visible.
                await dashboardVM.LoadSummaryAsync();
            }

            // Close the login window
            var loginWindow = Application.Current.Windows.OfType<LoginView>().FirstOrDefault();
            loginWindow?.Close();

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
