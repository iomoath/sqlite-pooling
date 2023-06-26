using System;
using System.Data;
using System.Data.SQLite;
using DataLayer;
using Models;

namespace DataAccessLayer
{
    public class UserDal
    {
        private readonly SQLiteDb _dal = new SQLiteDb();


        public long Add(User user)
        {
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
            return _dal.ExecuteNonQueryCmd(command);
        }

        public User GetById(int id)
        {
            var cmd = new SQLiteCommand { CommandText = "SELECT * FROM user where id = @id" };
            cmd.Parameters.AddWithValue("@id", id);

            var dt = _dal.ExecuteReader(cmd);
            if (dt.Rows.Count <= 0)
                return null;

            return UserDataRowToEntity(dt.Rows[0]);
        }

        public User GetByUsername(string username)
        {
            var cmd = new SQLiteCommand { CommandText = "SELECT * FROM user where username = @username" };
            cmd.Parameters.AddWithValue("@username", username);

            var dt = _dal.ExecuteReader(cmd);
            if (dt.Rows.Count <= 0)
                return null;

            return UserDataRowToEntity(dt.Rows[0]);
        }

        private User UserDataRowToEntity(DataRow row)
        {
            try
            {
                return new User
                {
                    Id = int.Parse(row["id"].ToString()),
                    Username = row["username"]?.ToString(),
                    Email = row["email"]?.ToString(),
                    MobileNumber = row["mobile_number"]?.ToString(),
                    CountryCode = row["country_code"]?.ToString(),
                };
            }
            catch (Exception)
            {
                // Log, handle
                return null;
            }
        }


    }
}
