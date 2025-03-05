using Oracle.ManagedDataAccess.Client;

namespace DatabaseGateway
{

    // This class and its subclasses implement the Table Data Gateway 
    // and Template Method design patterns
    abstract class DatabaseUpdater<T> : DatabaseOperator, IUpdater<T>
    {

        // This method is a Template Method
        public int Update(T itemToUpdate)
        {
            int numRowsUpdated = 0;

            OracleConnection conn = GetConnection();

            OracleCommand command = GetCommand(conn);

            try
            {
                numRowsUpdated = DoUpdate(command, itemToUpdate);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message, e);
            }

            ReleaseConnection(conn);
            return numRowsUpdated;
        }

        protected abstract int DoUpdate(OracleCommand command, T itemToUpdate);
        protected override abstract string GetSQL();
    }
}