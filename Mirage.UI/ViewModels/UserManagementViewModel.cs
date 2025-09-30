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
    public static string? AuthToken { get; set; }
    private readonly IPortalMirageApi _apiClient;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanAssignRole))]
    [NotifyPropertyChangedFor(nameof(CanManageSelectedUser))]
    private UserResponse? _selectedUser;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanAssignRole))]
    private RoleResponse? _selectedRoleToAssign;

    // Properties for the "Create User" Flyout
    [ObservableProperty]
    private bool _isCreateUserFlyoutOpen;

    [ObservableProperty]
    private string _newUsername = string.Empty;

    [ObservableProperty]
    private string _newFullName = string.Empty;

    // --- Properties for the "Manage Roles" Flyout ---
    [ObservableProperty]
    private bool _isManageRolesFlyoutOpen;

    [ObservableProperty]
    private string _newRoleName = string.Empty;

    // --- Properties for the Reset Password Flyout ---
    [ObservableProperty]
    private bool _isResetPasswordFlyoutOpen;

    public bool CanAssignRole => SelectedUser is not null && SelectedRoleToAssign is not null;
    public bool CanManageSelectedUser => SelectedUser is not null;

    public ObservableCollection<UserResponse> Users { get; } = new();
    public ObservableCollection<RoleResponse> AllRoles { get; } = new();
    public ObservableCollection<RoleResponse> SelectedUserRoles { get; } = new();

    public UserManagementViewModel()
    {
        _apiClient = RestService.For<IPortalMirageApi>("https://localhost:7210");
        _ = LoadDataAsync();
    }

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        if (string.IsNullOrEmpty(AuthToken)) return;
        try
        {
            var users = await _apiClient.GetAllUsersAsync(AuthToken);
            Users.Clear();
            foreach (var user in users) Users.Add(user);

            var roles = await _apiClient.GetAllRolesAsync(AuthToken);
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

    private async Task LoadRolesForSelectedUserAsync()
    {
        SelectedUserRoles.Clear();
        if (SelectedUser is null || string.IsNullOrEmpty(AuthToken)) return;
        try
        {
            var roles = await _apiClient.GetRolesForUserAsync(AuthToken, SelectedUser.Username);
            foreach (var role in roles) SelectedUserRoles.Add(role);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to load roles for user: {ex.Message}");
        }
    }

    [RelayCommand(CanExecute = nameof(CanAssignRole))]
    private async Task AssignRole()
    {
        if (SelectedUser is null || SelectedRoleToAssign is null || string.IsNullOrEmpty(AuthToken)) return;
        if (SelectedUserRoles.Any(r => r.RoleID == SelectedRoleToAssign.RoleID))
        {
            MessageBox.Show("User already has this role.");
            return;
        }
        try
        {
            var request = new AssignRoleRequest(SelectedUser.Username, SelectedRoleToAssign.RoleName);
            await _apiClient.AssignRoleAsync(AuthToken, request);
            await LoadRolesForSelectedUserAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to assign role: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task RemoveRole(RoleResponse roleToRemove)
    {
        if (SelectedUser is null || roleToRemove is null || string.IsNullOrEmpty(AuthToken)) return;
        if (MessageBox.Show($"Are you sure you want to remove the '{roleToRemove.RoleName}' role from {SelectedUser.Username}?", "Confirm Removal", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No) return;

        try
        {
            var request = new AssignRoleRequest(SelectedUser.Username, roleToRemove.RoleName);
            await _apiClient.RemoveRoleFromUserAsync(AuthToken, request);
            await LoadRolesForSelectedUserAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to remove role: {ex.Message}");
        }
    }

    // --- Updated Commands with Toggle Logic ---
    [RelayCommand]
    private void CreateUser()
    {
        // If this flyout is already open, just close it and do nothing else.
        if (IsCreateUserFlyoutOpen)
        {
            IsCreateUserFlyoutOpen = false;
            return;
        }

        // Close any other open flyouts before opening this one.
        IsManageRolesFlyoutOpen = false;
        IsResetPasswordFlyoutOpen = false;

        // Clear fields and open the flyout.
        NewUsername = string.Empty;
        NewFullName = string.Empty;
        IsCreateUserFlyoutOpen = true;
    }

    [RelayCommand]
    private async Task ManageRoles()
    {
        // If this flyout is already open, just close it and do nothing else.
        if (IsManageRolesFlyoutOpen)
        {
            IsManageRolesFlyoutOpen = false;
            return;
        }

        // Close any other open flyouts before opening this one.
        IsCreateUserFlyoutOpen = false;
        IsResetPasswordFlyoutOpen = false;

        // We already have the roles in AllRoles, just ensure it's up to date
        await LoadDataAsync();
        IsManageRolesFlyoutOpen = true;
    }

    [RelayCommand(CanExecute = nameof(CanManageSelectedUser))]
    private void ResetPassword()
    {
        // If this flyout is already open, just close it and do nothing else.
        if (IsResetPasswordFlyoutOpen)
        {
            IsResetPasswordFlyoutOpen = false;
            return;
        }

        // Close any other open flyouts before opening this one.
        IsCreateUserFlyoutOpen = false;
        IsManageRolesFlyoutOpen = false;

        IsResetPasswordFlyoutOpen = true;
    }

    [RelayCommand]
    private async Task SaveNewUser(object parameter)
    {
        // Read the password directly from the PasswordBox control
        if (parameter is not PasswordBox passwordBox) return;
        var password = passwordBox.Password;

        if (string.IsNullOrWhiteSpace(NewUsername) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(NewFullName))
        {
            MessageBox.Show("Username, Password, and Full Name are all required.", "Missing Information");
            return;
        }

        try
        {
            var request = new CreateUserRequest(NewUsername, password, NewFullName);
            await _apiClient.CreateUserAsync(AuthToken!, request);

            IsCreateUserFlyoutOpen = false;
            await LoadDataAsync();

            ShowMessageBox($"User '{NewUsername}' created successfully!", "Success", MessageBoxImage.Information);

            // Clear the password box
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
    private async Task CreateNewRole()
    {
        if (string.IsNullOrWhiteSpace(NewRoleName))
        {
            MessageBox.Show("Role name cannot be empty.", "Missing Information");
            return;
        }

        try
        {
            var request = new CreateRoleRequest(NewRoleName);
            var newRole = await _apiClient.CreateRoleAsync(AuthToken!, request);

            // Add to the list and clear the textbox
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
    private async Task ConfirmResetPassword(object parameter)
    {
        // Read the password directly from the PasswordBox control
        if (parameter is not PasswordBox passwordBox) return;
        var newPassword = passwordBox.Password;

        if (SelectedUser is null || string.IsNullOrWhiteSpace(newPassword))
        {
            ShowMessageBox("Please enter a new password.", "Missing Information", MessageBoxImage.Warning);
            return;
        }

        try
        {
            var request = new ResetPasswordRequest(SelectedUser.Username, newPassword);
            await _apiClient.ResetPasswordAsync(AuthToken!, request);
            IsResetPasswordFlyoutOpen = false;
            ShowMessageBox($"Password for {SelectedUser.Username} has been reset successfully.", "Success", MessageBoxImage.Information);

            // Clear the password box
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

    // Helper method for consistent message boxes
    private void ShowMessageBox(string message, string caption, MessageBoxImage image)
    {
        MessageBox.Show(message, caption, MessageBoxButton.OK, image);
    }
}