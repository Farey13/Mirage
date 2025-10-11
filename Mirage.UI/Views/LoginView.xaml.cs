using MahApps.Metro.Controls;
using Microsoft.Extensions.DependencyInjection;
using Mirage.UI.ViewModels;
using System.Windows;

namespace Mirage.UI.Views
{
    public partial class LoginView : MetroWindow
    {
        public LoginView()
        {
            InitializeComponent();
            // Get the shared LoginViewModel from our app's service provider
            DataContext = App.ServiceProvider?.GetRequiredService<LoginViewModel>();
        }
    }
}