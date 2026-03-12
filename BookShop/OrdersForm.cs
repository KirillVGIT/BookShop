using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace BookShop
{
    public partial class OrdersForm : Form
    {
        string connStr = "server=127.0.0.1;user=root;password=root;database=dbbook50;";
        DataTable ordersTable;
        public OrdersForm()
        {
            InitializeComponent();

            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Text = "Управление заказами";

        }

        private void OrdersForm_Load(object sender, EventArgs e)
        {
            LoadOrders();
            LoadStatuses();
            SetupDataGridView();
        }

        private void LoadOrders()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    string sql = @"SELECT 
                                    o.order_id AS ID,
                                    CONCAT(o.customer_first_name, ' ', o.customer_last_name) AS Клиент,
                                    o.customer_email AS Email,
                                    o.customer_phone AS Телефон,
                                    DATE_FORMAT(o.order_date, '%d.%m.%Y %H:%i') AS Дата,
                                    CONCAT(o.total_amount, ' руб.') AS Сумма,
                                    o.status AS Статус
                                  FROM `order` o
                                  ORDER BY o.order_date DESC";

                    MySqlDataAdapter da = new MySqlDataAdapter(sql, conn);
                    ordersTable = new DataTable();
                    da.Fill(ordersTable);
                    dataGridView1.DataSource = ordersTable;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке заказов: " + ex.Message,
                              "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadStatuses()
        {
            comboStatus.Items.Clear();
            comboStatus.Items.Add("Все статусы");
            comboStatus.Items.Add("В обработке");
            comboStatus.Items.Add("Оплачен");
            comboStatus.Items.Add("Доставлен");
            comboStatus.SelectedIndex = 0;
        }

        private void SetupDataGridView()
        {
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.ReadOnly = true;
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.RowHeadersVisible = false;

            if (dataGridView1.Columns.Contains("ID"))
                dataGridView1.Columns["ID"].Visible = false;
        }

        private void comboStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyFilters();
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            ApplyFilters();
        }
        private void ApplyFilters()
        {
            if (ordersTable == null) return;

            string filter = "";

            if (!string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                filter = $"Клиент LIKE '%{txtSearch.Text}%' OR " +
                         $"Email LIKE '%{txtSearch.Text}%' OR " +
                         $"Телефон LIKE '%{txtSearch.Text}%'";
            }

            if (comboStatus.SelectedIndex > 0)
            {
                if (filter != "") filter += " AND ";
                filter += $"Статус = '{comboStatus.Text}'";
            }

            (dataGridView1.DataSource as DataTable).DefaultView.RowFilter = filter;
        }

        private void btnChangeStatus_Click(object sender, EventArgs e)
        {

            if (dataGridView1.SelectedRows.Count > 0)
            {
                // Получаем данные выбранного заказа
                int orderId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["ID"].Value);
                string customerName = dataGridView1.SelectedRows[0].Cells["Клиент"].Value.ToString();
                string currentStatus = dataGridView1.SelectedRows[0].Cells["Статус"].Value.ToString();

                // Открываем форму изменения статуса
                ChangeOrderStatusForm statusForm = new ChangeOrderStatusForm(orderId, customerName, currentStatus);

                if (statusForm.ShowDialog() == DialogResult.OK)
                {
                    // Обновляем список заказов
                    LoadOrders();
                }
            }
            else
            {
                MessageBox.Show("Выберите заказ для изменения статуса!", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // Метод обновления статуса заказа в БД
        private bool UpdateOrderStatus(int orderId, string newStatus)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    string query = "UPDATE `order` SET status = @status WHERE order_id = @id";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@status", newStatus);
                        cmd.Parameters.AddWithValue("@id", orderId);

                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show($"Статус заказа успешно изменен на '{newStatus}'!", "Успех",
                                          MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return true;
                        }
                        else
                        {
                            MessageBox.Show("Не удалось изменить статус заказа!", "Ошибка",
                                          MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при изменении статуса: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void btnViewDetails_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int orderId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["ID"].Value);
                string customerName = dataGridView1.SelectedRows[0].Cells["Клиент"].Value.ToString();
                string orderDate = dataGridView1.SelectedRows[0].Cells["Дата"].Value.ToString();
                string totalAmount = dataGridView1.SelectedRows[0].Cells["Сумма"].Value.ToString();
                string status = dataGridView1.SelectedRows[0].Cells["Статус"].Value.ToString();

                try
                {
                    using (MySqlConnection conn = new MySqlConnection(connStr))
                    {
                        conn.Open();

                        // Получаем информацию о клиенте
                        string customerInfoQuery = @"SELECT 
                                                    customer_email, 
                                                    customer_phone 
                                                   FROM `order` 
                                                   WHERE order_id = @orderId";

                        string customerEmail = "";
                        string customerPhone = "";

                        using (MySqlCommand cmd = new MySqlCommand(customerInfoQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@orderId", orderId);

                            using (MySqlDataReader reader = cmd.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    customerEmail = reader["customer_email"].ToString();
                                    customerPhone = reader["customer_phone"].ToString();
                                }
                            }
                        }

                        // Получаем состав заказа
                        string orderDetailsQuery = @"SELECT 
                                                    p.title AS Книга,
                                                    op.quantity AS Количество,
                                                    op.unit_price AS 'Цена за шт.',
                                                    (op.quantity * op.unit_price) AS Сумма
                                                   FROM orderproduct op
                                                   JOIN product p ON op.product_id = p.product_id
                                                   WHERE op.order_id = @orderId";

                        StringBuilder details = new StringBuilder();
                        details.AppendLine($"ЗАКАЗ №{orderId}\n");
                        details.AppendLine($"Клиент: {customerName}");
                        details.AppendLine($"Email: {customerEmail}");
                        details.AppendLine($"Телефон: {customerPhone}");
                        details.AppendLine($"Дата: {orderDate}");
                        details.AppendLine($"Статус: {status}");
                        details.AppendLine($"\nСОСТАВ ЗАКАЗА\n");

                        decimal orderTotal = 0;

                        using (MySqlCommand cmd = new MySqlCommand(orderDetailsQuery, conn))
                        {
                            cmd.Parameters.AddWithValue("@orderId", orderId);

                            using (MySqlDataReader reader = cmd.ExecuteReader())
                            {
                                bool hasItems = false;

                                while (reader.Read())
                                {
                                    hasItems = true;
                                    string book = reader["Книга"].ToString();
                                    int quantity = Convert.ToInt32(reader["Количество"]);
                                    decimal price = Convert.ToDecimal(reader["Цена за шт."]);
                                    decimal sum = Convert.ToDecimal(reader["Сумма"]);

                                    details.AppendLine($"{book}");
                                    details.AppendLine($" Кол-во: {quantity} × {price:C} = {sum:C}");
                                    details.AppendLine();

                                    orderTotal += sum;
                                }

                                if (!hasItems)
                                {
                                    details.AppendLine("(Заказ пуст)");
                                }
                            }
                        }

                        details.AppendLine($"\n=== ИТОГО ===\n");
                        details.AppendLine($"Общая сумма: {orderTotal:C}");

                        // Показываем информацию
                        MessageBox.Show(details.ToString(), "Детали заказа",
                                      MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при загрузке деталей заказа: {ex.Message}", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Выберите заказ для просмотра деталей!", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}

