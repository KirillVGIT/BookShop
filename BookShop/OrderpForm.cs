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
using BookShop;



namespace BookShop
{
    public partial class OrderpForm : Form
    {

        private int currentSellerId;

        public OrderpForm(int userId)
        {
            InitializeComponent();
            currentSellerId = userId;
        }

        private void OrderpForm_Load(object sender, EventArgs e)
        {
            this.Text = "Все заказы";

            // Загружаем статусы для фильтрации
            cmbStatus.Items.Add("Все статусы");
            cmbStatus.Items.Add("В обработке");
            cmbStatus.Items.Add("Оплачен");
            cmbStatus.Items.Add("Доставлен");
            cmbStatus.Items.Add("Отменен");
            cmbStatus.SelectedIndex = 0;

            // Загружаем продавцов для фильтрации
            LoadSellersFilter();

            // Загружаем все заказы
            LoadAllOrders();
        }

        private void LoadSellersFilter()
        {
            cmbSeller.Items.Add("Все продавцы");

            string connStr = "server=127.0.0.1;user=root;password=root;database=dbbook50;";

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    string sql = @"SELECT u.user_id, u.username, 
                                 CONCAT(u.first_name, ' ', u.last_name) as full_name
                                 FROM user u
                                 JOIN role r ON u.role_id = r.role_id
                                 WHERE r.role_name = 'Продавец' OR r.role_name = 'Администратор'
                                 ORDER BY u.username";

                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                cmbSeller.Items.Add(new SellerItem
                                {
                                    Id = Convert.ToInt32(reader["user_id"]),
                                    Name = reader["full_name"].ToString(),
                                    Username = reader["username"].ToString()
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Если ошибка - просто пропускаем
            }

            cmbSeller.SelectedIndex = 0;
            cmbSeller.DisplayMember = "Name";
        }

        // Класс для хранения данных о продавце в ComboBox
        public class SellerItem
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string Username { get; set; }

            public override string ToString()
            {
                return Name;
            }
        }

        // Загрузка ВСЕХ заказов
        private void LoadAllOrders()
        {
            string connStr = "server=127.0.0.1;user=root;password=root;database=dbbook50;";

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();

                    // SQL запрос для ВСЕХ заказов
                    string sql = @"SELECT 
                                    o.order_id AS 'ID',
                                    o.order_id AS '№ заказа',
                                    DATE_FORMAT(o.order_date, '%d.%m.%Y %H:%i') AS 'Дата',
                                    CONCAT(o.customer_first_name, ' ', o.customer_last_name) AS 'Клиент',
                                    o.customer_phone AS 'Телефон',
                                    o.total_amount AS 'Сумма',
                                    o.status AS 'Статус',
                                    CONCAT(u.first_name, ' ', u.last_name) AS 'Продавец',
                                    u.username AS 'Логин продавца'
                                  FROM `order` o
                                  LEFT JOIN user u ON o.user_id = u.user_id
                                  ORDER BY o.order_date DESC";

                    MySqlDataAdapter adapter = new MySqlDataAdapter(sql, conn);
                    DataTable table = new DataTable();
                    adapter.Fill(table);

                    // Привязываем к DataGridView
                    dgvOrders.DataSource = table;

                    // Настройка DataGridView
                    SetupDataGridView();

                    // Обновляем статус
                    UpdateStatusLabel(table.Rows.Count);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заказов: {ex.Message}",
                              "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void SetupDataGridView()
        {
            dgvOrders.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvOrders.ReadOnly = true;
            dgvOrders.AllowUserToAddRows = false;
            dgvOrders.AllowUserToDeleteRows = false;
            dgvOrders.RowHeadersVisible = false;
            dgvOrders.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            // Скрываем ID и Логин продавца
            if (dgvOrders.Columns.Contains("ID"))
                dgvOrders.Columns["ID"].Visible = false;

            if (dgvOrders.Columns.Contains("Логин продавца"))
                dgvOrders.Columns["Логин продавца"].Visible = false;

            // Форматируем сумму
            if (dgvOrders.Columns.Contains("Сумма"))
            {
                dgvOrders.Columns["Сумма"].DefaultCellStyle.Format = "C";
                dgvOrders.Columns["Сумма"].DefaultCellStyle.Alignment =
                    DataGridViewContentAlignment.MiddleRight;
            }

            // Настраиваем ширину колонок
            if (dgvOrders.Columns.Contains("№ заказа"))
                dgvOrders.Columns["№ заказа"].Width = 70;

            if (dgvOrders.Columns.Contains("Дата"))
                dgvOrders.Columns["Дата"].Width = 120;

            if (dgvOrders.Columns.Contains("Клиент"))
                dgvOrders.Columns["Клиент"].Width = 150;

            if (dgvOrders.Columns.Contains("Телефон"))
                dgvOrders.Columns["Телефон"].Width = 100;

            if (dgvOrders.Columns.Contains("Сумма"))
                dgvOrders.Columns["Сумма"].Width = 90;

            if (dgvOrders.Columns.Contains("Статус"))
                dgvOrders.Columns["Статус"].Width = 100;

            if (dgvOrders.Columns.Contains("Продавец"))
                dgvOrders.Columns["Продавец"].Width = 120;

            // Подсветка строк по статусу
            dgvOrders.CellFormatting += DgvOrders_CellFormatting;
        }
        private void DgvOrders_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex >= 0 && dgvOrders.Columns.Contains("Статус") &&
                e.ColumnIndex == dgvOrders.Columns["Статус"].Index)
            {
                string status = dgvOrders.Rows[e.RowIndex].Cells["Статус"].Value?.ToString();
                Color backColor = Color.White;

                switch (status)
                {
                    case "В обработке":
                        backColor = Color.LightYellow;
                        break;
                    case "Оплачен":
                        backColor = Color.LightGreen;
                        break;
                    case "Доставлен":
                        backColor = Color.LightBlue;
                        break;
                    case "Отменен":
                        backColor = Color.LightCoral;
                        break;
                }

                dgvOrders.Rows[e.RowIndex].DefaultCellStyle.BackColor = backColor;
            }
        }

