using System.Windows;
using System.Windows.Controls;
using CCNAssignment2.WPFPresenters;
using CommandLineUI;
using CommandLineUI.Commands;
using DatabaseGateway;
using DTOs;

namespace CCNAssignment2;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly CommandFactory commandFactory;
    private readonly DatabaseLoginWindow.DatabaseCredentials? credentials;

    public MainWindow()
    {
        InitializeComponent();
        commandFactory = new CommandFactory();
        
        // Show login window when created through XAML
        var loginWindow = new DatabaseLoginWindow();
        if (loginWindow.ShowDialog() == true)
        {
            credentials = loginWindow.Credentials;
            DatabaseCredentialsManager.SetCredentials(credentials);
            InitializeDatabase();
            this.Loaded += MainWindow_Loaded;
        }
        else
        {
            Application.Current.Shutdown();
        }
    }

    public MainWindow(DatabaseLoginWindow.DatabaseCredentials credentials)
    {
        InitializeComponent();
        this.credentials = credentials;
        DatabaseCredentialsManager.SetCredentials(credentials);
        commandFactory = new CommandFactory();
        InitializeDatabase();
        this.Loaded += MainWindow_Loaded;
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        SetupButtonHandlers();
        LoadInitialData();
    }

    private void LoadInitialData()
    {
        try
        {
            // Load books
            var booksCommand = commandFactory.CreateCommand(RequestUseCase.VIEW_ALL_BOOKS);
            var booksData = booksCommand.Execute();
            DisplayTableFromDTO(booksData.ViewData, "Books", BooksGrid, BooksTitle);

            // Load members
            var membersCommand = commandFactory.CreateCommand(RequestUseCase.VIEW_ALL_MEMBERS);
            var membersData = membersCommand.Execute();
            DisplayTableFromDTO(membersData.ViewData, "Members", MembersGrid, MembersTitle);

            // Load loans
            var loansCommand = commandFactory.CreateCommand(RequestUseCase.VIEW_CURRENT_LOANS);
            var loansData = loansCommand.Execute();
            DisplayTableFromDTO(loansData.ViewData, "Current Loans", LoansGrid, LoansTitle);
        }
        catch (Exception ex)
        {
            ResultsTextBlock.Text = $"Error loading initial data: {ex.Message}";
        }
    }

    private void InitializeDatabase()
    {
        try
        {
            var command = commandFactory.CreateCommand(RequestUseCase.INITIALISE_DATABASE);
            command.Execute();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error initializing database: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Application.Current.Shutdown();
        }
    }

    private void SetupButtonHandlers()
    {
        // Get buttons by their names from XAML
        if (this.FindName("BorrowBookButton") is Button borrowButton)
            borrowButton.Click += Button_Click;
        
        if (this.FindName("ReturnBookButton") is Button returnButton)
            returnButton.Click += Button_Click;
        
        if (this.FindName("RenewLoanButton") is Button renewButton)
            renewButton.Click += Button_Click;
        
        if (this.FindName("ViewAllBooksButton") is Button viewBooksButton)
            viewBooksButton.Click += Button_Click;
        
        if (this.FindName("ViewAllMembersButton") is Button viewMembersButton)
            viewMembersButton.Click += Button_Click;
        
        if (this.FindName("ViewCurrentLoansButton") is Button viewLoansButton)
            viewLoansButton.Click += Button_Click;
    }

    private void Button_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button)
        {
            try
            {
                int useCase = GetUseCaseFromButtonContent(button.Content.ToString());
                var command = commandFactory.CreateCommand(useCase);
                UiViewData returnedData = command.Execute();

                // Handle different types of operations
                switch (useCase)
                {
                    case RequestUseCase.VIEW_ALL_BOOKS:
                        DisplayTableFromDTO(returnedData.ViewData, "Books", BooksGrid, BooksTitle);
                        ResultsTextBlock.Text = "Books list refreshed";
                        break;

                    case RequestUseCase.VIEW_ALL_MEMBERS:
                        DisplayTableFromDTO(returnedData.ViewData, "Members", MembersGrid, MembersTitle);
                        ResultsTextBlock.Text = "Members list refreshed";
                        break;

                    case RequestUseCase.VIEW_CURRENT_LOANS:
                        DisplayTableFromDTO(returnedData.ViewData, "Current Loans", LoansGrid, LoansTitle);
                        ResultsTextBlock.Text = "Loans list refreshed";
                        break;

                    case RequestUseCase.BORROW_BOOK:
                    case RequestUseCase.RETURN_BOOK:
                    case RequestUseCase.RENEW_LOAN:
                        // Refresh all lists after loan operations
                        LoadInitialData();
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                ResultsTextBlock.Text = $"Error: {ex.Message}";
            }
        }
    }

    private void DisplayTableFromDTO(List<IDto> items, string title, Grid targetGrid, TextBlock titleBlock)
    {
        if (items.Count == 0)
        {
            titleBlock.Text = $"{title} - No items available";
            return;
        }

        targetGrid.Children.Clear();
        targetGrid.RowDefinitions.Clear();
        targetGrid.ColumnDefinitions.Clear();
        
        // Set the title
        titleBlock.Text = title;

        // Get properties of the first item
        var properties = items[0].GetType().GetProperties();
        
        // Add column definitions
        foreach (var property in properties)
        {
            targetGrid.ColumnDefinitions.Add(new ColumnDefinition 
            { 
                Width = property.PropertyType == typeof(string) ? 
                    new GridLength(1, GridUnitType.Star) : 
                    GridLength.Auto 
            });
        }

        // Add header row
        targetGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        for (int i = 0; i < properties.Length; i++)
        {
            var headerText = new TextBlock
            {
                Text = properties[i].Name,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(5),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            Grid.SetColumn(headerText, i);
            Grid.SetRow(headerText, 0);
            targetGrid.Children.Add(headerText);
        }

        // Add data rows
        for (int rowIndex = 0; rowIndex < items.Count; rowIndex++)
        {
            targetGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            var item = items[rowIndex];
            
            for (int colIndex = 0; colIndex < properties.Length; colIndex++)
            {
                var cellText = new TextBlock
                {
                    Text = properties[colIndex].GetValue(item)?.ToString() ?? "",
                    Margin = new Thickness(5),
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                Grid.SetColumn(cellText, colIndex);
                Grid.SetRow(cellText, rowIndex + 1); // +1 because row 0 is the header
                targetGrid.Children.Add(cellText);
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
            "Refresh Books" => RequestUseCase.VIEW_ALL_BOOKS,
            "Refresh Members" => RequestUseCase.VIEW_ALL_MEMBERS,
            "Refresh Loans" => RequestUseCase.VIEW_CURRENT_LOANS,
            _ => throw new ArgumentException($"Unknown operation: {content}")
        };
    }
}