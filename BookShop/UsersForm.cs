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
    public partial class UsersForm : Form
    {
        string connStr = "server=127.0.0.1;user=root;password=root;database=dbbook50;";
        DataTable usersTable;
        DatabaseHelper dbHelper;
        public UsersForm()
        {
            InitializeComponent();
            dbHelper = new DatabaseHelper();
        }

        private void UsersForm_Load(object sender, EventArgs e)
        {
            LoadUsers();
            SetupDataGridView();
        }
        private void LoadUsers()
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    string sql = @"SELECT 
                                    u.user_id AS ID,
                                    u.username AS Логин,
                                    u.first_name AS Имя,
                                    u.last_name AS Фамилия,
                                    r.role_name AS Роль,
                                    u.email AS Email
                                  FROM user u
                                  JOIN role r ON u.role_id = r.role_id
                                  ORDER BY u.user_id";

                    MySqlDataAdapter da = new MySqlDataAdapter(sql, conn);
                    usersTable = new DataTable();
                    da.Fill(usersTable);
                    dataGridView1.DataSource = usersTable;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка при загрузке пользователей: " + ex.Message,
                              "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetupDataGridView()
        {
            // Настраиваем внешний вид таблицы
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.ReadOnly = true;

            // Скрываем колонку ID
            if (dataGridView1.Columns.Contains("ID"))
                dataGridView1.Columns["ID"].Visible = false;

            // Настраиваем ширину колонок
            if (dataGridView1.Columns.Contains("Логин"))
                dataGridView1.Columns["Логин"].Width = 100;

            if (dataGridView1.Columns.Contains("Имя"))
                dataGridView1.Columns["Имя"].Width = 100;

            if (dataGridView1.Columns.Contains("Фамилия"))
                dataGridView1.Columns["Фамилия"].Width = 100;

            if (dataGridView1.Columns.Contains("Роль"))
                dataGridView1.Columns["Роль"].Width = 100;
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            if (usersTable == null) return;

            string filter = "";

            if (!string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                filter = $"Логин LIKE '%{txtSearch.Text}%' OR " +
                         $"Имя LIKE '%{txtSearch.Text}%' OR " +
                         $"Фамилия LIKE '%{txtSearch.Text}%'";
            }

            (dataGridView1.DataSource as DataTable).DefaultView.RowFilter = filter;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            AddUserForm addForm = new AddUserForm();
            if (addForm.ShowDialog() == DialogResult.OK)
            {
                LoadUsers(); // Обновляем список после добавления
            }    
    }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                string username = dataGridView1.SelectedRows[0].Cells["Логин"].Value.ToString();
                string role = dataGridView1.SelectedRows[0].Cells["Роль"].Value.ToString();

                // Нельзя удалять администраторов
                if (role == "Администратор")
                {
                    MessageBox.Show("Нельзя удалять администраторов!", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                DialogResult result = MessageBox.Show($"Вы уверены, что хотите удалить пользователя '{username}'?",
                                                    "Подтверждение удаления",
                                                    MessageBoxButtons.YesNo,
                                                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    try
                    {
                        int userId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["ID"].Value);

                        using (MySqlConnection conn = new MySqlConnection(connStr))
                        {
                            conn.Open();
                            string query = "DELETE FROM user WHERE user_id = @id";

                            using (MySqlCommand cmd = new MySqlCommand(query, conn))
                            {
                                cmd.Parameters.AddWithValue("@id", userId);
                                cmd.ExecuteNonQuery();

                                MessageBox.Show("Пользователь успешно удален!", "Успех",
                                              MessageBoxButtons.OK, MessageBoxIcon.Information);
                                LoadUsers(); // Обновляем список
                            }
                        }
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
                MessageBox.Show("Выберите пользователя для удаления!", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                string username = dataGridView1.Rows[e.RowIndex].Cells["Логин"].Value.ToString();
                string firstName = dataGridView1.Rows[e.RowIndex].Cells["Имя"].Value.ToString();
                string lastName = dataGridView1.Rows[e.RowIndex].Cells["Фамилия"].Value.ToString();
                string role = dataGridView1.Rows[e.RowIndex].Cells["Роль"].Value.ToString();
                string email = dataGridView1.Rows[e.RowIndex].Cells["Email"].Value.ToString();

                string info = $"Логин: {username}\n" +
                             $"Имя: {firstName}\n" +
                             $"Фамилия: {lastName}\n" +
                             $"Роль: {role}\n" +
                             $"Email: {email}";

                MessageBox.Show(info, "Информация о пользователе",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                int userId = Convert.ToInt32(dataGridView1.SelectedRows[0].Cells["ID"].Value);
                string role = dataGridView1.SelectedRows[0].Cells["Роль"].Value.ToString();

                // Открываем форму редактирования
                AddUserForm editForm = new AddUserForm(userId);
                if (editForm.ShowDialog() == DialogResult.OK)
                {
                    LoadUsers(); // Обновляем список
                }
            }
            else
            {
                MessageBox.Show("Выберите пользователя для редактирования!", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}

