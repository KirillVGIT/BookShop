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
    public partial class ProdavecForm : Form
    {
        // Переменные для хранения данных пользователя
        private int currentUserId;
        private string currentUserName;
        public ProdavecForm()
        {
            InitializeComponent();
            SessionManager.Initialize(this);
            // Сохраняем кто вошел
            
        }

        private void ProdavecForm_Load(object sender, EventArgs e)
        {

        }

        private void btnProducts_Click(object sender, EventArgs e)
        {
            // Создаем форму товаров
            TovarForm tovarForm = new TovarForm();

            // Устанавливаем Tag, чтобы TovarForm поняла, что это продавец
            tovarForm.Tag = "seller";

            // Открываем форму
            tovarForm.ShowDialog();
        }

        private void btnOrders_Click(object sender, EventArgs e)
        {
            OrderpForm orderpForm = new OrderpForm(currentUserId);

            // Открываем форму
            orderpForm.ShowDialog();
        }

        private void btnNewOrder_Click(object sender, EventArgs e)
        {
            NewOrderForm newOrderForm = new NewOrderForm(currentUserId);
            DialogResult result = newOrderForm.ShowDialog();

            if (result == DialogResult.OK)
            {
                MessageBox.Show(" Заказ успешно создан!", "Успех",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
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
