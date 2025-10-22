using QuestPDF.Infrastructure;
using Mirage.UI.Services;
using Mirage.UI.ViewModels;
using Mirage.UI.Views;
using Microsoft.Extensions.DependencyInjection;
using Refit;
using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Markup;

namespace Mirage.UI
{
    public partial class App : Application
    {
        private static Mutex? _mutex = null; // <-- ADD THIS LINE

        public static IServiceProvider? ServiceProvider { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            // --- SINGLE INSTANCE CHECK ---
            const string appName = "MiragePortal-9A8B7C6D-5E4F-4G3H-2I1J-K0L9M8N7O6P5";
            _mutex = new Mutex(true, appName, out bool createdNew);

            if (!createdNew)
            {
                // Another instance is already running.
                MessageBox.Show("Project Mirage is already running.", "Application Already Running", MessageBoxButton.OK, MessageBoxImage.Information);
                Application.Current.Shutdown();
                return; // Exit the startup process
            }
            // ---------------------------

            base.OnStartup(e);

            // --- ADD QUESTPDF LICENSE ---
            QuestPDF.Settings.License = LicenseType.Community;
            // ----------------------------

            // Set the custom date format for the entire application
            var cultureInfo = new CultureInfo("en-GB");
            var customCulture = (CultureInfo)cultureInfo.Clone();
            customCulture.DateTimeFormat.ShortDatePattern = "dd/MMM/yy";
            Thread.CurrentThread.CurrentCulture = customCulture;
            Thread.CurrentThread.CurrentUICulture = customCulture;
            FrameworkElement.LanguageProperty.OverrideMetadata(
                typeof(FrameworkElement),
                new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(customCulture.IetfLanguageTag))
            );

            // Configure our services for dependency injection
            var serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);
            ServiceProvider = serviceCollection.BuildServiceProvider();

            // Manually open the Login window
            var loginView = ServiceProvider.GetRequiredService<LoginView>();
            loginView.Show();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // --- Central Services ---
            services.AddSingleton<IAuthService, AuthService>();

            // --- API Clients ---
            // Main PortalMirage API
            services.AddRefitClient<IPortalMirageApi>()
                    .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://localhost:7210"));

            // External Patient Info API
            services.AddRefitClient<PatientInfo.Api.Sdk.IPatientInfoApi>()
                    .ConfigureHttpClient(c => c.BaseAddress = new Uri("http://localhost:5104"));
            // Add this line to register your new PDF service
            services.AddSingleton<IPdfExportService, PdfExportService>();

            // --- ViewModels (Registered as Singletons to preserve state) ---
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<LoginViewModel>(); // Critical - was missing
            services.AddSingleton<DashboardViewModel>();
            services.AddSingleton<LogbooksViewModel>();
            services.AddSingleton<ReportsViewModel>();
            services.AddSingleton<AdminViewModel>();
            services.AddSingleton<SettingsViewModel>();
            services.AddSingleton<UserManagementViewModel>();
            services.AddSingleton<ShiftManagementViewModel>();
            services.AddSingleton<MasterListViewModel>();
            services.AddSingleton<AuditLogViewModel>();
            services.AddSingleton<CalibrationLogViewModel>();
            services.AddSingleton<KitValidationViewModel>();
            services.AddSingleton<SampleStorageViewModel>();
            services.AddSingleton<HandoverViewModel>();
            services.AddSingleton<MachineBreakdownViewModel>();
            services.AddSingleton<MediaSterilityViewModel>();
            services.AddSingleton<RepeatSampleViewModel>();
            services.AddSingleton<DailyTaskLogViewModel>();
            services.AddSingleton<IInactivityService, InactivityService>();
            services.AddSingleton<SystemSettingsViewModel>();


            // --- Views (Registered as Transient so a new one is created each time) ---
            services.AddTransient<LoginView>();
            services.AddTransient<MainWindow>();

            // Additional Views for complete coverage
            services.AddTransient<DashboardView>();
            services.AddTransient<HandoverView>();
            services.AddTransient<MachineBreakdownView>();
            services.AddTransient<DailyTaskLogView>();
            services.AddTransient<SampleStorageView>();
            services.AddTransient<AuditLogView>();
            services.AddTransient<CalibrationLogView>();
            services.AddTransient<KitValidationView>();
            services.AddTransient<MediaSterilityView>();
            services.AddTransient<RepeatSampleView>();
            services.AddTransient<UserManagementView>();
            services.AddTransient<ShiftManagementView>();
            services.AddTransient<MasterListView>();
            services.AddTransient<AdminView>();
            services.AddTransient<LogbooksView>();
            services.AddTransient<ReportsView>();
            services.AddTransient<SettingsView>();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            // Clean up service provider
            (ServiceProvider as IDisposable)?.Dispose();

            // Release the mutex
            _mutex?.ReleaseMutex();
            _mutex?.Dispose();

            base.OnExit(e);
        }
    }
}