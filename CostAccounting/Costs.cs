using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CostAccounting
{

    /// <summary>
    /// Класс, определяющий каждую из трат в списке
    /// </summary>
    public class Costs
    {
        private int id;
        private string cost_name; 
        private string category_name;
        private decimal amount;
        private DateTime date;

        public Costs(int id, string cost_name, string category_name, decimal amount, DateTime date)
        {
            this.id = id;
            this.cost_name = cost_name;
            this.category_name = category_name;
            this.amount = amount;
            this.date = date;
        }

        public int ID { get => id; set => id = value; }
        public string CostName { get => cost_name; set => cost_name = value; }
        public string CategoryName { get => category_name; set => category_name = value; }
        public decimal Amount { get => amount; set => amount = value; }
        public DateTime Date { get => date; set => date = value; }

        /// <summary>
        /// Статический метод, который определяет  List<Costs> costlist, содержащий в себе все траты из базы данных
        /// </summary>
        /// <returns></returns>
        public static List<Costs> GetFromDB()
        {
            List<Costs> costlist = new List<Costs>();

            string connectionString = "Data Source=CostAccounting.db;";
            string query = "SELECT * FROM Costs";

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {

                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int id = reader.GetInt32(0);
                            string cost_name = reader.GetString(1);
                            string category_name = reader.GetString(2);
                            decimal amount = reader.GetDecimal(3);
                            string datestring = reader.GetString(4);
                            DateTime date = DateTime.Parse(datestring);


                            Costs newcost = new Costs(id, cost_name, category_name, amount, date);
                            costlist.Add(newcost);
                        }
                    }
                }
            }

            return costlist;
        }
    }
}
