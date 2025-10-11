using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mirage.UI.Services;
using PortalMirage.Core.Dtos;
using Refit;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Mirage.UI.ViewModels;

public partial class UserManagementViewModel : ObservableObject
{
    private readonly IPortalMirageApi _apiClient;
    private readonly IAuthService _authService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanAssignRole))]
    [NotifyPropertyChangedFor(nameof(CanManageSelectedUser))]
    [NotifyCanExecuteChangedFor(nameof(AssignRoleCommand))]
    [NotifyCanExecuteChangedFor(nameof(ResetPasswordCommand))]
    private UserResponse? _selectedUser;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanAssignRole))]
    [NotifyCanExecuteChangedFor(nameof(AssignRoleCommand))]
    private RoleResponse? _selectedRoleToAssign;

    [ObservableProperty]
    private bool _isCreateUserFlyoutOpen;
    [ObservableProperty]
    private string _newUsername = string.Empty;
    [ObservableProperty]
    private string _newFullName = string.Empty;
    [ObservableProperty]
    private bool _isManageRolesFlyoutOpen;
    [ObservableProperty]
    private string _newRoleName = string.Empty;
    [ObservableProperty]
    private bool _isResetPasswordFlyoutOpen;

    public bool CanAssignRole => SelectedUser is not null && SelectedRoleToAssign is not null;
    public bool CanManageSelectedUser => SelectedUser is not null;

    public ObservableCollection<UserResponse> Users { get; } = new();
    public ObservableCollection<RoleResponse> AllRoles { get; } = new();
    public ObservableCollection<RoleResponse> SelectedUserRoles { get; } = new();

    public UserManagementViewModel(IPortalMirageApi apiClient, IAuthService authService)
    {
        _apiClient = apiClient;
        _authService = authService;
        _ = LoadDataAsync();
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task LoadDataAsync()
    {
        var authToken = _authService.GetToken();
        if (string.IsNullOrEmpty(authToken)) return;

        try
        {
            var users = await _apiClient.GetAllUsersAsync(authToken);
            Users.Clear();
            foreach (var user in users) Users.Add(user);

            var roles = await _apiClient.GetAllRolesAsync(authToken);
            AllRoles.Clear();
            foreach (var role in roles) AllRoles.Add(role);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to load data: {ex.Message}");
        }
    }

    partial void OnSelectedUserChanged(UserResponse? value)
    {
        _ = LoadRolesForSelectedUserAsync();
    }

    private async System.Threading.Tasks.Task LoadRolesForSelectedUserAsync()
    {
        SelectedUserRoles.Clear();
        var authToken = _authService.GetToken();
        if (SelectedUser is null || string.IsNullOrEmpty(authToken)) return;

        try
        {
            var roles = await _apiClient.GetRolesForUserAsync(authToken, SelectedUser.Username);
            foreach (var role in roles) SelectedUserRoles.Add(role);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to load roles for user: {ex.Message}");
        }
    }

    [RelayCommand(CanExecute = nameof(CanAssignRole))]
    private async System.Threading.Tasks.Task AssignRole()
    {
        var authToken = _authService.GetToken();
        if (SelectedUser is null || SelectedRoleToAssign is null || string.IsNullOrEmpty(authToken)) return;

        if (SelectedUserRoles.Any(r => r.RoleID == SelectedRoleToAssign.RoleID))
        {
            MessageBox.Show("User already has this role.");
            return;
        }
        try
        {
            var request = new AssignRoleRequest(SelectedUser.Username, SelectedRoleToAssign.RoleName);
            await _apiClient.AssignRoleAsync(authToken, request);
            await LoadRolesForSelectedUserAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to assign role: {ex.Message}");
        }
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task RemoveRole(RoleResponse roleToRemove)
    {
        var authToken = _authService.GetToken();
        if (SelectedUser is null || roleToRemove is null || string.IsNullOrEmpty(authToken)) return;

        if (MessageBox.Show($"Are you sure you want to remove the '{roleToRemove.RoleName}' role from {SelectedUser.Username}?", "Confirm Removal", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) return;

        try
        {
            var request = new AssignRoleRequest(SelectedUser.Username, roleToRemove.RoleName);
            await _apiClient.RemoveRoleFromUserAsync(authToken, request);
            await LoadRolesForSelectedUserAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to remove role: {ex.Message}");
        }
    }

    [RelayCommand]
    private void CreateUser()
    {
        if (IsCreateUserFlyoutOpen)
        {
            IsCreateUserFlyoutOpen = false;
            return;
        }
        IsManageRolesFlyoutOpen = false;
        IsResetPasswordFlyoutOpen = false;
        NewUsername = string.Empty;
        NewFullName = string.Empty;
        IsCreateUserFlyoutOpen = true;
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task ManageRoles()
    {
        if (IsManageRolesFlyoutOpen)
        {
            IsManageRolesFlyoutOpen = false;
            return;
        }
        IsCreateUserFlyoutOpen = false;
        IsResetPasswordFlyoutOpen = false;
        await LoadDataAsync();
        IsManageRolesFlyoutOpen = true;
    }

    [RelayCommand(CanExecute = nameof(CanManageSelectedUser))]
    private void ResetPassword()
    {
        if (IsResetPasswordFlyoutOpen)
        {
            IsResetPasswordFlyoutOpen = false;
            return;
        }
        IsCreateUserFlyoutOpen = false;
        IsManageRolesFlyoutOpen = false;
        IsResetPasswordFlyoutOpen = true;
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task SaveNewUser(object parameter)
    {
        if (parameter is not PasswordBox passwordBox) return;
        var password = passwordBox.Password;
        var authToken = _authService.GetToken();

        if (string.IsNullOrEmpty(authToken) || string.IsNullOrWhiteSpace(NewUsername) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(NewFullName))
        {
            ShowMessageBox("Username, Password, and Full Name are all required.", "Missing Information", MessageBoxImage.Warning);
            return;
        }

        try
        {
            var request = new CreateUserRequest(NewUsername, password, NewFullName);
            await _apiClient.CreateUserAsync(authToken, request);

            IsCreateUserFlyoutOpen = false;
            await LoadDataAsync();
            ShowMessageBox($"User '{NewUsername}' created successfully!", "Success", MessageBoxImage.Information);
            passwordBox.Password = string.Empty;
        }
        catch (ApiException ex)
        {
            ShowMessageBox($"Failed to create user. The server responded with: {ex.StatusCode}. The username may already be taken.", "API Error", MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            ShowMessageBox($"An unexpected error occurred: {ex.Message}", "Error", MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void CancelCreateUser()
    {
        IsCreateUserFlyoutOpen = false;
        NewUsername = string.Empty;
        NewFullName = string.Empty;
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task CreateNewRole()
    {
        if (string.IsNullOrWhiteSpace(NewRoleName))
        {
            MessageBox.Show("Role name cannot be empty.", "Missing Information");
            return;
        }
        var authToken = _authService.GetToken();
        if (string.IsNullOrEmpty(authToken)) return;

        try
        {
            var request = new CreateRoleRequest(NewRoleName);
            var newRole = await _apiClient.CreateRoleAsync(authToken, request);
            AllRoles.Add(newRole);
            NewRoleName = string.Empty;
            MessageBox.Show($"Role '{newRole.RoleName}' created successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (ApiException ex)
        {
            MessageBox.Show($"Failed to create role. The server responded with: {ex.StatusCode}. The role may already exist.", "API Error");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Error");
        }
    }

    [RelayCommand]
    private void CancelManageRoles()
    {
        IsManageRolesFlyoutOpen = false;
        NewRoleName = string.Empty;
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task ConfirmResetPassword(object parameter)
    {
        if (parameter is not PasswordBox passwordBox) return;
        var newPassword = passwordBox.Password;
        var authToken = _authService.GetToken();

        if (SelectedUser is null || string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrEmpty(authToken))
        {
            ShowMessageBox("Please enter a new password.", "Missing Information", MessageBoxImage.Warning);
            return;
        }

        try
        {
            var request = new ResetPasswordRequest(SelectedUser.Username, newPassword);
            await _apiClient.ResetPasswordAsync(authToken, request);
            IsResetPasswordFlyoutOpen = false;
            ShowMessageBox($"Password for {SelectedUser.Username} has been reset successfully.", "Success", MessageBoxImage.Information);
            passwordBox.Password = string.Empty;
        }
        catch (Exception ex)
        {
            ShowMessageBox($"Failed to reset password: {ex.Message}", "Error", MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void CancelResetPassword()
    {
        IsResetPasswordFlyoutOpen = false;
    }

    private void ShowMessageBox(string message, string caption, MessageBoxImage image)
    {
        MessageBox.Show(message, caption, MessageBoxButton.OK, image);
    }
}