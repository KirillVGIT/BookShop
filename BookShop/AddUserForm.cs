using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BookShop
{
    public partial class AddUserForm : Form
    {
        private DatabaseHelper dbHelper;
        private int userId = 0; // 0 = добавление, >0 = редактирование
        private bool isEditMode = false;
        public AddUserForm()
        {
            InitializeComponent();
            dbHelper = new DatabaseHelper();
            LoadRoles();
            // Устанавливаем текст кнопки и заголовок
            button1.Text = "Добавить";
            this.Text = "Добавление пользователя";

            // При добавлении пароль обязателен
            txtPassword.Enabled = true;
            txtConfirmPassword.Enabled = true;
        }
        // Конструктор для РЕДАКТИРОВАНИЯ
        public AddUserForm(int id) : this()
        {
            userId = id;
            isEditMode = true;

            // Меняем текст кнопки и заголовок
            button1.Text = "Сохранить";
            this.Text = "Редактирование пользователя";

            // Загружаем данные пользователя
            LoadUserData();
        }

        private void LoadRoles()
        {
            var roles = dbHelper.GetRoles();
            comboRole.DataSource = roles;
            comboRole.DisplayMember = "RoleName";
            comboRole.ValueMember = "RoleId";

            if (comboRole.Items.Count > 0)
                comboRole.SelectedIndex = 0;
        }
        private void LoadUserData()
        {
            if (userId <= 0) return;

            var user = dbHelper.GetUserById(userId);
            if (user != null)
            {
                txtUsername.Text = user.Username;
                txtEmail.Text = user.Email;
                txtFirstName.Text = user.FirstName;
                txtLastName.Text = user.LastName;
                comboRole.SelectedValue = user.RoleId;

                // При редактировании пароль не изменяется
                txtPassword.Text = "********";
                txtConfirmPassword.Text = "********";
                txtPassword.Enabled = false;
                txtConfirmPassword.Enabled = false;

                // Меняем подписи
                lblPassword.Text = "Пароль (не изменяется):";
                lblConfirmPassword.Text = "Подтверждение:";
            }
        }

        private void AddUserForm_Load(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Проверка ввода логина и ФИО
            if (string.IsNullOrEmpty(txtUsername.Text.Trim()))
            {
                MessageBox.Show("Введите логин!", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtUsername.Focus();
                return;
            }

            if (string.IsNullOrEmpty(txtFirstName.Text.Trim()))
            {
                MessageBox.Show("Введите имя!", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtFirstName.Focus();
                return;
            }

            if (string.IsNullOrEmpty(txtLastName.Text.Trim()))
            {
                MessageBox.Show("Введите фамилию!", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtLastName.Focus();
                return;
            }

            // Проверка пароля только при добавлении
            if (!isEditMode)
            {
                if (string.IsNullOrEmpty(txtPassword.Text))
                {
                    MessageBox.Show("Введите пароль!", "Внимание",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtPassword.Focus();
                    return;
                }

                if (txtPassword.Text != txtConfirmPassword.Text)
                {
                    MessageBox.Show("Пароли не совпадают!", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtConfirmPassword.Focus();
                    return;
                }
            }

            // Получаем данные
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;
            string email = txtEmail.Text.Trim();
            string firstName = txtFirstName.Text.Trim();
            string lastName = txtLastName.Text.Trim();
            int roleId = Convert.ToInt32(comboRole.SelectedValue);

            bool success;

            if (isEditMode)
            {
                // РЕДАКТИРОВАНИЕ пользователя
                success = dbHelper.UpdateUser(userId, username, email, firstName, lastName, roleId);
            }
            else
            {
                // ДОБАВЛЕНИЕ пользователя
                success = dbHelper.AddUser(username, password, email, firstName, lastName, roleId);
            }

            if (success)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }


        private void btnCanel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void txtEmail_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtFirstName_Leave(object sender, EventArgs e)
        {
            // Делаем первую букву заглавной
            txtFirstName.Text = CapitalizeFirstLetter(txtFirstName.Text);
        }

        private void txtLastName_Leave(object sender, EventArgs e)
        {
            txtLastName.Text = CapitalizeFirstLetter(txtLastName.Text);
        }
        private string CapitalizeFirstLetter(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return text;

            text = text.Trim();

            // Первая буква заглавная, остальные маленькие
            return char.ToUpper(text[0]) + text.Substring(1).ToLower();
        }

        private void txtFirstName_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Разрешаем: русские буквы, пробел, backspace
            char c = e.KeyChar;

            // Backspace разрешён
            if (c == (char)Keys.Back)
                return;

            // Проверка на русские буквы и пробел
            if (!(c >= 'а' && c <= 'я' ||
                  c >= 'А' && c <= 'Я' ||
                  c == ' ' ||
                  c == '-' ||
                  c == 'ё' ||
                  c == 'Ё'))
            {
                e.Handled = true; // Запрещаем ввод
            }
        }

        private void txtLastName_KeyPress(object sender, KeyPressEventArgs e)
        {
            char c = e.KeyChar;

            if (c == (char)Keys.Back)
                return;

            if (!(c >= 'а' && c <= 'я' ||
                  c >= 'А' && c <= 'Я' ||
                  c == ' ' ||
                  c == '-' ||
                  c == 'ё' ||
                  c == 'Ё'))
            {
                e.Handled = true;
            }
        }
    }
}

