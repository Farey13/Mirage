using Microsoft.Extensions.DependencyInjection;
using Mirage.UI.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Mirage.UI.Views
{
    public partial class AuditLogView : UserControl
    {
        public AuditLogView()
        {
            InitializeComponent();
            // This line explicitly gets the shared AuditLogViewModel and sets it.
            this.DataContext = App.ServiceProvider?.GetRequiredService<AuditLogViewModel>();
        }

        private void AuditLogView_OnLoaded(object sender, RoutedEventArgs e)
        {
            // When the view loads, we get the ViewModel and execute the command.
            if (this.DataContext is AuditLogViewModel viewModel && viewModel.LoadLogsCommand.CanExecute("week"))
            {
                viewModel.LoadLogsCommand.Execute("week");
            }
        }
    }
}