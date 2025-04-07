using MySql.Data.MySqlClient;
using System.Data;
using System.Threading;

namespace DatabaseGateway
{
    // This abstract class has been added to reduce duplication and shorten 
    // methods in DatabaseSelector, DatabaseInserter and DatabaseUpdater
    abstract class DatabaseOperator
    {
        private const int MaxRetries = 3;
        private const int RetryDelayMs = 100;

        protected MySqlConnection GetConnection()
        {
            int retries = 0;
            MySqlConnection conn = null;
            
            while (retries < MaxRetries)
            {
                conn = DatabaseConnectionPool.GetInstance().AcquireConnection();
                if (conn != null)
                {
                    return conn;
                }
                
                // If we couldn't get a connection, wait a bit and try again
                Thread.Sleep(RetryDelayMs);
                retries++;
            }
            
            throw new Exception("ERROR: Could not acquire a database connection after multiple attempts");
        }

        protected MySqlCommand GetCommand(MySqlConnection conn)
        {
            var command = new MySqlCommand
            {
                Connection = conn,
                CommandText = GetSQL(),
                CommandType = CommandType.Text
            };
            return command;
        }

        protected abstract string GetSQL();

        protected void ReleaseConnection(MySqlConnection conn)
        {
            if (conn != null)
            {
                DatabaseConnectionPool.GetInstance().ReleaseConnection(conn);
            }
        }
    }
}
