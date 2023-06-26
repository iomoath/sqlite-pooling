using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Threading;

namespace DataLayer
{
    public class SQLiteConnectionManager : IConnectionManager
    {
        private static readonly Lazy<SQLiteConnectionManager> LazyInstance =
            new Lazy<SQLiteConnectionManager>(() => new SQLiteConnectionManager());

        public static SQLiteConnectionManager Instance => LazyInstance.Value;


        private readonly Queue<Tuple<TransactionAwareSQLiteConnection, DateTime>> _connectionPoolQueue;
        private readonly object _connectionPoolQueueLock = new object();
        private const int MaxPoolSize = 50;
        private volatile bool _disposed;
        private int _currentPoolSize;
        private readonly System.Timers.Timer _cleanupTimer;


        /// <summary>
        /// An empty pool will be created. Connections are created on demand.
        /// </summary>
        public SQLiteConnectionManager()
        {
            _connectionPoolQueue = new Queue<Tuple<TransactionAwareSQLiteConnection, DateTime>>(MaxPoolSize);

            _cleanupTimer = new System.Timers.Timer(10 * 60 * 1000); // run every 10 minutes
            _cleanupTimer.Elapsed += CleanupTimerElapsed;
            _cleanupTimer.AutoReset = true;
            _cleanupTimer.Start();
        }

        /// <summary>
        /// Disposing connections that were not used for 15 minutes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CleanupTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            lock (_connectionPoolQueueLock)
            {
                while (_connectionPoolQueue.Count > 0 && (DateTime.UtcNow - _connectionPoolQueue.Peek().Item2).TotalMinutes > 15)
                {
                    var tup = _connectionPoolQueue.Dequeue();
                    tup.Item1.Dispose();
                    _currentPoolSize--;
                }
            }
        }

        private TransactionAwareSQLiteConnection CreateNewConnection()
        {
            var connection = new TransactionAwareSQLiteConnection(Env.SqliteDatabaseConnectionString);
            connection.Connection.Open();
            using (var command = new SQLiteCommand("PRAGMA foreign_keys=ON;", connection.Connection))
            {
                command.ExecuteNonQuery();
            }

            return connection;
        }

        public TransactionAwareSQLiteConnection GetConnection()
        {
            lock (_connectionPoolQueueLock)
            {
                while (_connectionPoolQueue.Count == 0 || _connectionPoolQueue.Peek().Item1.IsDisposed || _connectionPoolQueue.Peek().Item1.Connection.State != ConnectionState.Open)
                {
                    if (_disposed)
                    {
                        throw new ObjectDisposedException("The DB connection pool is is already disposed");
                    }

                    if (_currentPoolSize < MaxPoolSize)
                    {
                        var tup = new Tuple<TransactionAwareSQLiteConnection, DateTime>(CreateNewConnection(), DateTime.UtcNow);

                        _connectionPoolQueue.Enqueue(tup);
                        _currentPoolSize++;
                    }
                    else
                    {
                        Monitor.Wait(_connectionPoolQueueLock);
                    }
                }

                return _connectionPoolQueue.Dequeue().Item1;
            }
        }


        public void ReturnConnection(TransactionAwareSQLiteConnection connection)
        {
            if (connection == null || connection.IsDisposed)
            {
                return;
            }


            lock (_connectionPoolQueueLock)
            {
                if (_connectionPoolQueue.Count >= MaxPoolSize ||
                    connection.Connection.State != ConnectionState.Open)
                {
                    // Pool is full or connection is not open, discard it
                    connection.Dispose();
                }
                else
                {
                    // Clear any ongoing transaction before returning the connection to the pool
                    if (connection.IsTransactionInProgress())
                    {
                        connection.Rollback();
                    }

                    var tup = new Tuple<TransactionAwareSQLiteConnection, DateTime>(connection, DateTime.UtcNow);
                    _connectionPoolQueue.Enqueue(tup);
                    Monitor.Pulse(_connectionPoolQueueLock);
                }
            }
        }

        /// <summary>
        /// Should not be called from outside unless the application is exiting. Connections are expected to be returned to the pool
        /// </summary>
        public void Dispose()
        {
            lock (_connectionPoolQueueLock)
            {
                _disposed = true;

                while (_connectionPoolQueue.Count > 0)
                {
                    var tup = _connectionPoolQueue.Dequeue();
                    tup.Item1.Dispose();
                    _currentPoolSize--;
                }

                // wake up any waiting threads
                Monitor.PulseAll(_connectionPoolQueueLock);
            }

            _cleanupTimer.Stop();
            _cleanupTimer.Dispose();
        }
    }
}