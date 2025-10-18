using Microsoft.Extensions.DependencyInjection;
using Mirage.UI.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Mirage.UI.Views
{
    public partial class MachineBreakdownView : UserControl
    {
        public MachineBreakdownView()
        {
            InitializeComponent();
            this.DataContext = App.ServiceProvider?.GetRequiredService<MachineBreakdownViewModel>();
        }

        private void MachineBreakdownView_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is MachineBreakdownViewModel viewModel)
            {
                // Load both the machine names and the breakdown data
                viewModel.LoadMachineNamesCommand.Execute(null);
                viewModel.SearchCommand.Execute(null);
            }
        }
    }
}