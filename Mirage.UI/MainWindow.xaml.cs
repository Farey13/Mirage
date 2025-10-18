using MahApps.Metro.Controls;
using Microsoft.Extensions.DependencyInjection;
using Mirage.UI.Services;
using Mirage.UI.ViewModels;
using System.Windows.Input;

namespace Mirage.UI
{
    public partial class MainWindow : MetroWindow
    {
        private readonly IInactivityService _inactivityService;

        // The constructor now receives the InactivityService and MainViewModel
        public MainWindow(IInactivityService inactivityService, MainViewModel viewModel)
        {
            InitializeComponent();

            // Store the service
            _inactivityService = inactivityService;

            // Assign the ViewModel to the DataContext
            this.DataContext = viewModel;

            // Hook into the events to detect user activity
            this.PreviewMouseMove += MainWindow_PreviewInput;
            this.PreviewKeyDown += MainWindow_PreviewInput;
        }

        // This single method handles both mouse and keyboard events
        private void MainWindow_PreviewInput(object sender, InputEventArgs e)
        {
            // When the user is active, we reset the timer in our service
            _inactivityService.ResetTimer();
        }
    }
}