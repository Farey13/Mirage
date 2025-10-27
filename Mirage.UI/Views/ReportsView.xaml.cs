using Microsoft.Extensions.DependencyInjection;
using Mirage.UI.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Mirage.UI.Views
{
    public partial class ReportsView : UserControl
    {
        public ReportsView()
        {
            InitializeComponent();
            // Connect to the shared ViewModel
            this.DataContext = App.ServiceProvider?.GetRequiredService<ReportsViewModel>();
        }

        // This method runs when the Reports tab is shown
        private void ReportsView_OnLoaded(object sender, RoutedEventArgs e)
        {
            // Get the ViewModel and execute the command to load filters
            if (this.DataContext is ReportsViewModel viewModel && viewModel.LoadFilterOptionsCommand.CanExecute(null))
            {
                viewModel.LoadFilterOptionsCommand.Execute(null);
            }
        }
    }
}