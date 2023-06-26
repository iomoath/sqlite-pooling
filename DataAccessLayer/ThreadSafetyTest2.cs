using System;
using System.Data;
using System.Data.SQLite;
using System.Threading.Tasks;
using DataLayer;
using Models;

namespace DataAccessLayer
{
    public class ThreadSafetyTest2
    {
        private readonly IConnectionManager _dbConnectionManager = SQLiteConnectionManager.Instance;

        public void RunConcurrentReadWriteTest()
        {
            Console.WriteLine("@Executing  Concurrent Read Write Test");
            Init();

            const int taskCount = 1000000;

            Task[] tasks = new Task[taskCount];

            // Half the tasks will be reading, half will be writing
            for (int i = 0; i < taskCount; i++)
            {
                tasks[i] = i < taskCount / 2 ? Task.Run(PerformReadOperation) : Task.Run(PerformWriteOperation);
            }

            Task.WaitAll(tasks);
            Console.WriteLine("@Executed  Concurrent Read Write Test");
        }


        private void PerformReadOperation()
        {
            var command = new SQLiteCommand
            {
                CommandText = "SELECT country_code from user",
                CommandType = CommandType.Text
            };

            var result = (string)ExecuteScalar(command);

            // Verify the result
            if (result != "JO")
            {
                throw new Exception($"Unexpected result: {result}");
            }
        }

        private void PerformWriteOperation()
        {
            var user = new User
            {
                Username = $"USER_{Guid.NewGuid().ToString().Split('-')[0]}",
                Password = "123456",
                Email = "test@localhost",
                MobileNumber = "07900000",
                CountryCode = "JO"
            };

            var command = new SQLiteCommand
            {
                CommandText = "INSERT INTO user(username, password, email, mobile_number, country_code) VALUES(@username, @password, @email, @mobile_number, @country_code)",
                CommandType = CommandType.Text
            };

            command.Parameters.AddWithValue("@username", user.Username);
            command.Parameters.AddWithValue("@password", user.Password);
            command.Parameters.AddWithValue("@email", user.Email);
            command.Parameters.AddWithValue("@mobile_number", user.MobileNumber);
            command.Parameters.AddWithValue("@country_code", user.CountryCode);

            var insertionId = ExecuteNonQueryCmd(command);

            if (insertionId <= 0)
            {
                throw new Exception($"data insertion failed. Insertion Id: {insertionId}");
            }
        }


        public object ExecuteScalar(SQLiteCommand cmd)
        {
            var connection = _dbConnectionManager.GetConnection();
            cmd.Connection = connection.Connection;
            try
            {
                using (cmd)
                {
                    return cmd.ExecuteScalar();
                }
            }
            finally
            {
                _dbConnectionManager.ReturnConnection(connection);
            }
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
                            throw;
                        }
                        catch (InvalidOperationException e)
                        {
                            transaction.Rollback();
                            throw;
                        }
                        catch (Exception e)
                        {
                            transaction.Rollback();
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


        private void Init()
        {
            var userDal = new UserDal();
            var postDal = new PostDal();

            var u = new User
            {
                Username = "USER_admin",
                Password = "123456",
                Email = "test@localhost",
                MobileNumber = "07900000",
                CountryCode = "JO"
            };

            var uId = (int)userDal.Add(u);

            var p = new Post { Text = "Hello" };
            postDal.Add(p, uId);
        }

    }
}
