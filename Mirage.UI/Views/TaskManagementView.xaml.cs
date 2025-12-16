using Mirage.UI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Mirage.UI.Views
{
    /// <summary>
    /// Interaction logic for TaskManagementView.xaml
    /// </summary>
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
                // Triggers the data load every time you open the tab
                if (viewModel.LoadDataCommand.CanExecute(null))
                {
                    viewModel.LoadDataCommand.Execute(null);
                }
            }
        }
    }
}