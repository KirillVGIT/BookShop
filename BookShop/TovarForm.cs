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
using System.IO;

namespace BookShop
{
    public partial class TovarForm : Form
    {
        //string connStr = "server=10.207.106.12;user=user83;password=qp96;database=db83;";
        string connStr = "server=127.0.0.1;user=root;password=root;database=dbbook50;";
        DataTable productsTable;
        DatabaseHelper dbHelper;
        private int currentPage = 1;
        private int totalPages = 1;
        private int totalRecords = 0;
        private int pageSize = 20;
        private DataTable fullDataTable; // для хранения всех данных при фильтрации

        // Добавляем публичные свойства для управления кнопками
        public bool ShowAddButton { get; set; } = true;
        public bool ShowEditButton { get; set; } = true;
        public bool ShowDeleteButton { get; set; } = true;
        public TovarForm()
        {
            InitializeComponent();
            dbHelper = new DatabaseHelper();
        }

        private void TovarForm_Load(object sender, EventArgs e)
        {
            LoadCategories();
            LoadProducts();
            SetupDataGridView();


            // Проверяем, если Tag = "seller" - скрываем кнопки
            if (this.Tag != null && this.Tag.ToString() == "seller")
            {
                btnAdd.Visible = false;
                btnEdit.Visible = false;
                btnDelete.Visible = false;
                this.Text = "Просмотр книг (Продавец)";
            }
            // Проверяем, если Tag = "seller" - скрываем кнопки
            if (this.Tag != null && this.Tag.ToString() == "store")
            {
                btnAdd.Visible = false;
                btnEdit.Visible = false;
                btnDelete.Visible = false;
                this.Text = "Просмотр книг (Кладовщик)";
            }
        }

