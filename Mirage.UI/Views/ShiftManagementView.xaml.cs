using Microsoft.Extensions.DependencyInjection;
using Mirage.UI.ViewModels;
using System.Windows;             // Required for RoutedEventArgs
using System.Windows.Controls;

namespace Mirage.UI.Views
{
    public partial class ShiftManagementView : UserControl
    {
        public ShiftManagementView()
        {
            InitializeComponent();
            // Connect the View to the ViewModel
            this.DataContext = App.ServiceProvider?.GetRequiredService<ShiftManagementViewModel>();
        }

        // This runs every time the tab becomes visible
        private void ShiftManagementView_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is ShiftManagementViewModel viewModel)
            {
                // Force a refresh of the list from the database
                if (viewModel.LoadShiftsCommand.CanExecute(null))
                {
                    viewModel.LoadShiftsCommand.Execute(null);
                }
            }
        }
    }
}