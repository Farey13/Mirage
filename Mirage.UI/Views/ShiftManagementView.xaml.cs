using Microsoft.Extensions.DependencyInjection;
using Mirage.UI.ViewModels;
using System.Windows.Controls;

namespace Mirage.UI.Views
{
    public partial class ShiftManagementView : UserControl
    {
        public ShiftManagementView()
        {
            InitializeComponent();
            this.DataContext = App.ServiceProvider?.GetRequiredService<ShiftManagementViewModel>();
        }
    }
}