        private int GetTotalRecords()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    string sql = "SELECT COUNT(*) FROM product";
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        return Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка подсчёта записей: " + ex.Message);
                return 0;
            }
        }
        private void LoadCategories()
        {
            try
            {


                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    string sql = "SELECT category_id AS id, name FROM category";
                    MySqlDataAdapter da = new MySqlDataAdapter(sql, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    comboCategory.DataSource = dt;
                    comboCategory.DisplayMember = "name";
                    comboCategory.ValueMember = "id";
                    comboCategory.SelectedIndex = -1;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке категорий: " + ex.Message);
            }
        }
        private void LoadProducts()
        {
            try
            {
                // Получаем общее количество записей
                totalRecords = GetTotalRecords();
                totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

                // Корректируем текущую страницу
                if (currentPage < 1) currentPage = 1;
                if (currentPage > totalPages) currentPage = totalPages;

                int offset = (currentPage - 1) * pageSize;

                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    string sql = @"SELECT
                    p.product_id AS ID,
                    p.title AS Название,
                    p.author AS Автор,
                    p.price AS Цена,
                    c.name AS Категория,
                    p.stock_quantity AS Остаток,
                    p.image_path
                  FROM product p
                  JOIN category c ON p.category_id = c.category_id
                  ORDER BY p.title
                  LIMIT @pageSize OFFSET @offset";

                    MySqlDataAdapter da = new MySqlDataAdapter(sql, conn);
                    da.SelectCommand.Parameters.AddWithValue("@pageSize", pageSize);
                    da.SelectCommand.Parameters.AddWithValue("@offset", offset);

                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    // СОЗДАЁМ КОЛОНКУ ДЛЯ КАРТИНОК
                    DataColumn imageColumn = new DataColumn("Обложка", typeof(Image));
                    dt.Columns.Add(imageColumn);
                    imageColumn.SetOrdinal(0);

                    // ЗАГРУЖАЕМ КАРТИНКИ
                    DatabaseHelper db = new DatabaseHelper();
                    foreach (DataRow row in dt.Rows)
                    {
                        string imagePath = row["image_path"]?.ToString();
                        row["Обложка"] = db.GetProductImage(imagePath);
                    }

                    // СКРЫВАЕМ СЛУЖЕБНЫЕ ПОЛЯ
                    dt.Columns["ID"].ColumnMapping = MappingType.Hidden;
                    dt.Columns["image_path"].ColumnMapping = MappingType.Hidden;

                    // ПРИВЯЗЫВАЕМ ДАННЫЕ
                    dataGridView1.DataSource = dt;
                    productsTable = dt;

                    // Обновляем информацию о страницах
                    UpdatePageInfo();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке товаров: " + ex.Message);
            }
        }

        private void UpdatePageInfo()
        {
            
            // Обновляем информацию о странице
            lblPageInfo.Text = $"Страница {currentPage} из {totalPages}";

            // Вычисляем диапазон записей на текущей странице
            int startRecord = (currentPage - 1) * pageSize + 1;
            int endRecord = Math.Min(currentPage * pageSize, totalRecords);
            lblRecordInfo.Text = $"{startRecord}-{endRecord} из {totalRecords}";

            // Включаем/выключаем кнопки навигации
            btnFirstPage.Enabled = currentPage > 1;
            btnPrevPage.Enabled = currentPage > 1;
            btnNextPage.Enabled = currentPage < totalPages;
            btnLastPage.Enabled = currentPage < totalPages;
        }
        private void SetupDataGridView()
        {
            // Настройка внешнего вида DataGridView
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.ReadOnly = true;

            // Скрываем колонку ID
            if (dataGridView1.Columns.Contains("ID"))
                dataGridView1.Columns["ID"].Visible = false;
        }

        private void comboCategory_SelectedIndexChanged(object sender, EventArgs e)
        {
            ApplyFilters();
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            ApplyFilters();
        }
        private void ApplyFilters()
        {
            if (productsTable == null) return;

            string filter = "";

            if (!string.IsNullOrWhiteSpace(txtSearch.Text))
                filter += $"Название LIKE '%{txtSearch.Text}%'";

            if (comboCategory.SelectedIndex != -1)
            {
                if (filter != "") filter += " AND ";
                filter += $"Категория = '{comboCategory.Text}'";
            }

            (dataGridView1.DataSource as DataTable).DefaultView.RowFilter = filter;
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            txtSearch.Text = "";
            comboCategory.SelectedIndex = -1;
            (dataGridView1.DataSource as DataTable).DefaultView.RowFilter = "";
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            AddProductForm addForm = new AddProductForm();
            if (addForm.ShowDialog() == DialogResult.OK)
            {
                LoadProducts(); // Обновляем список после добавления
            }
        }



        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                // ПОЛУЧАЕМ ID ЧЕРЕЗ DataBoundItem (КАК В btnEdit)
                DataRowView rowView = (DataRowView)dataGridView1.SelectedRows[0].DataBoundItem;
                int productId = Convert.ToInt32(rowView["ID"]);
                string productName = rowView["Название"].ToString();

                DialogResult result = MessageBox.Show($"Вы уверены, что хотите удалить товар '{productName}'?",
                                                    "Подтверждение удаления",
                                                    MessageBoxButtons.YesNo,
                                                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    string imagePath = null; // ОБЪЯВЛЯЕМ ПЕРЕМЕННУЮ ЗДЕСЬ, ЧТОБЫ БЫЛА ДОСТУПНА ВЕЗДЕ

                    try
                    {
                        using (MySqlConnection conn = new MySqlConnection(connStr))
                        {
                            conn.Open();

                            // СНАЧАЛА ПОЛУЧАЕМ ИНФОРМАЦИЮ О КАРТИНКЕ ДЛЯ УДАЛЕНИЯ
                            string selectImageQuery = "SELECT image_path FROM product WHERE product_id = @id";

                            using (MySqlCommand selectCmd = new MySqlCommand(selectImageQuery, conn))
                            {
                                selectCmd.Parameters.AddWithValue("@id", productId);
                                imagePath = selectCmd.ExecuteScalar()?.ToString(); // ТЕПЕРЬ ПРИСВАИВАЕМ ЗНАЧЕНИЕ
                            }

                            // УДАЛЯЕМ ТОВАР ИЗ БД
                            string deleteQuery = "DELETE FROM product WHERE product_id = @id";
                            using (MySqlCommand cmd = new MySqlCommand(deleteQuery, conn))
                            {
                                cmd.Parameters.AddWithValue("@id", productId);
                                cmd.ExecuteNonQuery();
                            }
                        } // ЗДЕСЬ СОЕДИНЕНИЕ ЗАКРЫВАЕТСЯ, НО imagePath УЖЕ СОХРАНЕН

                        // УДАЛЯЕМ ФАЙЛ КАРТИНКИ (ЕСЛИ ОН ЕСТЬ И ЭТО НЕ "no_image.jpg")
                        if (!string.IsNullOrEmpty(imagePath) && imagePath != "no_image.jpg")
                        {
                            string fullPath = Path.Combine(Application.StartupPath, "Images", imagePath);
                            if (File.Exists(fullPath))
                            {
                                File.Delete(fullPath);
                            }
                        }

                        MessageBox.Show("Товар успешно удален!", "Успех",
                                      MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadProducts(); // Обновляем список
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка",
                                      MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите товар для удаления!", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        private int GetCategoryIdByName(string categoryName)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    string query = "SELECT category_id FROM category WHERE name = @name";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@name", categoryName);
                        object result = cmd.ExecuteScalar();

                        return result != null ? Convert.ToInt32(result) : 1;
                    }
                }
            }
            catch (Exception)
            {
                return 1;
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                // Получаем ID через DataTable, а не через колонки DataGridView
                DataRowView rowView = (DataRowView)dataGridView1.SelectedRows[0].DataBoundItem;
                int productId = Convert.ToInt32(rowView["ID"]);

                AddProductForm editForm = new AddProductForm(productId);
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    LoadProducts();
                }
            }
            else
            {
                MessageBox.Show("Выберите товар для редактирования!", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}



