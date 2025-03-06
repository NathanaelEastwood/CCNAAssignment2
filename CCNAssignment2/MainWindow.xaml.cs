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
    private readonly CommandFactory _commandFactory;
    private BookDTO? _selectedBook;
    private MemberDTO? _selectedMember;
    private LoanDTO? _selectedLoan;

    public MainWindow()
    {
        InitializeComponent();
        _commandFactory = new CommandFactory();
        
        // Show login window when created through XAML
        var loginWindow = new DatabaseLoginWindow();
        if (loginWindow.ShowDialog() == true)
        {
            var credentials = loginWindow.Credentials;
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
            var booksCommand = _commandFactory.CreateCommand(RequestUseCase.VIEW_ALL_BOOKS);
            var booksData = booksCommand.Execute();
            BooksGrid.ItemsSource = booksData.ViewData;

            // Load members
            var membersCommand = _commandFactory.CreateCommand(RequestUseCase.VIEW_ALL_MEMBERS);
            var membersData = membersCommand.Execute();
            MembersGrid.ItemsSource = membersData.ViewData;

            // Load loans
            var loansCommand = _commandFactory.CreateCommand(RequestUseCase.VIEW_CURRENT_LOANS);
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
            var command = _commandFactory.CreateCommand(RequestUseCase.INITIALISE_DATABASE);
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
        if (FindName("ViewAllBooksButton") is Button viewBooksButton)
            viewBooksButton.Click += Button_Click;
        
        if (FindName("ViewAllMembersButton") is Button viewMembersButton)
            viewMembersButton.Click += Button_Click;
        
        if (FindName("ViewCurrentLoansButton") is Button viewLoansButton)
            viewLoansButton.Click += Button_Click;
    }

    private void BooksGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selectedBook = BooksGrid.SelectedItem as BookDTO;
        UpdateBorrowButtonState();
    }

    private void MembersGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selectedMember = MembersGrid.SelectedItem as MemberDTO;
        UpdateBorrowButtonState();
    }

    private void LoansGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selectedLoan = LoansGrid.SelectedItem as LoanDTO;
        UpdateRenewButtonState();
        UpdateReturnButtonState();
    }

    private void UpdateBorrowButtonState()
    {
        if (BorrowBookButton != null)
        {
            bool canBorrow = _selectedBook != null && 
                           _selectedMember != null && 
                           _selectedBook.State == "Available";
            BorrowBookButton.IsEnabled = canBorrow;
        }
    }

    private void UpdateRenewButtonState()
    {
        if (RenewLoanButton != null)
        {
            bool canRenew = _selectedLoan != null;
            RenewLoanButton.IsEnabled = canRenew;
        }
    }

    private void UpdateReturnButtonState()
    {
        if (ReturnBookButton != null)
        {
            ReturnBookButton.IsEnabled = _selectedLoan != null;
        }
    }

    private void BorrowBookButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new BorrowBookDialog(_selectedBook, _selectedMember);
        if (dialog.ShowDialog() == true && _selectedBook != null && _selectedMember != null)
        {
            try
            {
                _commandFactory.SetLoanParameters(_selectedMember.ID, _selectedBook.Id);
                var command = _commandFactory.CreateCommand(RequestUseCase.BORROW_BOOK);
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

    private void RenewLoanButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedLoan != null)
        {
            try
            {
                _commandFactory.SetLoanParameters(_selectedLoan.Member.ID, _selectedLoan.Book.Id);
                var command = _commandFactory.CreateCommand(RequestUseCase.RENEW_LOAN);
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

    private void ReturnBookButton_Click(object sender, RoutedEventArgs e)
    {
        if (_selectedLoan != null)
        {
            try
            {
                _commandFactory.SetLoanParameters(_selectedLoan.Member.ID, _selectedLoan.Book.Id);
                var command = _commandFactory.CreateCommand(RequestUseCase.RETURN_BOOK);
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
                var command = _commandFactory.CreateCommand(useCase);
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