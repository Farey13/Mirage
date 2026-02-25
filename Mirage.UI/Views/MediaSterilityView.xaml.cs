using Microsoft.Extensions.DependencyInjection;
using Mirage.UI.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Mirage.UI.Views
{
    public partial class MediaSterilityView : UserControl
    {
        public MediaSterilityView()
        {
            InitializeComponent();
            this.DataContext = App.ServiceProvider?.GetRequiredService<MediaSterilityViewModel>();
        }

        private void MediaSterilityView_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is MediaSterilityViewModel viewModel)
            {
                if (viewModel.LoadLogsCommand.CanExecute(null))
                {
                    viewModel.LoadLogsCommand.Execute(null);
                }
                // ADDED: Force refresh the dropdowns every time the page opens
                _ = viewModel.LoadMasterLists();
            }
        }
    }
}