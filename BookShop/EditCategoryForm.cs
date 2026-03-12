using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace BookShop
{
    public partial class EditCategoryForm : Form
    {
        private DatabaseHelper dbHelper;
        private int categoryId;
        public EditCategoryForm(int id, string name, string description)
        {
            InitializeComponent();
            categoryId = id;
            dbHelper = new DatabaseHelper();

            // Заполняем поля данными категории
            txtName.Text = name;
            txtDescription.Text = description;

            // Настройки формы
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Text = "Редактирование категории";
        }

        private void EditCategoryForm_Load(object sender, EventArgs e)
        {

        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            string name = txtName.Text.Trim();
            string description = txtDescription.Text.Trim();

            // Проверка ввода
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Введите название категории!", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return;
            }

            // Обновляем категорию в БД
            bool success = dbHelper.UpdateCategory(categoryId, name, description);

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
    }
}
