using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using System.Data;

namespace DatabaseGateway
{
    class DatabaseInitialiser
    {

        private readonly OracleCommand createBookSeq = new OracleCommand
        {
            CommandText = "CREATE SEQUENCE SDAM_Book_Seq START WITH 1001 INCREMENT BY 1",
            CommandType = CommandType.Text
        };

        private readonly OracleCommand createBookTable = new OracleCommand
        {
            CommandText = "CREATE TABLE SDAM_Book(id number primary key, author varchar2(20) not null, title varchar2(20) not null, isbn varchar2(13) not null)",
            CommandType = CommandType.Text
        };

        private readonly OracleCommand createLoanSeq = new OracleCommand
        {
            CommandText = "CREATE SEQUENCE SDAM_Loan_Seq START WITH 1 INCREMENT BY 1",
            CommandType = CommandType.Text
        };

        private readonly OracleCommand createLoanTable = new OracleCommand
        {
            CommandText = "CREATE TABLE SDAM_Loan (id number primary key, memberId number not null CONSTRAINT mem_FK REFERENCES SDAM_Member(Id), bookId number not null CONSTRAINT bk_FK REFERENCES SDAM_Book(Id), loanDate TIMESTAMP not null, dueDate TIMESTAMP not null, returnDate TIMESTAMP, numberOfRenewals number)",
            CommandType = CommandType.Text
        };

        private readonly OracleCommand createMemberSeq = new OracleCommand
        {
            CommandText = "CREATE SEQUENCE SDAM_Member_Seq START WITH 1 INCREMENT BY 1",
            CommandType = CommandType.Text
        };

        private readonly OracleCommand createMemberTable = new OracleCommand
        {
            CommandText = "CREATE TABLE SDAM_Member(id number primary key, name varchar2(20) not null)",
            CommandType = CommandType.Text
        };

        private readonly OracleCommand dropBookSeq = new OracleCommand
        {
            CommandText = "DROP SEQUENCE SDAM_Book_Seq",
            CommandType = CommandType.Text
        };

        private readonly OracleCommand dropBookTable = new OracleCommand
        {
            CommandText = "DROP TABLE SDAM_Book",
            CommandType = CommandType.Text
        };

        private readonly OracleCommand dropLoanSeq = new OracleCommand
        {
            CommandText = "DROP SEQUENCE SDAM_Loan_Seq",
            CommandType = CommandType.Text
        };

        private readonly OracleCommand dropLoanTable = new OracleCommand
        {
            CommandText = "DROP TABLE SDAM_Loan",
            CommandType = CommandType.Text
        };

        private readonly OracleCommand dropMemberSeq = new OracleCommand
        {
            CommandText = "DROP SEQUENCE SDAM_Member_Seq",
            CommandType = CommandType.Text
        };

        private readonly OracleCommand dropMemberTable = new OracleCommand
        {
            CommandText = "DROP TABLE SDAM_Member",
            CommandType = CommandType.Text
        };

        private readonly List<OracleCommand> commandSequence;

        public DatabaseInitialiser()
        {
            commandSequence = new List<OracleCommand>()
            {
                dropLoanTable,
                dropLoanSeq,

                dropMemberTable,
                dropMemberSeq,

                dropBookTable,
                dropBookSeq,

                createBookTable,
                createBookSeq,

                createMemberTable,
                createMemberSeq,

                createLoanTable,
                createLoanSeq
            };
        }

        public void Initialise()
        {
            DatabaseConnectionPool connectionPool = DatabaseConnectionPool.GetInstance();
            OracleConnection conn = connectionPool.AcquireConnection();

            foreach (OracleCommand c in commandSequence)
            {
                //Console.WriteLine(c.CommandText);
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

            connectionPool.ReleaseConnection(conn);
        }
    }
}
