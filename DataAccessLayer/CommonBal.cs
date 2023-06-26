using System;
using DataLayer;

namespace DataAccessLayer
{
    public class CommonBal
    {
        private readonly SQLiteDb _dal = new SQLiteDb();

        public bool InitPrimaryDatabase()
        {
            if (!_dal.InitDb())
                throw new Exception("DB initialization failed");

            return true;
        }

        public void PrimaryDbVacuum()
        {
            _dal.Vacuum();
        }
    }
}
