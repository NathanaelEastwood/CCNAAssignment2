using CCNAssignment2;

namespace DatabaseGateway
{
    public static class DatabaseCredentialsManager
    {
        private static DatabaseLoginWindow.DatabaseCredentials? _credentials;

        public static void SetCredentials(DatabaseLoginWindow.DatabaseCredentials credentials)
        {
            _credentials = credentials;
        }

        public static DatabaseLoginWindow.DatabaseCredentials GetCredentials()
        {
            if (_credentials == null)
            {
                throw new System.InvalidOperationException("Database credentials have not been set.");
            }
            return _credentials;
        }
    }
} 