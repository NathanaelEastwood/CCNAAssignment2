using System.Windows;
using DTOs;

namespace CCNAssignment2;

public partial class BorrowBookDialog : Window
{
    public BookDTO? SelectedBook { get; private set; }
    public MemberDTO? SelectedMember { get; private set; }

    public BorrowBookDialog(BookDTO? book, MemberDTO? member)
    {
        InitializeComponent();
        SelectedBook = book;
        SelectedMember = member;

        if (SelectedBook != null)
        {
            SelectedBookText.Text = $"{SelectedBook.Title} by {SelectedBook.Author} (ISBN: {SelectedBook.ISBN})";
        }
        else
        {
            SelectedBookText.Text = "No book selected";
        }

        if (SelectedMember != null)
        {
            SelectedMemberText.Text = SelectedMember.Name;
        }
        else
        {
            SelectedMemberText.Text = "No member selected";
        }
    }

    private void BorrowButton_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedBook == null || SelectedMember == null)
        {
            MessageBox.Show("Please select both a book and a member.", "Selection Required", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
} 