namespace DataLayer
{
    public interface IConnectionManager
    {
        TransactionAwareSQLiteConnection GetConnection();
        void Dispose();
        void ReturnConnection(TransactionAwareSQLiteConnection connection);
    }
}
