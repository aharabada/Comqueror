using Comqueror.Properties;
using System.Globalization;
using System.Windows;

namespace Comqueror;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private void Application_Startup(object sender, StartupEventArgs e)
    {
        CultureInfo.CurrentCulture = new CultureInfo(Settings.Default.Culture);
        CultureInfo.CurrentUICulture = new CultureInfo(Settings.Default.Culture);
    }
}
