using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;



namespace BookShop
{
    public partial class AddProductForm : Form
    {
        private DatabaseHelper dbHelper;
        private int productId = 0; // 0 = добавление, >0 = редактирование
        private bool isEditMode = false;
        private string selectedImagePath = null;
        private string currentImagePath = null;
        string connStr = "server=127.0.0.1;user=root;password=root;database=dbbook50;";
        public AddProductForm()
        {
            InitializeComponent();
            dbHelper = new DatabaseHelper();
            LoadCategories();
            // Показываем заглушку
            picBookImage.Image = dbHelper.GetProductImage("no_image.jpg");
            btnDeleteImage.Visible = false;
            this.Text = "Добавление товара";
        }
        // Конструктор для РЕДАКТИРОВАНИЯ
        public AddProductForm(int id) : this()
        {
            productId = id;
            isEditMode = true;
            this.Text = "Редактирование товара";
            LoadProductData();
        }
        private void LoadCategories()
        {
            var categories = dbHelper.GetCategories();
            comboBoxCategory.DataSource = categories;
            comboBoxCategory.DisplayMember = "Name";
            comboBoxCategory.ValueMember = "CategoryId";

            if (comboBoxCategory.Items.Count > 0)
                comboBoxCategory.SelectedIndex = 0;
        }
        private void LoadProductData()
        {
            if (productId <= 0) return;

            var product = dbHelper.GetProductById(productId);
            if (product != null)
            {
                txtTitle.Text = product.Title;
                txtAuthor.Text = product.Author;
                txtPrice.Text = product.Price.ToString();
                txtQuantity.Text = product.StockQuantity.ToString();
                comboBoxCategory.SelectedValue = product.CategoryId;
            }
            // Загрузка картинки
            currentImagePath = product.ImagePath;
            picBookImage.Image = dbHelper.GetProductImage(currentImagePath);
            btnDeleteImage.Visible = !string.IsNullOrEmpty(currentImagePath) && currentImagePath != "no_image.jpg";
        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void AddProductForm_Load(object sender, EventArgs e)
        {

        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            // Проверка ввода
            if (string.IsNullOrEmpty(txtTitle.Text.Trim()))
            {
                MessageBox.Show("Введите название книги!", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTitle.Focus();
                return;
            }

            if (string.IsNullOrEmpty(txtAuthor.Text.Trim()))
            {
                MessageBox.Show("Введите автора книги!", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtAuthor.Focus();
                return;
            }

            if (!decimal.TryParse(txtPrice.Text, out decimal price) || price <= 0)
            {
                MessageBox.Show("Введите корректную цену!", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPrice.Focus();
                return;
            }

            if (!int.TryParse(txtQuantity.Text, out int quantity) || quantity < 0)
            {
                MessageBox.Show("Введите корректное количество!", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtQuantity.Focus();
                return;
            }

            string title = txtTitle.Text.Trim();
            string author = txtAuthor.Text.Trim();
            int categoryId = Convert.ToInt32(comboBoxCategory.SelectedValue);

            bool success;

            if (isEditMode)
            {
                // РЕДАКТИРОВАНИЕ
                success = dbHelper.UpdateProduct(productId, title, author, price, quantity, categoryId);

                if (success)
                {
                    // 1. ЕСЛИ ВЫБРАЛИ НОВОЕ ФОТО
                    if (!string.IsNullOrEmpty(selectedImagePath))
                    {
                        // Сохраняем файл и получаем имя
                        string fileName = dbHelper.SaveProductImage(selectedImagePath, productId);

                        // Обновляем путь в базе
                        using (MySqlConnection conn = new MySqlConnection(connStr))
                        {
                            conn.Open();
                            string sql = "UPDATE product SET image_path = @path WHERE product_id = @id";
                            using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                            {
                                cmd.Parameters.AddWithValue("@path", fileName);
                                cmd.Parameters.AddWithValue("@id", productId);
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                    // 2. ЕСЛИ УДАЛИЛИ ФОТО (кнопка удалить стала невидимой)
                    else if (btnDeleteImage.Visible == false && currentImagePath != null && currentImagePath != "no_image.jpg")
                    {
                        // Удаляем старый файл
                        string oldPath = Path.Combine(Application.StartupPath, "Images", currentImagePath);
                        if (File.Exists(oldPath))
                            File.Delete(oldPath);

                        // Ставим заглушку в базе
                        using (MySqlConnection conn = new MySqlConnection(connStr))
                        {
                            conn.Open();
                            string sql = "UPDATE product SET image_path = 'no_image.jpg' WHERE product_id = @id";
                            using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                            {
                                cmd.Parameters.AddWithValue("@id", productId);
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                }
            
            else if (btnDeleteImage.Visible == false && currentImagePath != null)
            {
                // Удалили фото
                string connStr = "server=127.0.0.1;user=root;password=;database=dbbook50;";
                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    string sql = "UPDATE product SET image_path = 'no_image.jpg' WHERE product_id = @id";
                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", productId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            }
            else
            {
                
                // ДОБАВЛЕНИЕ
                int newProductId = dbHelper.AddProduct(title, author, price, quantity, categoryId);
                // СОХРАНЯЕМ КАРТИНКУ (ДОБАВЛЕНИЕ)
                // СОХРАНЯЕМ КАРТИНКУ
                if (!string.IsNullOrEmpty(selectedImagePath))
                {
                    string fileName = dbHelper.SaveProductImage(selectedImagePath, newProductId);

                    using (MySqlConnection conn = new MySqlConnection(connStr))
                    {
                        conn.Open();
                        string sql = "UPDATE product SET image_path = @path WHERE product_id = @id";
                        using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                        {
                            cmd.Parameters.AddWithValue("@path", fileName);
                            cmd.Parameters.AddWithValue("@id", newProductId);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                success = true;
            
        }

            if (success)
            {
                MessageBox.Show("Книга успешно добавлена!", "Успех",
                               MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
                this.Close();
            } 
        }

        private void btnCanel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void txtPrice_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
            {
                e.Handled = true;
            }

            // Проверяем, что точка только одна
            if (e.KeyChar == '.' && (sender as TextBox).Text.IndexOf('.') > -1)
            {
                e.Handled = true;
            }
        }

        private void txtQuantity_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void btnSelectImage_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Title = "Выберите обложку книги";
                ofd.Filter = "Файлы изображений|*.jpg;*.jpeg;*.png;*.bmp";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    selectedImagePath = ofd.FileName;
                    picBookImage.Image = Image.FromFile(selectedImagePath);
                    btnDeleteImage.Visible = true;
                }
            }
        }

        private void btnDeleteImage_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Удалить изображение?", "Подтверждение",
                                         MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                selectedImagePath = null;
                currentImagePath = "no_image.jpg";
                picBookImage.Image = dbHelper.GetProductImage("no_image.jpg");
                btnDeleteImage.Visible = false;
            }
        }
    }
}

       
