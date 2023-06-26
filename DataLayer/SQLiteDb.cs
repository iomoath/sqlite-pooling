using System;
using System.Data;
using System.Data.SQLite;

namespace DataLayer
{
    public class SQLiteDb
    {
        private readonly IConnectionManager _dbConnectionManager;

        public SQLiteDb()
        {
            _dbConnectionManager = SQLiteConnectionManager.Instance;
        }

        public bool InitDb()
        {
            var dbUtil = new SQLiteDbUtils();
            if (!dbUtil.CreateDbFile()) return false;
            if (!dbUtil.CreateTables()) return false;

            return true;
        }

        public void Vacuum()
        {
            var connection = _dbConnectionManager.GetConnection();
            using (var command = new SQLiteCommand("vacuum;", connection.Connection))
            {
                command.ExecuteNonQuery();
            }
            _dbConnectionManager.ReturnConnection(connection);
        }

        public long ExecuteNonQueryCmd(SQLiteCommand cmd)
        {
            long lastInsertionId;
            var connection = _dbConnectionManager.GetConnection();

            try
            {
                using (var transaction = connection.BeginTransaction())
                {
                    cmd.Connection = connection.Connection;
                    using (cmd)
                    {
                        try
                        {
                            cmd.ExecuteNonQuery();
                            transaction.Commit();
                            lastInsertionId = connection.LastInsertRowId;
                        }
                        catch (SQLiteException e)
                        {
                            transaction.Rollback();
                            //Log.Error($"SQLite error occurred at ExecuteNonQueryCmd(): {e.Message}");
                            // Log handle
                            throw;
                        }
                        catch (InvalidOperationException e)
                        {
                            transaction.Rollback();
                            //Log.Error($"Invalid operation at ExecuteNonQueryCmd(): {e.Message}");
                            throw;
                        }
                        catch (Exception e)
                        {
                            //Log.Error($"Unexpected error at ExecuteNonQueryCmd(): {e.Message}");
                            throw;
                        }
                    }
                }
            }
            finally
            {
                connection.Commit();
                _dbConnectionManager.ReturnConnection(connection);
            }

            return lastInsertionId;
        }


        public DataTable ExecuteReader(SQLiteCommand cmd)
        {
            var dt = new DataTable();
            var connection = _dbConnectionManager.GetConnection();
            cmd.Connection = connection.Connection;
            try
            {
                using (cmd)
                {
                    using (var dr = cmd.ExecuteReader())
                    {
                        if (dr.HasRows)
                        {
                            dt = new DataTable();
                            dt.Load(dr);
                        }
                    }
                }
            }
            finally
            {
                _dbConnectionManager.ReturnConnection(connection);
            }
            return dt;
        }
    }
}
