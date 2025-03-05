using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CommandLineUI;
using CommandLineUI.Commands;
using DatabaseGateway;

namespace CCNAssignment2;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly CommandFactory commandFactory;
    private readonly DatabaseLoginWindow.DatabaseCredentials credentials;

    public MainWindow(DatabaseLoginWindow.DatabaseCredentials credentials)
    {
        InitializeComponent();
        this.credentials = credentials;
        DatabaseCredentialsManager.SetCredentials(credentials);
        commandFactory = new CommandFactory();
        InitializeDatabase();
        SetupButtonHandlers();
    }

    private void InitializeDatabase()
    {
        try
        {
            var command = commandFactory.CreateCommand(RequestUseCase.INITIALISE_DATABASE);
            command.Execute();
        }
        catch (System.Exception ex)
        {
            MessageBox.Show($"Error initializing database: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Application.Current.Shutdown();
        }
    }

    private void SetupButtonHandlers()
    {
        // Find all buttons in the window
        var buttons = FindVisualChildren<Button>(this);
        foreach (var button in buttons)
        {
            button.Click += Button_Click;
        }
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            try
            {
                int useCase = GetUseCaseFromButtonContent(button.Content.ToString());
                var command = commandFactory.CreateCommand(useCase);
                command.Execute();
                // Update results area with success message
                ResultsTextBlock.Text = $"Successfully executed: {button.Content}";
            }
            catch (System.Exception ex)
            {
                ResultsTextBlock.Text = $"Error: {ex.Message}";
            }
        }
    }

    private int GetUseCaseFromButtonContent(string content)
    {
        return content switch
        {
            "Borrow Book" => RequestUseCase.BORROW_BOOK,
            "Return Book" => RequestUseCase.RETURN_BOOK,
            "Renew Loan" => RequestUseCase.RENEW_LOAN,
            "View All Books" => RequestUseCase.VIEW_ALL_BOOKS,
            "View All Members" => RequestUseCase.VIEW_ALL_MEMBERS,
            "View Current Loans" => RequestUseCase.VIEW_CURRENT_LOANS,
            _ => throw new System.ArgumentException($"Unknown operation: {content}")
        };
    }

    // Helper method to find all controls of a specific type
    private System.Collections.Generic.IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
    {
        if (depObj == null) yield break;

        for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
        {
            var child = VisualTreeHelper.GetChild(depObj, i);
            if (child != null && child is T t)
                yield return t;

            foreach (T childOfChild in FindVisualChildren<T>(child))
                yield return childOfChild;
        }
    }
}