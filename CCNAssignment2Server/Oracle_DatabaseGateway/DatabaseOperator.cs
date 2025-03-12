using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace DatabaseGateway
{
    // This abstract class has been added to reduce duplication and shorten 
    // methods in DatabaseSelector, DatabaseInserter and DatabaseUpdater
    abstract class DatabaseOperator
    {
        protected OracleConnection GetConnection()
        {
            return DatabaseConnectionPool.GetInstance().AcquireConnection();
        }

        protected OracleCommand GetCommand(OracleConnection conn)
        {
            return new OracleCommand
            {
                Connection = conn,
                CommandText = GetSQL(),
                CommandType = CommandType.Text
            };
        }

        protected abstract string GetSQL();

        protected void ReleaseConnection(OracleConnection conn)
        {
            DatabaseConnectionPool.GetInstance().ReleaseConnection(conn);
        }
    }
}
