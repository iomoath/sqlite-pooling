using System.Data.SQLite;
using System.Threading;

namespace DataLayer
{
    public class TransactionAwareSQLiteConnection
    {
        private readonly SQLiteConnection _connection;
        private long _transactionInProgress;
        private volatile bool _disposed;

        public SQLiteConnection Connection => _connection;
        public bool IsDisposed => _disposed;

        public long LastInsertRowId => _connection.LastInsertRowId;

        public TransactionAwareSQLiteConnection(string connectionString)
        {
            _connection = new SQLiteConnection(connectionString);
            //connection.Open();
        }

        public SQLiteTransaction BeginTransaction()
        {
            Interlocked.Exchange(ref _transactionInProgress, 1);
            return _connection.BeginTransaction();
        }

        public void Commit()
        {
            Interlocked.Exchange(ref _transactionInProgress, 0); // set to 0 (false)
        }

        public void Commit(SQLiteTransaction transaction)
        {
            transaction.Commit();
            Interlocked.Exchange(ref _transactionInProgress, 0);
        }

        public void Rollback()
        {
            Interlocked.Exchange(ref _transactionInProgress, 0);
        }

        public bool IsTransactionInProgress()
        {
            return Interlocked.Read(ref _transactionInProgress) == 1;
        }

        public void Dispose()
        {
            _disposed = true;
            _connection?.Dispose();
        }
    }
}
