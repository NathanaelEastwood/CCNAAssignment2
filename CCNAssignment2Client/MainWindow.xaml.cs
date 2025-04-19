using System.Windows;
using System.Windows.Controls;
using CCNAssignment2.ServerGateway;
using Entities;
using EntityLayer;
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
        Loaded += MainWindow_Loaded;
    }
    
    

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        SetupButtonHandlers();
        LoadInitialData();
        MyTcpClient.OnMessageReceived += HandleServerBroadcast;
    }
    
    private void HandleServerBroadcast(ResponseMessageDTO message)
    {
        // Ensure this executes on the WPF UI thread
        Dispatcher.Invoke(() =>
        {
            if (message.Book != null)
            {
                BooksGrid.ItemsSource = message.Book;
            }
            
            if (message.Loan != null)
            {
                LoansGrid.ItemsSource = message.Loan;
            }
            
            if (message.Member != null)
            {
                MembersGrid.ItemsSource = message.Member;
            }
        });
    }

    
    private void LoadInitialData()
    {
        try
        {
            _databaseGatewayFacade.GetAllBooks();
            _databaseGatewayFacade.GetAllMembers();
            _databaseGatewayFacade.GetCurrentLoans();
        }
        catch (Exception ex)
        {
            ResultsTextBlock.Text = $"Error loading initial data: {ex.Message}";
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
                _databaseGatewayFacade.CreateLoan(new Loan(0, _selectedMember, _selectedBook, DateTime.Now, DateTime.Today.AddDays(7))); 
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
                Loan newLoan = new Loan(previousLoan.ID, previousLoan.Member, previousLoan.Book, previousLoan.LoanDate, previousLoan.DueDate + new TimeSpan(7, 0, 0, 0));
                newLoan.NumberOfRenewals = previousLoan.NumberOfRenewals + 1;
                _databaseGatewayFacade.RenewLoan(newLoan);
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
                _databaseGatewayFacade.EndLoan(_selectedLoan.Member.ID, _selectedLoan.Book.ID);
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
                        _databaseGatewayFacade.GetAllBooks();
                        ResultsTextBlock.Text = "Books list refreshed";
                        break;

                    case "Refresh Members":
                        _databaseGatewayFacade.GetAllMembers();
                        ResultsTextBlock.Text = "Members list refreshed";
                        break;

                    case "Refresh Loans":
                        _databaseGatewayFacade.GetCurrentLoans();
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