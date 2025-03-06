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
    private BookDTO? selectedBook;
    private MemberDTO? selectedMember;

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
            BooksGrid.ItemsSource = booksData.ViewData;

            // Load members
            var membersCommand = commandFactory.CreateCommand(RequestUseCase.VIEW_ALL_MEMBERS);
            var membersData = membersCommand.Execute();
            MembersGrid.ItemsSource = membersData.ViewData;

            // Load loans
            var loansCommand = commandFactory.CreateCommand(RequestUseCase.VIEW_CURRENT_LOANS);
            var loansData = loansCommand.Execute();
            LoansGrid.ItemsSource = loansData.ViewData;
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
        if (FindName("ReturnBookButton") is Button returnButton)
            returnButton.Click += Button_Click;
        
        if (FindName("RenewLoanButton") is Button renewButton)
            renewButton.Click += Button_Click;
        
        if (FindName("ViewAllBooksButton") is Button viewBooksButton)
            viewBooksButton.Click += Button_Click;
        
        if (FindName("ViewAllMembersButton") is Button viewMembersButton)
            viewMembersButton.Click += Button_Click;
        
        if (FindName("ViewCurrentLoansButton") is Button viewLoansButton)
            viewLoansButton.Click += Button_Click;
    }

    private void BooksGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        selectedBook = BooksGrid.SelectedItem as BookDTO;
        UpdateBorrowButtonState();
    }

    private void MembersGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        selectedMember = MembersGrid.SelectedItem as MemberDTO;
        UpdateBorrowButtonState();
    }

    private void UpdateBorrowButtonState()
    {
        if (BorrowBookButton != null)
        {
            bool canBorrow = selectedBook != null && 
                           selectedMember != null && 
                           selectedBook.State == "Available";
            BorrowBookButton.IsEnabled = canBorrow;
        }
    }

    private void BorrowBookButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new BorrowBookDialog(selectedBook, selectedMember);
        if (dialog.ShowDialog() == true && selectedBook != null && selectedMember != null)
        {
            try
            {
                commandFactory.SetBorrowParameters(selectedMember.ID, selectedBook.Id);
                var command = commandFactory.CreateCommand(RequestUseCase.BORROW_BOOK);
                var returnedData = command.Execute();
                ResultsTextBlock.Text = returnedData.ViewData[0].ToString();
                LoadInitialData(); // Refresh all lists
            }
            catch (Exception ex)
            {
                ResultsTextBlock.Text = $"Error: {ex.Message}";
            }
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
                UiViewData returnedData = command.Execute();

                // Handle different types of operations
                switch (useCase)
                {
                    case RequestUseCase.VIEW_ALL_BOOKS:
                        BooksGrid.ItemsSource = returnedData.ViewData;
                        ResultsTextBlock.Text = "Books list refreshed";
                        break;

                    case RequestUseCase.VIEW_ALL_MEMBERS:
                        MembersGrid.ItemsSource = returnedData.ViewData;
                        ResultsTextBlock.Text = "Members list refreshed";
                        break;

                    case RequestUseCase.VIEW_CURRENT_LOANS:
                        LoansGrid.ItemsSource = returnedData.ViewData;
                        ResultsTextBlock.Text = "Loans list refreshed";
                        break;

                    case RequestUseCase.RETURN_BOOK:
                    case RequestUseCase.RENEW_LOAN:
                        ResultsTextBlock.Text = returnedData.ViewData[0].ToString();
                        LoadInitialData(); // Refresh all lists
                        break;

                    default:
                        ResultsTextBlock.Text = returnedData.ViewData[0].ToString();
                        break;
                }
            }
            catch (Exception ex)
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
            "Refresh Books" => RequestUseCase.VIEW_ALL_BOOKS,
            "Refresh Members" => RequestUseCase.VIEW_ALL_MEMBERS,
            "Refresh Loans" => RequestUseCase.VIEW_CURRENT_LOANS,
            _ => throw new ArgumentException($"Unknown operation: {content}")
        };
    }
}