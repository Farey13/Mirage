using Mirage.UI.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Mirage.UI.Views
{
    public partial class TaskManagementView : UserControl
    {
        public TaskManagementView()
        {
            InitializeComponent();
        }

        private void TaskManagementView_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is TaskManagementViewModel viewModel)
            {
                if (viewModel.LoadDataCommand.CanExecute(null))
                {
                    viewModel.LoadDataCommand.Execute(null);
                }
            }
        }

        private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is DataGrid dataGrid && dataGrid.SelectedItem is PortalMirage.Core.Models.TaskModel task)
            {
                if (this.DataContext is TaskManagementViewModel viewModel)
                {
                    viewModel.SelectTaskCommand.Execute(task);
                }
            }
        }
    }
}