using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lotech.Data.Example
{
    public class TransactionManagerTest
    {
        IDatabase db = DatabaseFactory.CreateDatabase("mysql");
        public void BasicTest()
        {
            using (TransactionManager tm = new TransactionManager())
            {
                db.ExecuteNonQuery("SELECT name FROM mysql.user");
            }
        }
        public void NestedSuccessTest()
        {
            using (TransactionManager tm = new TransactionManager())
            {
                db.ExecuteNonQuery("SELECT name FROM mysql.user");

                using (TransactionManager tm2 = new TransactionManager())
                {
                    db.ExecuteNonQuery("SELECT name FROM mysql.user");
                    tm2.Commit();
                }
                // exception for : rollbacked
                db.ExecuteNonQuery("SELECT now()");
            }
        }

        public void NestedTest()
        {
            using (TransactionManager tm = new TransactionManager())
            {
                db.ExecuteNonQuery("SELECT name FROM mysql.user");

                using (TransactionManager tm2 = new TransactionManager())
                {
                    db.ExecuteNonQuery("SELECT name FROM mysql.user");
                }
                // exception for : rollbacked
                db.ExecuteNonQuery("SELECT now()");
            }
        }


        public void ComplexNestedTest()
        {
            using (var tm = new TransactionManager())
            {
                var cid = db.ExecuteScalar("SELECT CONNECTION_ID(),1 -- OPEN");
                cid = db.ExecuteScalar("SELECT CONNECTION_ID(),2 ");

                using (var suppress = new TransactionManager(suppress: true))
                {
                    cid = db.ExecuteScalar("SELECT CONNECTION_ID(),3 --  OPEN");
                    cid = db.ExecuteScalar("SELECT CONNECTION_ID(),4 --  OPEN");

                    using (var tm3 = new TransactionManager())
                    {
                        cid = db.ExecuteScalar("SELECT CONNECTION_ID(),5 --  OPEN");
                        cid = db.ExecuteScalar("SELECT CONNECTION_ID(),6");
                    }
                }

                using (var requiresNew = new TransactionManager(true))
                {
                    cid = db.ExecuteScalar("SELECT CONNECTION_ID(),7 --  OPEN");
                    cid = db.ExecuteScalar("SELECT CONNECTION_ID(),8");
                }

                cid = db.ExecuteScalar("SELECT CONNECTION_ID(),9");

                using (var required = new TransactionManager())
                {
                    cid = db.ExecuteScalar("SELECT CONNECTION_ID(), 10");
                    cid = db.ExecuteScalar("SELECT CONNECTION_ID(), 11");
                    required.Commit();
                }

                using (var required = new TransactionManager())
                {
                    cid = db.ExecuteScalar("SELECT CONNECTION_ID(), 12");
                    cid = db.ExecuteScalar("SELECT CONNECTION_ID(), 13");
                }

                // exception for : rollbacked
                cid = db.ExecuteScalar("SELECT CONNECTION_ID(), 14 --  ERROR CLOSED");
                cid = db.ExecuteScalar("SELECT CONNECTION_ID(), 15 -- SHOULD NOT BE EXECUTED");
            }
        }
    }
}
