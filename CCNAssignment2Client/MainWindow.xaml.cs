using System.Windows;
using System.Windows.Controls;
using CCNAssignment2.ServerGateway;
using Entities;
using UseCase;

namespace CCNAssignment2;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    private readonly IDatabaseGatewayFacade _databaseGatewayFacade;
    private Book? _selectedBook;
    private Member? _selectedMember;
    private Loan? _selectedLoan;

    public MainWindow()
    {
        InitializeComponent();
        _databaseGatewayFacade = new ServerGatewayFacade();
        
        // Show login window when created through XAML
        var loginWindow = new DatabaseLoginWindow();
        if (loginWindow.ShowDialog() == true)
        {
            var credentials = loginWindow.Credentials;
            InitializeDatabase();
            Loaded += MainWindow_Loaded;
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
            var booksData = _databaseGatewayFacade.GetAllBooks();
            BooksGrid.ItemsSource = booksData;

            // Load members
            var membersData = _databaseGatewayFacade.GetAllMembers();
            MembersGrid.ItemsSource = membersData;

            // Load loans
            var loansData = _databaseGatewayFacade.GetCurrentLoans();
            LoansGrid.ItemsSource = loansData;
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
            _databaseGatewayFacade.InitialiseDatabase();
            _databaseGatewayFacade.AddBook(new Book(1, "Beff Jezos", "Amazong 101", "A1230", BookState.Available));
            _databaseGatewayFacade.AddBook(new Book(2, "Beff Jezos", "Amazong 102", "A1231", BookState.Available));
            _databaseGatewayFacade.AddBook(new Book(3, "Beff Jezos", "Amazong 103", "A1232", BookState.Available));

            _databaseGatewayFacade.AddMember(new Member(1, "Nathanael"));
            _databaseGatewayFacade.AddMember(new Member(2, "Grace"));
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
        _selectedBook = BooksGrid.SelectedItem as Book;
        UpdateBorrowButtonState();
    }

    private void MembersGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selectedMember = MembersGrid.SelectedItem as Member;
        UpdateBorrowButtonState();
    }

    private void LoansGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _selectedLoan = LoansGrid.SelectedItem as Loan;
        UpdateRenewButtonState();
        UpdateReturnButtonState();
    }

    private void UpdateBorrowButtonState()
    {
        if (BorrowBookButton != null)
        {
            bool canBorrow = _selectedBook != null && 
                           _selectedMember != null && 
                           _selectedBook.State == BookState.Available;
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
                var returnedData = _databaseGatewayFacade.CreateLoan(new Loan(0, _selectedMember, _selectedBook,
                    DateTime.Now, DateTime.Today.AddDays(7)));
                ResultsTextBlock.Text = $"Loan Added with Status Code: {returnedData}";
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
                Loan previousLoan = _selectedLoan;
                var returnedData = _databaseGatewayFacade.RenewLoan(new Loan(previousLoan.ID, previousLoan.Member,
                    previousLoan.Book, previousLoan.LoanDate, previousLoan.ReturnDate + new TimeSpan(1, 0, 0, 0)));
                ResultsTextBlock.Text = $"Loan renewed with Status Code: {returnedData}";
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
                var returnedData = _databaseGatewayFacade.EndLoan(_selectedLoan.Member.ID, _selectedLoan.Book.ID);
                ResultsTextBlock.Text = $"Book returned with Status Code: {returnedData}";
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
                var content = button.Content.ToString();

                // Handle different types of operations
                switch (content)
                {
                    case "Refresh Books":
                        BooksGrid.ItemsSource = _databaseGatewayFacade.GetAllBooks();
                        ResultsTextBlock.Text = "Books list refreshed";
                        break;

                    case "Refresh Members":
                        MembersGrid.ItemsSource = _databaseGatewayFacade.GetAllMembers();
                        ResultsTextBlock.Text = "Members list refreshed";
                        break;

                    case "Refresh Loans":
                        LoansGrid.ItemsSource = _databaseGatewayFacade.GetCurrentLoans();
                        ResultsTextBlock.Text = "Loans list refreshed";
                        break;

                    case "Return Book":
                    case "Borrow Book":
                        ResultsTextBlock.Text = "Reloading all  list data";
                        LoadInitialData(); // Refresh all lists
                        break;
                }
            }
            catch (Exception ex)
            {
                ResultsTextBlock.Text = $"Error: {ex.Message}";
            }
        }
    }
}