using Microsoft.Extensions.DependencyInjection;
using Mirage.UI.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Mirage.UI.Views
{
    public partial class SystemSettingsView : UserControl
    {
        public SystemSettingsView()
        {
            InitializeComponent();
            this.DataContext = App.ServiceProvider?.GetRequiredService<SystemSettingsViewModel>();
        }

        private void SystemSettingsView_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is SystemSettingsViewModel viewModel && viewModel.LoadSettingsCommand.CanExecute(null)) // Use LoadSettingsCommand
            {
                viewModel.LoadSettingsCommand.Execute(null); // Use LoadSettingsCommand
            }
        }
    }
}