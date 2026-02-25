using Microsoft.Extensions.DependencyInjection;
using Mirage.UI.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Mirage.UI.Views
{
    public partial class CalibrationLogView : UserControl
    {
        public CalibrationLogView()
        {
            InitializeComponent();
            this.DataContext = App.ServiceProvider?.GetRequiredService<CalibrationLogViewModel>();
        }

        private void CalibrationLogView_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is CalibrationLogViewModel viewModel)
            {
                // 1. Load the data grid logs
                if (viewModel.LoadLogsCommand.CanExecute(null))
                {
                    viewModel.LoadLogsCommand.Execute(null);
                }

                // 2. Force refresh the dropdowns (instruments/tests) every time the page opens
                _ = viewModel.LoadMasterLists();
            }
        }
    }
}