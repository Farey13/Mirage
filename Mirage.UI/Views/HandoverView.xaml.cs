using Microsoft.Extensions.DependencyInjection;
using Mirage.UI.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Mirage.UI.Views
{
    public partial class HandoverView : UserControl
    {
        public HandoverView()
        {
            InitializeComponent();
            this.DataContext = App.ServiceProvider?.GetRequiredService<HandoverViewModel>();
        }

        private void HandoverView_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is HandoverViewModel viewModel && viewModel.SearchCommand.CanExecute(null))
            {
                viewModel.SearchCommand.Execute(null);
            }
        }
    }
}