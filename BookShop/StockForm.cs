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
    public partial class StockForm : Form
    {
        private string connStr = "server=127.0.0.1;user=root;password=root;database=dbbook50;";
        private DataTable productsTable;
        private int selectedProductId = 0;
        public StockForm()
        {
            InitializeComponent();

            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Text = "Управление остатками на складе";

            LoadProducts();
            SetupDataGridView();
        }

        private void LoadProducts()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    string sql = @"SELECT 
                                    p.product_id AS ID,
                                    p.title AS Название,
                                    p.author AS Автор,
                                    p.stock_quantity AS Остаток
                                   FROM product p
                                   ORDER BY p.title";

                    MySqlDataAdapter da = new MySqlDataAdapter(sql, conn);
                    productsTable = new DataTable();
                    da.Fill(productsTable);
                    dataGridView1.DataSource = productsTable;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке товаров: " + ex.Message,
                              "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetupDataGridView()
        {
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.ReadOnly = true;

            // Убедитесь, что колонка ID есть и она скрыта
            if (dataGridView1.Columns.Contains("ID"))
                dataGridView1.Columns["ID"].Visible = false;

            // Проверьте наличие других колонок
            if (!dataGridView1.Columns.Contains("Название"))
                MessageBox.Show("Нет колонки 'Название'!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);

            if (!dataGridView1.Columns.Contains("Остаток"))
                MessageBox.Show("Нет колонки 'Остаток'!", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        

        private void StockForm_Load(object sender, EventArgs e)
        {

        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (selectedProductId == 0)
            {
                MessageBox.Show("Выберите товар для изменения остатка!", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(txtNewStock.Text, out int newStock) || newStock < 0)
            {
                MessageBox.Show("Введите корректное количество (целое число, не меньше 0)!", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtNewStock.Focus();
                txtNewStock.SelectAll();
                return;
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    string query = "UPDATE product SET stock_quantity = @quantity WHERE product_id = @id";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@quantity", newStock);
                        cmd.Parameters.AddWithValue("@id", selectedProductId);

                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Остаток успешно обновлен!", "Успех",
                                          MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadProducts(); // Обновляем список
                                            // Сбрасываем выбор
                            dataGridView1.ClearSelection();
                            lblBookName.Text = "Выберите товар из таблицы";
                            lblCurrent.Text = "Текущий остаток: 0";
                            txtNewStock.Text = "0";
                            selectedProductId = 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении остатка: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            if (productsTable == null) return;

            string filter = "";

            if (!string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                filter = $"Название LIKE '%{txtSearch.Text}%' OR " +
                         $"Автор LIKE '%{txtSearch.Text}%'";
            }

            (dataGridView1.DataSource as DataTable).DefaultView.RowFilter = filter;
        }

        private void txtNewStock_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Разрешаем только цифры и Backspace
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                DataGridViewRow row = dataGridView1.SelectedRows[0];

                // Проверяем, что строка не пустая
                if (row.Cells["ID"].Value != null && row.Cells["Название"].Value != null)
                {
                    selectedProductId = Convert.ToInt32(row.Cells["ID"].Value);
                    string bookName = row.Cells["Название"].Value.ToString();
                    string currentStock = row.Cells["Остаток"].Value?.ToString() ?? "0";

                    // Обновляем элементы формы
                    lblBookName.Text = $"Выбрана книга: {bookName}";
                    lblCurrent.Text = $"Текущий остаток: {currentStock} шт.";
                    txtNewStock.Text = currentStock;

                    // Фокус на поле ввода
                    txtNewStock.Focus();
                    txtNewStock.SelectAll();
                }
            }
        }
    }
    }




