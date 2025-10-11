using MahApps.Metro.Controls;
using Microsoft.Extensions.DependencyInjection;
using Mirage.UI.ViewModels;

namespace Mirage.UI
{
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            // This line is essential. It connects the window to its "brain" (the MainViewModel).
            DataContext = App.ServiceProvider?.GetRequiredService<MainViewModel>();
        }
    }
}