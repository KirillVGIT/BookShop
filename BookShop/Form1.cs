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
    public partial class LoginForm : Form
       // server=10.207.106.12; user=user83;password=qp96;database=db83;
    {
        string connStr = "server=127.0.0.1;user=root;password=;database=dbbook83;";
        public LoginForm()
        {
            InitializeComponent();
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {

        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text;
            string password = txtPassword.Text;

            if (username == "admin1" && password == "admin123")
            {
                AdminForm adminForm = new AdminForm();
                adminForm.Show();
                this.Hide();
            }
            else if (username == "seller1" && password == "sell123")
            {
                ProdavecForm prodavecForm = new ProdavecForm(); // Правильное название
                prodavecForm.Show();
                this.Hide();
            }
            else if (username == "store1" && password == "store123")
            {
                KladovshikForm kladovshikForm = new KladovshikForm(); // Правильное название
                kladovshikForm.Show();
                this.Hide();
            }
            else
            {
                MessageBox.Show("Неверный логин или пароль");
            }
        }
    }
}




