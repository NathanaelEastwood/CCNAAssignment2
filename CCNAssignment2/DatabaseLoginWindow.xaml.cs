using System.Windows;

namespace CCNAssignment2
{
    public partial class DatabaseLoginWindow : Window
    {
        public class DatabaseCredentials
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        public DatabaseCredentials Credentials { get; private set; }

        public DatabaseLoginWindow()
        {
            InitializeComponent();
        }

        private void ConnectButton_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateInputs())
            {
                Credentials = new DatabaseCredentials
                {
                    Username = UsernameTextBox.Text,
                    Password = PasswordBox.Password
                };

                DialogResult = true;
                Close();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrWhiteSpace(UsernameTextBox.Text))
            {
                MessageBox.Show("Please enter a username.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(PasswordBox.Password))
            {
                MessageBox.Show("Please enter a password.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            return true;
        }
    }
} 