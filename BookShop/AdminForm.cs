using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using BookShop;

namespace BookShop
{
    public partial class AdminForm : Form
    {
        public AdminForm()
        {
            InitializeComponent();
            SessionManager.Initialize(this);
        }

        private void btnTovar_Click(object sender, EventArgs e)
        {
            TovarForm tovarForm = new TovarForm();
            tovarForm.Show();
        }

        private void btnUser_Click(object sender, EventArgs e)
        {
            UsersForm usersForm = new UsersForm();
            usersForm.Show();
        }

        private void btnCategory_Click(object sender, EventArgs e)
        {
            CategoriesForm categoriesForm = new CategoriesForm();
            categoriesForm.Show();
        }

        private void btnOrders_Click(object sender, EventArgs e)
        {
            OrdersForm ordersForm = new OrdersForm();
            ordersForm.Show();
        }

        private void btnReport_Click(object sender, EventArgs e)
        {
            ReportForm report = new ReportForm();
            report.ShowDialog();
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

