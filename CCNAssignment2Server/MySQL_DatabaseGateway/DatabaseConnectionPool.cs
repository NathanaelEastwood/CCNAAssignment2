using MySql.Data.MySqlClient;
using System.Collections.Generic;
using System.Threading;

namespace DatabaseGateway
{

    // This class implements the Object Pool and Singleton design patterns
    class DatabaseConnectionPool
    {

        private static DatabaseConnectionPool instance = new DatabaseConnectionPool(10);

        public static DatabaseConnectionPool GetInstance()
        {
            return instance;
        }
        private List<MySqlConnection> availableConnections;
        private List<MySqlConnection> busyConnections;
        private readonly object lockObject = new object();
        private readonly int maxWaitTimeMs = 5000;

        protected DatabaseConnectionPool(int sizeOfPool)
        {
            availableConnections = new List<MySqlConnection>(sizeOfPool);
            busyConnections = new List<MySqlConnection>(sizeOfPool);

            for (int i = 0; i < sizeOfPool; i++)
            {
                availableConnections.Add(CreateConnection());
            }
        }

        ~DatabaseConnectionPool()
        {
            foreach (MySqlConnection conn in availableConnections)
            {
                CloseConnection(conn);
            }
            availableConnections.Clear();

            foreach (MySqlConnection conn in busyConnections)
            {
                CloseConnection(conn);
            }
            busyConnections.Clear();
        }

        public MySqlConnection AcquireConnection()
        {
            int waitTime = 0;
            int waitInterval = 100;
            
            while (waitTime < maxWaitTimeMs)
            {
                lock (lockObject)
                {
                    if (availableConnections.Count > 0)
                    {
                        MySqlConnection conn = availableConnections[0];
                        availableConnections.RemoveAt(0);
                        busyConnections.Add(conn);
                        return conn;
                    }
                }
                
                Thread.Sleep(waitInterval);
                waitTime += waitInterval;
            }
            
            MySqlConnection newConn = CreateConnection();
            lock (lockObject)
            {
                busyConnections.Add(newConn);
            }
            return newConn;
        }

        private void CloseConnection(MySqlConnection conn)
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

        protected MySqlConnection CreateConnection()
        {
            string DB_CONNECTION_STRING
                = "Server=localhost;Port=3306;Database=ccnassignment2;Uid=root;Pwd=root;";

            MySqlConnection conn = null;

            try
            {
                conn = new MySqlConnection(DB_CONNECTION_STRING);
                conn.Open();
            }
            catch (Exception e)
            {
                throw new Exception("ERROR: connection to database failed", e);
            }

            return conn;
        }

        public void ReleaseConnection(MySqlConnection conn)
        {
            if (conn == null) return;
            
            lock (lockObject)
            {
                if (busyConnections.Contains(conn))
                {
                    busyConnections.Remove(conn);
                    availableConnections.Add(conn);
                }
                else
                {
                    CloseConnection(conn);
                }
            }
        }
    }
}
