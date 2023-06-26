namespace DataLayer
{
    internal class Env
    {
        public static readonly string SqliteDatabaseFileName = "sqlite.db";
        public static readonly string SqliteDatabaseConnectionString = "Data Source=" + SqliteDatabaseFileName + ";Version=3; ";
    }
}
