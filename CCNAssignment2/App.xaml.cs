using System.Configuration;
using System.Data;
using System.Windows;

namespace CCNAssignment2;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var loginWindow = new DatabaseLoginWindow();
        if (loginWindow.ShowDialog() == true)
        {
            var mainWindow = new MainWindow(loginWindow.Credentials);
            mainWindow.Show();
        }
        else
        {
            Shutdown();
        }
    }
}