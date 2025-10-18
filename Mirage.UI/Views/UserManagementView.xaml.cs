using Microsoft.Extensions.DependencyInjection;
using Mirage.UI.ViewModels;
using System.Windows.Controls;

namespace Mirage.UI.Views
{
    public partial class UserManagementView : UserControl
    {
        public UserManagementView()
        {
            InitializeComponent();
            this.DataContext = App.ServiceProvider?.GetRequiredService<UserManagementViewModel>();
        }
    }
}