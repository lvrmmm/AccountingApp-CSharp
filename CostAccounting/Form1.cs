using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TaskbarClock;

namespace CostAccounting
{
    /// <summary>
    /// Основная форма взаимодействия пользователя с приложением "Учёт расходов"
    /// </summary>
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

        }
        /// <summary>
        /// Создание колонок в DataGridView с нужными заголовками
        /// </summary>
        public void GetColumns()
        {
            dataGridView1.Columns.Add("id", "Номер");
            dataGridView1.Columns.Add("cost_name", "Наименование операции");
            dataGridView1.Columns.Add("category_name", "Категория операции");
            dataGridView1.Columns.Add("amount", "Сумма операции");
            dataGridView1.Columns.Add("date", "Дата операции");
            dataGridView1.Columns[0].Visible = false;
        }
        /// <summary>
        /// Обновление информации в DataGridView, на графике и общего количества расходов
        /// </summary>
        /// <param name="dataGridView"></param>
        private void RefreshData(DataGridView dataGridView)
        {
            List<Costs> costslist = Costs.GetFromDB();
            dataGridView.Rows.Clear();
            foreach (Costs cost in costslist)
            {
                dataGridView.Rows.Add(cost.ID, cost.CostName, cost.CategoryName, cost.Amount, cost.Date);
            }

            List<string> uniccategory = costslist.Select(r => r.CategoryName).Distinct().ToList(); 

            chart1.Series[0].Points.Clear(); 
            foreach (string ctg in uniccategory)
            {
                decimal total = costslist.Where(q => q.CategoryName == ctg).Select(q => q.Amount).Sum(); 
                if (total != 0)
                {
                    chart1.Series[0].Points.AddXY(ctg, total); 
                }
            }

            decimal costsbalance = costslist.Where(q => q.Date.Month == DateTime.Now.Month).Select(r => r.Amount).Sum(); 
            label8.Text = "Текущая сумма расходов за месяц: "+ "\n" + costsbalance.ToString("C", CultureInfo.CreateSpecificCulture("ru-RU")); // Отображаем число в формате рублей
        }
        /// <summary>
        /// Метод, добавляющий в базу данных новую совершённую денежную операцию по нажатии кнопки "Добавить"
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1Add_Click(object sender, EventArgs e)
        {

            var category = comboBox1.Text;
            var name = textBoxname.Text;
            var amount = textBoxAmount.Text;
            var date = textBoxDate.Text;

            if (category == "" || name == "" ||  amount == "" ||  date == "" )
            {
                MessageBox.Show("Введиите данные");
                return;
            }
            DateTime Date;
            if (!DateTime.TryParse(date, out Date))
            {
                MessageBox.Show("Введите корректное значение для даты операции!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            decimal Amount;
            if (!decimal.TryParse(amount, out Amount))
            {

                MessageBox.Show("Введите корректное значение для суммы операции!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string connectionString = "Data Source=CostAccounting.db;";

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand(connection))
                {
                    string insertingquery = $"insert into Costs(cost_name,category_name, amount, date) values('{name}','{category}','{Amount}', '{Date}')";
                    command.CommandText = insertingquery;
                    command.ExecuteNonQuery();
                }

                connection.Close();
            }

            MessageBox.Show("Операция добавлена!", "Успешно", MessageBoxButtons.OK, MessageBoxIcon.Information);
            RefreshData(dataGridView1);

        }
        /// <summary>
        /// Удаление выбранных в DataGridView операций из базы данных 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2Delete_Click(object sender, EventArgs e)
        {
            
            if (dataGridView1.SelectedRows.Count == 0)
            {
                MessageBox.Show("Пожалуйста, выберите задачу для удаления! \n" +
                                "Выберите для этого крайнюю левую ячейку в строке нужной задачи!", "Укажите строку!", MessageBoxButtons.OK, MessageBoxIcon.Question);
                return;
            }

           
            string connectionString = "Data Source=CostAccounting.db;";

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand(connection))
                {

                    for (int i = 0; i < dataGridView1.SelectedRows.Count; i++)
                    {
                        int selectedCostId = Convert.ToInt32(dataGridView1.SelectedRows[i].Cells["id"].Value);
                        string deletingstring = $"DELETE FROM Costs WHERE id = '{selectedCostId}'";
                        command.CommandText = deletingstring;
                        command.ExecuteNonQuery();
                    }
                    
                }

                connection.Close();
            }

            dataGridView1.Rows.Remove(dataGridView1.SelectedRows[0]);
            RefreshData(dataGridView1);
        }
        /// <summary>
        /// Очистка всех полей ввода
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)
        {
            textBoxname.Text = "";
            textBoxAmount.Text = "";
            textBoxDate.Text = "";
            
        }
        /// <summary>
        /// Отображание операци1 по выбранному параметру, который задан в ComboBox
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2Filter_Click(object sender, EventArgs e)
        {
            List<Costs> costslist = Costs.GetFromDB();
            dataGridView1.Rows.Clear();
            switch (comboBox2.SelectedIndex)
            {
                case 0:
                    foreach (Costs cost in  costslist) { dataGridView1.Rows.Add(cost.ID, cost.CostName, cost.CategoryName, cost.Amount, cost.Date); }
                    break;
                case 1:
                    foreach (Costs cost in (from p in costslist where p.CategoryName == "Женское" select p).ToList()) { dataGridView1.Rows.Add(cost.ID, cost.CostName, cost.CategoryName, cost.Amount, cost.Date); }
                    break;
                case 2:
                    foreach (Costs cost in (from q in costslist where q.CategoryName == "Здоровье и красота" select q).ToList()) { dataGridView1.Rows.Add(cost.ID, cost.CostName, cost.CategoryName, cost.Amount, cost.Date); }
                    break;
                case 3:
                    foreach (Costs cost in (from q in costslist where q.CategoryName == "Мужское" select q).ToList()) { dataGridView1.Rows.Add(cost.ID, cost.CostName, cost.CategoryName, cost.Amount, cost.Date); }
                    break;
                case 4:
                    foreach (Costs cost in (from q in costslist where q.CategoryName == "Одежда и аксессуары" select q).ToList()) { dataGridView1.Rows.Add(cost.ID, cost.CostName, cost.CategoryName, cost.Amount, cost.Date); }
                    break;
                case 5:
                    foreach (Costs cost in (from q in costslist where q.CategoryName == "Переводы людям" select q).ToList()) { dataGridView1.Rows.Add(cost.ID, cost.CostName, cost.CategoryName, cost.Amount, cost.Date); }
                    break;
                case 6:
                    foreach (Costs cost in (from q in costslist where q.CategoryName == "Развлечения" select q).ToList()) { dataGridView1.Rows.Add(cost.ID, cost.CostName, cost.CategoryName, cost.Amount, cost.Date); }
                    break;
                case 7:
                    foreach (Costs cost in (from q in costslist where q.CategoryName == "Спорт и фитнес" select q).ToList()) { dataGridView1.Rows.Add(cost.ID, cost.CostName, cost.CategoryName, cost.Amount, cost.Date); }
                    break;
                case 8:
                    foreach (Costs cost in (from q in costslist where q.CategoryName == "Супермаркеты" select q).ToList()) { dataGridView1.Rows.Add(cost.ID, cost.CostName, cost.CategoryName, cost.Amount, cost.Date); }
                    break;
                case 9:
                    foreach (Costs cost in (from q in costslist where q.CategoryName == "Путешествия" select q).ToList()) { dataGridView1.Rows.Add(cost.ID, cost.CostName, cost.CategoryName, cost.Amount, cost.Date); }
                    break;
                case 10:
                    foreach (Costs cost in (from q in costslist where q.CategoryName == "Остальное" select q).ToList()) { dataGridView1.Rows.Add(cost.ID, cost.CostName, cost.CategoryName, cost.Amount, cost.Date); }
                    break;
                case 11:
                    foreach (Costs cost in (from q in costslist where q.CategoryName == "Связь" select q).ToList()) { dataGridView1.Rows.Add(cost.ID, cost.CostName, cost.CategoryName, cost.Amount, cost.Date); }
                    break;
                case 12:
                    foreach (Costs cost in (from p in costslist where p.Date.Month == DateTime.Now.Month select p).ToList()) { dataGridView1.Rows.Add(cost.ID, cost.CostName, cost.CategoryName, cost.Amount, cost.Date); }
                    break;
                case 13:
                    foreach (Costs cost in (from p in costslist where p.Date.Year == DateTime.Now.Year select p).ToList()) { dataGridView1.Rows.Add(cost.ID, cost.CostName, cost.CategoryName, cost.Amount, cost.Date); }
                    break;


            }

        }
        /// <summary>
        /// Меняет информацию в DataGridView и базе данных на указанную в полях управления операциями
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            
            string cost_name = textBoxname.Text;

            DateTime date;
            if (!DateTime.TryParse(textBoxDate.Text, out date))
            {
                MessageBox.Show("Введите корректное значение для даты!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            decimal amount;
            if (!decimal.TryParse(textBoxAmount.Text, out amount))
            {

                MessageBox.Show("Введите корректное значение для суммы!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int selectedID = int.Parse(textBoxid.Text);

            foreach (DataGridViewRow row in dataGridView1.SelectedRows)
            {
 
                row.Cells[1].Value = cost_name;
                row.Cells[2].Value = comboBox1.Text;
                row.Cells[3].Value = amount;
                row.Cells[4].Value = date;
            }


            string connectionString = "Data Source=CostAccounting.db;";

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();

                using (SQLiteCommand command = new SQLiteCommand(connection))
                {

                    string updatingquery = $"UPDATE Costs SET cost_name = '{cost_name}', category_name = '{comboBox1.Text}', amount = '{amount}', date = '{date}' WHERE id = '{selectedID}'";
                    command.CommandText = updatingquery;
                    command.ExecuteNonQuery();
                }

                connection.Close();
            }

            MessageBox.Show("Данные успешно обновлены", "Успешно", MessageBoxButtons.OK, MessageBoxIcon.Information);
            RefreshData(dataGridView1);

        }
        /// <summary>
        /// Вносит информацию из выбранной строки DataGridView в поля для управления операциями
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            int selectedRow = e.RowIndex;

            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[selectedRow];
                textBoxid.Text = row.Cells[0].Value.ToString();
                textBoxname.Text = row.Cells[1].Value.ToString();
                textBoxAmount.Text = row.Cells[3].Value.ToString();
                textBoxDate.Text = row.Cells[4].Value.ToString();
                comboBox1.Text = row.Cells[2].Value.ToString();



            }
        }
        /// <summary>
        /// Загрузка основной формы
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {
            GetColumns();
            RefreshData(dataGridView1);
        }
    }
}
