﻿using Microsoft.Extensions.DependencyInjection;
using Mirage.UI.ViewModels;
using System.Windows;
using System.Windows.Controls;

namespace Mirage.UI.Views
{
    public partial class KitValidationView : UserControl
    {
        public KitValidationView()
        {
            InitializeComponent();
            this.DataContext = App.ServiceProvider?.GetRequiredService<KitValidationViewModel>();
        }

        private void KitValidationView_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is KitValidationViewModel viewModel && viewModel.LoadLogsCommand.CanExecute(null))
            {
                viewModel.LoadLogsCommand.Execute(null);
            }
        }
    }
}