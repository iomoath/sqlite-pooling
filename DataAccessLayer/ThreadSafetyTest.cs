using System;
using System.Threading.Tasks;
using Models;

namespace DataAccessLayer
{
    public class ThreadSafetyTest
    {
        private void PerformReadOperation()
        {
            var userDal = new UserDal();
            var postDal = new PostDal();

            var p = postDal.GetOne();
            var u = userDal.GetByUsername("USER_admin");

            // Verify the result
            if (u?.CountryCode != "JO" || p?.Text == null)
            {
                throw new Exception("Unexpected result");
            }
        }
        private void PerformWriteOperation()
        {
            var userDal = new UserDal();

            var guid = Guid.NewGuid().ToString();

            var user = new User
            {
                Username = $"USER_{guid.Split('-')[0]}",
                Password = "123456",
                Email = "test@localhost",
                MobileNumber = "07900000",
                CountryCode = "JO"
            };
            
            var insertionId = userDal.Add(user);

            if (insertionId <= 0)
            {
                throw new Exception($"data insertion failed. Insertion Id: {insertionId}");
            }
        }

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

            var p = new Post { Text = "Hello"};
            postDal.Add(p, uId);
        }
    }
}
