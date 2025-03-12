using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;

namespace DatabaseGateway
{

    // This class implements the Object Pool and Singleton design patterns
    class DatabaseConnectionPool
    {

        private static DatabaseConnectionPool instance = new DatabaseConnectionPool(1);

        public static DatabaseConnectionPool GetInstance()
        {
            return instance;
        }
        private List<OracleConnection> availableConnections;
        private List<OracleConnection> busyConnections;

        protected DatabaseConnectionPool(int sizeOfPool)
        {
            availableConnections = new List<OracleConnection>(sizeOfPool);
            busyConnections = new List<OracleConnection>(sizeOfPool);

            for (int i = 0; i < sizeOfPool; i++)
            {
                availableConnections.Add(CreateConnection());
            }
        }

        ~DatabaseConnectionPool()
        {
            foreach (OracleConnection conn in availableConnections)
            {
                CloseConnection(conn);
            }
            availableConnections.Clear();

            foreach (OracleConnection conn in busyConnections)
            {
                CloseConnection(conn);
            }
            busyConnections.Clear();
        }

        public OracleConnection AcquireConnection()
        {
            if (availableConnections.Count > 0)
            {
                OracleConnection conn = availableConnections[0];
                availableConnections.RemoveAt(0);
                busyConnections.Add(conn);
                return conn;
            }

            return null;
        }

        private void CloseConnection(OracleConnection conn)
        {
            if (conn != null)
            {
                try
                {
                    conn.Close();
                }
                catch (Exception e)
                {
                    throw new Exception("ERROR: closure of database connection failed", e);
                }
            }
        }

        protected OracleConnection CreateConnection()
        {
            string DB_CONNECTION_STRING =
                "Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=vmp3wstuoradb1.staff.staffs.ac.uk)(PORT=1521))" +
                "(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=STORAPDB.staff.staffs.ac.uk)));enlist=dynamic;" +
                $"User Id=e026008k;Password=e026008k;";

            OracleConnection conn;

            try
            {
                conn = new OracleConnection(DB_CONNECTION_STRING);
                conn.Open();
            }
            catch (Exception e)
            {
                throw new Exception("ERROR: connection to database failed", e);
            }

            return conn;
        }

        public void ReleaseConnection(OracleConnection conn)
        {
            if (busyConnections.Contains(conn))
            {
                busyConnections.Remove(conn);
                availableConnections.Add(conn);
            }
        }
    }
}
