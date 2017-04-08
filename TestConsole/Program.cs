using DBHelper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            TransactionTest();
            Console.ReadLine();
        }

        static int AddProduct(string name, int amount)
        {
            return Db.ExecuteNonQuery("insert Product values(@Name,@Amount)", name, amount);
        }

        static int ProductCount()
        {
            return (int)Db.ExecuteScalar("select count(*) from Product");
        }

        static int ProductAmount(int id)
        {
            return (int)Db.ExecuteScalar("select Amount from Product where Id = @Id", id);
        }

        static List<string> ProductNames()
        {
            List<string> list = new List<string>();
            var reader = Db.ExecuteReader("select Name from Poduct Order By Name");
            while (reader.Read())
                list.Add(reader.GetString(0));
            reader.Close();
            return list;
        }

        static DataTable ProductDataTable()
        {
            DataTable dt = new DataTable("Product");
            Db.FillTable(dt, "select * from Product");
            return dt;
        }

        static void TransactionTest()
        {
            Db.OpenConnection();
            var bb = Db.BeginTransaction();
            try
            {
                Db.ExecuteNonQuery("delete Product where Id = @Id", 4);
                Db.ExecuteNonQuery("update Product sed Amount = 10000 where Id = @Id", 5);
                Db.CommitTransaction(bb);
            }
            catch (Exception ex)
            {
                Db.RollbackTransaction();
                Console.WriteLine(ex.Message);
            }

        }
    }
}
