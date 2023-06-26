using System;
using System.Data;
using System.Data.SQLite;
using DataLayer;
using Models;

namespace DataAccessLayer
{
    public class PostDal
    {
        private readonly SQLiteDb _dal = new SQLiteDb();

        public long Add(Post post, int userId)
        {
            var command = new SQLiteCommand
            {
                CommandText = "INSERT INTO post(text, user_id) VALUES(@text, @user_id)",
                CommandType = CommandType.Text
            };

            command.Parameters.AddWithValue("@text", post.Text);
            command.Parameters.AddWithValue("@user_id", userId);
            return _dal.ExecuteNonQueryCmd(command);
        }

        public Post GetOne()
        {
            var cmd = new SQLiteCommand { CommandText = "SELECT * FROM post LIMIT 1;" };

            var dt = _dal.ExecuteReader(cmd);
            if (dt.Rows.Count <= 0)
                return null;

            return PostDataRowToEntity(dt.Rows[0]);
        }

        private Post PostDataRowToEntity(DataRow row)
        {
            try
            {
                return new Post
                {
                    Id = int.Parse(row["id"].ToString()),
                    Text = row["text"]?.ToString()
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
