using Microsoft.Extensions.DependencyInjection;
using Mirage.UI.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Mirage.UI.Views
{
    public partial class DailyTaskLogView : UserControl
    {
        public DailyTaskLogView()
        {
            InitializeComponent();
            this.DataContext = App.ServiceProvider?.GetRequiredService<DailyTaskLogViewModel>();
        }

        private void DailyTaskLogView_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is DailyTaskLogViewModel viewModel && viewModel.LoadTasksCommand.CanExecute(null))
            {
                // This now uses the CORRECT command name: LoadTasksCommand
                viewModel.LoadTasksCommand.Execute(null);
            }
        }
    }
}