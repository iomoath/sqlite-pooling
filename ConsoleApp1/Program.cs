using System;
using DataAccessLayer;

namespace ConsoleApp1
{
    internal class Program
    {
        private static void Main()
        {
            // Init
            var s = new CommonBal();
            s.InitPrimaryDatabase();
            s.PrimaryDbVacuum();


            // Run tests
            //var t = new ThreadSafetyTest();
            var t = new ThreadSafetyTest2();
            t.RunConcurrentReadWriteTest();
            Console.ReadLine();
        }
    }
}
