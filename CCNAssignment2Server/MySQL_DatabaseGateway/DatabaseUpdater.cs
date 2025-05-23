﻿using MySql.Data.MySqlClient;

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
            MySqlConnection conn = null;
            MySqlCommand command = null;

            try
            {
                conn = GetConnection();
                command = GetCommand(conn);
                numRowsUpdated = DoUpdate(command, itemToUpdate);
            }
            catch (Exception e)
            {
                throw new Exception(e.Message, e);
            }
            finally
            {
                command?.Dispose();
                ReleaseConnection(conn);
            }

            return numRowsUpdated;
        }

        protected abstract int DoUpdate(MySqlCommand command, T itemToUpdate);
        protected override abstract string GetSQL();
    }
}