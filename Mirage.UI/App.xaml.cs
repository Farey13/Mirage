using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Markup;

namespace Mirage.UI;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // 1. Create a culture info object for a region that is close to what we want (e.g., 'en-GB').
        var cultureInfo = new CultureInfo("en-GB");

        // 2. Clone it so we can make custom changes.
        var customCulture = (CultureInfo)cultureInfo.Clone();

        // 3. Set the specific date format pattern you requested.
        customCulture.DateTimeFormat.ShortDatePattern = "dd/MMM/yy";

        // 4. Set this new custom culture as the default for the application.
        Thread.CurrentThread.CurrentCulture = customCulture;
        Thread.CurrentThread.CurrentUICulture = customCulture;

        FrameworkElement.LanguageProperty.OverrideMetadata(
            typeof(FrameworkElement),
            new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(customCulture.IetfLanguageTag))
        );
    }
}