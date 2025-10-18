using Microsoft.Extensions.DependencyInjection;
using Mirage.UI.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Mirage.UI.Views
{
    public partial class RepeatSampleView : UserControl
    {
        public RepeatSampleView()
        {
            InitializeComponent();
            this.DataContext = App.ServiceProvider?.GetRequiredService<RepeatSampleViewModel>();
        }

        private void RepeatSampleView_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is RepeatSampleViewModel viewModel && viewModel.LoadLogsCommand.CanExecute(null))
            {
                viewModel.LoadLogsCommand.Execute(null);
            }
        }
    }
}