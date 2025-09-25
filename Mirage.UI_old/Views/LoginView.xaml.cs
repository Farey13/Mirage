using MahApps.Metro.Controls;
using Mirage.UI.ViewModels;
using System.Windows;

namespace Mirage.UI.Views
{
    public partial class LoginView : MetroWindow
    {
        public LoginView()
        {
            InitializeComponent();
            if (DataContext is LoginViewModel vm)
            {
                vm.CloseWindowAction = (window) => window.Close();
            }
        }
    }
}