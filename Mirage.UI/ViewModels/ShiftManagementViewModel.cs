using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mirage.UI.Services;
using PortalMirage.Core.Dtos;
using Refit;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace Mirage.UI.ViewModels;

public partial class ShiftManagementViewModel : ObservableObject
{
    public static string? AuthToken { get; set; }
    private readonly IPortalMirageApi _apiClient;

    public ObservableCollection<ShiftResponse> Shifts { get; } = new();

    [ObservableProperty]
    private ShiftResponse? _selectedShift;

    [ObservableProperty]
    private string _editShiftName = string.Empty;

    [ObservableProperty]
    private DateTime? _editStartTime; // Use DateTime? for TimePicker

    [ObservableProperty]
    private DateTime? _editEndTime;   // Use DateTime? for TimePicker

    [ObservableProperty]
    private int _editGracePeriodHours;

    [ObservableProperty]
    private bool _isEditing;

    public ShiftManagementViewModel()
    {
        _apiClient = RestService.For<IPortalMirageApi>("https://localhost:7210");
        LoadShifts();
    }

    private async void LoadShifts() => await LoadShiftsAsync();

    [RelayCommand]
    private async System.Threading.Tasks.Task LoadShiftsAsync()
    {
        if (string.IsNullOrEmpty(AuthToken)) return;
        try
        {
            var shifts = await _apiClient.GetAllShiftsAsync(AuthToken);
            Shifts.Clear();
            foreach (var shift in shifts) Shifts.Add(shift);
        }
        catch (Exception ex) { MessageBox.Show($"Failed to load shifts: {ex.Message}"); }
    }

    partial void OnSelectedShiftChanged(ShiftResponse? value)
    {
        if (value is not null)
        {
            IsEditing = true;
            EditShiftName = value.ShiftName;

            // Convert TimeOnly to DateTime for TimePicker binding
            EditStartTime = DateTime.Today.Add(value.StartTime.ToTimeSpan());
            EditEndTime = DateTime.Today.Add(value.EndTime.ToTimeSpan());

            EditGracePeriodHours = value.GracePeriodHours;
        }
        else
        {
            IsEditing = false;
        }
    }

    [RelayCommand]
    private void NewShift()
    {
        SelectedShift = null;
        IsEditing = true;
        EditShiftName = "New Shift";

        // Set default times as DateTime for TimePicker
        EditStartTime = DateTime.Today.Add(new TimeSpan(8, 0, 0));
        EditEndTime = DateTime.Today.Add(new TimeSpan(16, 0, 0));

        EditGracePeriodHours = 2;
    }

    [RelayCommand]
    private async System.Threading.Tasks.Task SaveShift()
    {
        if (string.IsNullOrEmpty(AuthToken) || string.IsNullOrWhiteSpace(EditShiftName))
        {
            MessageBox.Show("Shift Name cannot be empty.");
            return;
        }

        if (EditStartTime is null || EditEndTime is null)
        {
            MessageBox.Show("Start Time and End Time must be set.");
            return;
        }

        try
        {
            // Extract TimeOnly from DateTime for API request
            var startTime = TimeOnly.FromTimeSpan(EditStartTime.Value.TimeOfDay);
            var endTime = TimeOnly.FromTimeSpan(EditEndTime.Value.TimeOfDay);

            if (SelectedShift is null) // Creating a new shift
            {
                var request = new CreateShiftRequest(EditShiftName, startTime, endTime, EditGracePeriodHours);
                await _apiClient.CreateShiftAsync(AuthToken, request);
            }
            else // Updating an existing shift
            {
                var request = new UpdateShiftRequest(SelectedShift.ShiftID, EditShiftName, startTime, endTime, EditGracePeriodHours, true);
                await _apiClient.UpdateShiftAsync(AuthToken, SelectedShift.ShiftID, request);
            }

            IsEditing = false;
            SelectedShift = null;
            await LoadShiftsAsync();
        }
        catch (Exception ex) { MessageBox.Show($"Failed to save shift: {ex.Message}"); }
    }

    [RelayCommand]
    private async Task DeleteShift()
    {
        if (SelectedShift is null || string.IsNullOrEmpty(AuthToken)) return;

        if (MessageBox.Show($"Are you sure you want to deactivate the '{SelectedShift.ShiftName}' shift?", "Confirm Deactivation", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.No)
        {
            return;
        }

        try
        {
            await _apiClient.DeactivateShiftAsync(AuthToken, SelectedShift.ShiftID);
            await LoadShiftsAsync();
        }
        catch (Exception ex) { MessageBox.Show($"Failed to deactivate shift: {ex.Message}"); }
    }
}