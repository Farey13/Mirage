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

namespace Mirage.UI.ViewModels;

public partial class UserManagementViewModel : ObservableObject
{
    public static string? AuthToken { get; set; }
    private readonly IPortalMirageApi _apiClient;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanAssignRole))]
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
    private string _newPassword = string.Empty;

    [ObservableProperty]
    private string _newFullName = string.Empty;

    public bool CanAssignRole => SelectedUser is not null && SelectedRoleToAssign is not null;

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

    // --- Commands for New Features ---
    [RelayCommand]
    private void CreateUser()
    {
        // Clear fields and open the flyout
        NewUsername = string.Empty;
        NewPassword = string.Empty;
        NewFullName = string.Empty;
        IsCreateUserFlyoutOpen = true;
    }

    [RelayCommand]
    private async Task SaveNewUser()
    {
        if (string.IsNullOrWhiteSpace(NewUsername) || string.IsNullOrWhiteSpace(NewPassword) || string.IsNullOrWhiteSpace(NewFullName))
        {
            MessageBox.Show("Username, Password, and Full Name are all required.", "Missing Information");
            return;
        }

        try
        {
            // Use parameterized constructor - make sure it exists in your DTO
            var request = new CreateUserRequest(NewUsername, NewPassword, NewFullName);

            await _apiClient.CreateUserAsync(AuthToken!, request);

            IsCreateUserFlyoutOpen = false; // Close flyout on success
            await LoadDataAsync(); // Refresh the user list

            MessageBox.Show($"User '{NewUsername}' created successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (ApiException ex)
        {
            MessageBox.Show($"Failed to create user. The server responded with: {ex.StatusCode}. The username may already be taken.", "API Error");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"An unexpected error occurred: {ex.Message}", "Error");
        }
    }

    [RelayCommand]
    private void CancelCreateUser()
    {
        IsCreateUserFlyoutOpen = false;
        NewUsername = string.Empty;
        NewPassword = string.Empty;
        NewFullName = string.Empty;
    }

    [RelayCommand]
    private void ManageRoles() => MessageBox.Show("Manage Roles feature coming soon!");

    [RelayCommand]
    private void ResetPassword() => MessageBox.Show("Reset Password feature coming soon!");
}