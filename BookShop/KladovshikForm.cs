using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//
using BookShop;

namespace BookShop
{
    public partial class KladovshikForm : Form
    {
        public KladovshikForm()
        {
            InitializeComponent();
            
            SessionManager.Initialize(this);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Text = "Панель кладовщика";
        }

        private void KladovshikForm_Load(object sender, EventArgs e)
        {

        }

        private void btnTovar_Click(object sender, EventArgs e)
        {
            // Создаем форму товаров
            TovarForm tovarForm = new TovarForm();

            // Устанавливаем Tag, чтобы TovarForm поняла, что это продавец
            tovarForm.Tag = "store";

            // Открываем форму
            tovarForm.ShowDialog();
        }

        private void btnStock_Click(object sender, EventArgs e)
        {
            // Открываем форму управления остатками
            StockForm stockForm = new StockForm();
            stockForm.ShowDialog();
        }

        private void btnCategories_Click(object sender, EventArgs e)
        {
            SimpleCategoriesForm categoriesForm = new SimpleCategoriesForm();
            categoriesForm.Show();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Вы уверены, что хотите выйти?", "Выход",
                                               MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (result == DialogResult.Yes)
            {
                LoginForm loginForm = new LoginForm();
                loginForm.Show();
                this.Close();
            }
        }
    }
}