        // Обновление статусной строки
        private void UpdateStatusLabel(int count)
        {
            // Если есть Label для статуса
            if (Controls.Find("lblStatus", true).Length > 0)
            {
                Label lblStatus = (Label)Controls.Find("lblStatus", true)[0];
                lblStatus.Text = $"Найдено заказов: {count}";
            }
        }

        // Фильтрация заказов
        private void FilterOrders()
        {
            if (dgvOrders.DataSource == null) return;

            DataTable table = (DataTable)dgvOrders.DataSource;
            string filter = "";

            // 1. Фильтр по статусу
            if (cmbStatus.SelectedIndex > 0)
            {
                filter = $"[Статус] = '{cmbStatus.SelectedItem}'";
            }

            // 2. Фильтр по продавцу
            if (cmbSeller.SelectedIndex > 0)
            {
                SellerItem selectedSeller = (SellerItem)cmbSeller.SelectedItem;
                if (!string.IsNullOrEmpty(filter)) filter += " AND ";
                filter += $"[Логин продавца] = '{selectedSeller.Username}'";
            }

            // 3. Фильтр по поиску (ТОЛЬКО по текстовым полям, без номера заказа)
            if (!string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                string searchText = txtSearch.Text;
                if (!string.IsNullOrEmpty(filter)) filter += " AND ";

                // Ищем только в текстовых полях
                filter += $"([Клиент] LIKE '%{searchText}%' OR " +
                         $"[Телефон] LIKE '%{searchText}%')";
            }

            // Применяем фильтр
            try
            {
                table.DefaultView.RowFilter = filter;
                UpdateStatusLabel(table.DefaultView.Count);
            }
            catch
            {
                // Игнорируем ошибки фильтрации
            }
        }
        private void LoadOrdersSimple()
        {
            
        }
        
        
        

        

        

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            FilterOrders();
        }

        private void cmbStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            FilterOrders();
        }
        

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadAllOrders();
            txtSearch.Text = "";
            cmbStatus.SelectedIndex = 0;
            cmbSeller.SelectedIndex = 0;
        }

        private void btnViewDetails_Click(object sender, EventArgs e)
        {
            if (dgvOrders.SelectedRows.Count > 0)
            {
                DataGridViewRow selectedRow = dgvOrders.SelectedRows[0];

                string orderId = selectedRow.Cells["ID"].Value?.ToString() ?? "N/A";
                string orderNumber = selectedRow.Cells["№ заказа"].Value?.ToString() ?? "N/A";
                string customer = selectedRow.Cells["Клиент"].Value?.ToString() ?? "N/A";
                string date = selectedRow.Cells["Дата"].Value?.ToString() ?? "N/A";
                string total = selectedRow.Cells["Сумма"].Value?.ToString() ?? "N/A";
                string status = selectedRow.Cells["Статус"].Value?.ToString() ?? "N/A";
                string seller = selectedRow.Cells["Продавец"].Value?.ToString() ?? "N/A";

                string details = $"ДЕТАЛИ ЗАКАЗА\n" +
                               $"──────────────\n" +
                               $"№ заказа: {orderNumber}\n" +
                               $"Дата: {date}\n" +
                               $"Клиент: {customer}\n" +
                               $"Продавец: {seller}\n" +
                               $"Статус: {status}\n" +
                               $"Сумма: {total}\n\n" +
                               $"";

                MessageBox.Show(details, $"Заказ #{orderNumber}",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("Выберите заказ для просмотра деталей",
                              "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    

        private void btnPrintReceipt_Click(object sender, EventArgs e)
        {
            if (dgvOrders.SelectedRows.Count > 0)
            {
                int orderId = Convert.ToInt32(dgvOrders.SelectedRows[0].Cells["ID"].Value);
                ReceiptHelper receipt = new ReceiptHelper();
                receipt.CreateReceipt(orderId);
            }
            else
            {
                MessageBox.Show("Выберите заказ для печати чека!",
                              "Внимание", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnNewOrder_Click(object sender, EventArgs e)
        {

        }
    }
    }
   


