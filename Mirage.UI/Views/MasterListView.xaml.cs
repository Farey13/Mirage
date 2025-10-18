using Microsoft.Extensions.DependencyInjection;
using Mirage.UI.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Mirage.UI.Views
{
    public partial class MasterListView : UserControl
    {
        public MasterListView()
        {
            InitializeComponent();
            // This line connects the View to its shared ViewModel
            this.DataContext = App.ServiceProvider?.GetRequiredService<MasterListViewModel>();
        }

        // This method will run when the Master List tab is shown
        private void MasterListView_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is MasterListViewModel viewModel && viewModel.LoadAllItemsCommand.CanExecute(null))
            {
                viewModel.LoadAllItemsCommand.Execute(null);
            }
        }
    }
}