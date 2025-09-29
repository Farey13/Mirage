using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Mirage.UI.Services;
using PortalMirage.Core.Dtos;
using Refit;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Mirage.UI.ViewModels;

public partial class MasterListViewModel : ObservableObject
{
    public static string? AuthToken { get; set; }
    private readonly IPortalMirageApi _apiClient;

    private List<AdminListItemDto> _allItems = new();
    public ObservableCollection<AdminListItemDto> FilteredItems { get; } = new();
    public ObservableCollection<string> ListTypes { get; } = new();

    [ObservableProperty]
    private AdminListItemDto? _selectedItem;

    [ObservableProperty]
    private string? _selectedListType;

    [ObservableProperty]
    private string _searchFilter = string.Empty;

    [ObservableProperty]
    private string _editItemValue = string.Empty;

    [ObservableProperty]
    private string? _editDescription;

    [ObservableProperty]
    private bool _isItemActive = true;

    [ObservableProperty]
    private bool _isEditing;

    public int ItemCount => FilteredItems.Count;

    public MasterListViewModel()
    {
        _apiClient = RestService.For<IPortalMirageApi>("https://localhost:7210");
        LoadInitialData();
    }

    private async void LoadInitialData() => await LoadAllItemsAsync();

    [RelayCommand]
    private async Task LoadAllItemsAsync()
    {
        if (string.IsNullOrEmpty(AuthToken)) return;
        try
        {
            // Fetch all items and list types in parallel for optimal performance
            var itemsTask = _apiClient.GetAllListItemsAsync(AuthToken);
            var typesTask = _apiClient.GetListTypesAsync(AuthToken);
            await Task.WhenAll(itemsTask, typesTask);

            _allItems = await itemsTask;
            var types = await typesTask;

            // Always refresh list types to ensure consistency
            ListTypes.Clear();
            foreach (var type in types.OrderBy(t => t))
                ListTypes.Add(type);

            // Maintain selection or set default
            if (SelectedListType is null)
                SelectedListType = ListTypes.FirstOrDefault();
            else
                FilterItems();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to load lists: {ex.Message}");
        }
    }

    partial void OnSelectedListTypeChanged(string? value)
    {
        // Reset search when changing lists for better UX
        SearchFilter = string.Empty;
        FilterItems();
        NewItem(); // Reset edit form
    }

    partial void OnSearchFilterChanged(string value) => FilterItems();

    partial void OnSelectedItemChanged(AdminListItemDto? value)
    {
        if (value is not null)
        {
            IsEditing = true;
            EditItemValue = value.ItemValue;
            EditDescription = value.Description;
            IsItemActive = value.IsActive;
        }
        else
        {
            IsEditing = false;
        }
    }

    private void FilterItems()
    {
        if (SelectedListType is null) return;

        var filtered = _allItems.Where(i => i.ListType == SelectedListType);

        if (!string.IsNullOrWhiteSpace(SearchFilter))
        {
            filtered = filtered.Where(i =>
                i.ItemValue.Contains(SearchFilter, StringComparison.OrdinalIgnoreCase) ||
                (i.Description != null && i.Description.Contains(SearchFilter, StringComparison.OrdinalIgnoreCase)));
        }

        FilteredItems.Clear();
        foreach (var item in filtered)
            FilteredItems.Add(item);

        OnPropertyChanged(nameof(ItemCount));
    }

    [RelayCommand]
    private void NewItem()
    {
        SelectedItem = null;
        IsEditing = true;
        EditItemValue = "New Item";
        EditDescription = string.Empty;
        IsItemActive = true;
    }

    [RelayCommand]
    private async Task SaveItem()
    {
        if (string.IsNullOrEmpty(AuthToken) || string.IsNullOrWhiteSpace(EditItemValue) || SelectedListType is null)
            return;

        try
        {
            if (SelectedItem is null) // Create new
            {
                var request = new CreateAdminListItemRequest(SelectedListType, EditItemValue, EditDescription);
                await _apiClient.CreateListItemAsync(AuthToken, request);
            }
            else // Update existing
            {
                var request = new UpdateAdminListItemRequest(SelectedItem.ItemID, EditItemValue, EditDescription, IsItemActive);
                await _apiClient.UpdateListItemAsync(AuthToken, SelectedItem.ItemID, request);
            }
            await LoadAllItemsAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to save item: {ex.Message}");
        }
    }
}