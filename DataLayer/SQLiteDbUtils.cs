using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;

namespace DataLayer
{
    internal class SQLiteDbUtils
    {
        private readonly List<Action> _tablesCreators = new List<Action>();
        private readonly IConnectionManager _dbConnectionManager;

        public SQLiteDbUtils()
        {
            _dbConnectionManager = SQLiteConnectionManager.Instance;
            Init();
        }

        public bool CreateDbFile()
        {
            try
            {
                if (File.Exists(Env.SqliteDatabaseFileName))
                    return true;

                SQLiteConnection.CreateFile(Env.SqliteDatabaseFileName);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public bool CreateTables()
        {
            try
            {
                SetDbConfigPragma();
                foreach (Action func in _tablesCreators)
                {
                    func();
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private void SetDbConfigPragma()
        {
            var connection = _dbConnectionManager.GetConnection();

            using (var command = new SQLiteCommand(connection.Connection))
            {
                var cmd = "PRAGMA journal_mode=WAL;";
                command.CommandText = cmd;
                command.ExecuteNonQuery();
            }

            connection.Commit();
            _dbConnectionManager.ReturnConnection(connection);
        }

        private void Init()
        {
            _tablesCreators.Add(CreateUserTable);
            _tablesCreators.Add(CreatePostTable);
        }

        private void CreateUserTable()
        {
            var connection = _dbConnectionManager.GetConnection();

            try
            {
                using (var transaction = connection.BeginTransaction())
                {
                    using (var command = new SQLiteCommand(connection.Connection))
                    {
                        var cmd = "CREATE TABLE IF NOT EXISTS `user` (" +
                                  "`id` INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL," +
                                  "`username` TEXT(16) NOT NULL, " +
                                  "`password` TEXT(128) NOT NULL, " +
                                  "`email` TEXT NOT NULL, " +
                                  "`mobile_number` TEXT(16), " +
                                  "`country_code` TEXT(2));";

                        command.CommandText = cmd;
                        command.ExecuteNonQuery();

                        transaction.Commit();
                    }
                }
            }
            finally
            {
                connection.Commit();
                _dbConnectionManager.ReturnConnection(connection);
            }
        }

        private void CreatePostTable()
        {
            var connection = _dbConnectionManager.GetConnection();

            try
            {
                using (var transaction = connection.BeginTransaction())
                {
                    using (var command = new SQLiteCommand(connection.Connection))
                    {
                        var cmd = "CREATE TABLE IF NOT EXISTS `post` (" +
                                  "`id` INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, " +
                                  "`text` TEXT, " +
                                  "`user_id` INTEGER NOT NULL, " +
                                  "FOREIGN KEY(`user_id`) REFERENCES user(`id`) ON DELETE CASCADE);";
                        command.CommandText = cmd;
                        command.ExecuteNonQuery();

                        transaction.Commit();
                    }
                }
            }
            finally
            {
                connection.Commit();
                _dbConnectionManager.ReturnConnection(connection);
            }
        }
    }
}
