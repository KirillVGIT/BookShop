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
    public partial class CategoriesForm : Form
    {
        string connStr = "server=127.0.0.1;user=root;password=root;database=dbbook50;";
        private DatabaseHelper dbHelper;
        
        public CategoriesForm()
        {
            InitializeComponent();
            dbHelper = new DatabaseHelper();
            LoadCategories();
            // Если режим только для чтения - скрываем элементы редактирования
            
        }
    
        private void LoadCategories()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    string sql = "SELECT category_id AS ID, name AS Название, description AS Описание FROM category";
                    MySqlDataAdapter da = new MySqlDataAdapter(sql, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    dataGridView1.DataSource = dt;

                    // Скрываем колонку ID
                    if (dataGridView1.Columns.Contains("ID"))
                        dataGridView1.Columns["ID"].Visible = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке категорий: " + ex.Message);
            }
        }

        private void CategoriesForm_Load(object sender, EventArgs e)
        {

        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            string name = txtName.Text.Trim();
            string description = txtDescription.Text.Trim();

            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Введите название категории!", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool success = dbHelper.AddCategory(name, description);

            if (success)
            {
                txtName.Text = "";
                txtDescription.Text = "";
                LoadCategories(); // Обновляем список
            }
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int categoryId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["ID"].Value);
                string name = dataGridView1.SelectedRows[0].Cells["Название"].Value.ToString();
                string description = dataGridView1.SelectedRows[0].Cells["Описание"].Value.ToString();

                // Открываем форму редактирования
                EditCategoryForm editForm = new EditCategoryForm(categoryId, name, description);
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    LoadCategories(); // Обновляем список категорий
                }
            }
            else
            {
                MessageBox.Show("Выберите категорию для редактирования!", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
