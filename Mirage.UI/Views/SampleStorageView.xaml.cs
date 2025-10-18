using Microsoft.Extensions.DependencyInjection;
using Mirage.UI.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Mirage.UI.Views
{
    public partial class SampleStorageView : UserControl
    {
        public SampleStorageView()
        {
            InitializeComponent();
            this.DataContext = App.ServiceProvider?.GetRequiredService<SampleStorageViewModel>();
        }

        private void SampleStorageView_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is SampleStorageViewModel viewModel && viewModel.SearchCommand.CanExecute(null))
            {
                viewModel.SearchCommand.Execute(null);
            }
        }
    }
}