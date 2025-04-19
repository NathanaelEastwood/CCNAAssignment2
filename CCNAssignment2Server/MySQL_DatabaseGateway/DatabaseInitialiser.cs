using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Data;

namespace DatabaseGateway
{
    class DatabaseInitialiser
    {

        private readonly MySqlCommand createBookTable = new MySqlCommand
        {
            CommandText = "CREATE TABLE SDAM_Book(id integer auto_increment primary key, author varchar(20) not null, title varchar(20) not null, isbn varchar(13) not null)",
            CommandType = CommandType.Text
        };

        private readonly MySqlCommand createLoanTable = new MySqlCommand
        {
            CommandText = "CREATE TABLE SDAM_Loan ( id integer auto_increment primary key, memberId integer not null REFERENCES SDAM_Member(Id), bookId integer not null REFERENCES SDAM_Book(Id), loanDate TIMESTAMP null default null, dueDate TIMESTAMP null default null, returnDate TIMESTAMP null default null, numberOfRenewals integer)",
            CommandType = CommandType.Text
        };

        private readonly MySqlCommand createMemberTable = new MySqlCommand
        {
            CommandText = "CREATE TABLE SDAM_Member(id integer auto_increment primary key, name varchar(20) not null)",
            CommandType = CommandType.Text
        };

        private readonly MySqlCommand dropBookTable = new MySqlCommand
        {
            CommandText = "DROP TABLE IF EXISTS SDAM_Book",
            CommandType = CommandType.Text
        };

        private readonly MySqlCommand dropLoanTable = new MySqlCommand
        {
            CommandText = "DROP TABLE IF EXISTS SDAM_Loan",
            CommandType = CommandType.Text
        };

        private readonly MySqlCommand dropMemberTable = new MySqlCommand
        {
            CommandText = "DROP TABLE IF EXISTS SDAM_Member",
            CommandType = CommandType.Text
        };

        private readonly MySqlCommand populateMemberTable = new MySqlCommand
        {
            CommandText = "INSERT INTO SDAM_Member (Name) VALUES \n('Graham'),\n('Bob'),\n('Nathanael');\n",
            CommandType = CommandType.Text
        };

        private readonly MySqlCommand populateBookTable = new MySqlCommand
        {
            CommandText = "INSERT INTO sdam_book (author, title, isbn) VALUES \n('Test Author', 'Test Book 1', 'A12345'),\n('Test Author 2', 'Test Book 2', 'A12346'),\n('Test Author 3', 'Test Book 2', 'A12347');\n"
        };
        

        private readonly List<MySqlCommand> commandSequence;

        public DatabaseInitialiser()
        {
            commandSequence = new List<MySqlCommand>()
            {
                dropLoanTable,
                dropMemberTable,
                dropBookTable,

                createBookTable,
                createMemberTable,
                createLoanTable,
                
                populateMemberTable,
                populateBookTable
            };
        }

        public void Initialise()
        {
            DatabaseConnectionPool connectionPool = DatabaseConnectionPool.GetInstance();
            MySqlConnection conn = null;
            int retries = 0;
            const int maxRetries = 3;
            
            while (retries < maxRetries && conn == null)
            {
                conn = connectionPool.AcquireConnection();
                if (conn == null)
                {
                    System.Threading.Thread.Sleep(100); // Wait 100ms before retrying
                    retries++;
                }
            }
            
            if (conn == null)
            {
                throw new Exception("ERROR: Could not acquire a database connection for initialization");
            }

            try
            {
                foreach (MySqlCommand c in commandSequence)
                {
                    try
                    {
                        c.Connection = conn;
                        c.ExecuteNonQuery();
                    }
                    catch (Exception e)
                    {
                        throw new Exception("ERROR: SQL command failed\n" + e.StackTrace, e);
                    }
                }
            }
            finally
            {
                if (conn != null)
                {
                    connectionPool.ReleaseConnection(conn);
                }
            }
        }
    }
}
