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
    public partial class ChangeOrderStatusForm : Form
    {
        private int orderId;
        private string currentStatus;
        private string connStr = "server=127.0.0.1;user=root;password=root;database=dbbook50;";

        // Свойство для получения нового статуса
        public string NewStatus { get; private set; }
        public ChangeOrderStatusForm(int orderId, string customerName, string currentStatus)
        {
            InitializeComponent();

            this.orderId = orderId;
            this.currentStatus = currentStatus;

            // Заполняем форму данными
            lblOrderId.Text = orderId.ToString();
            lblCustomer.Text = customerName;
            lblCurrentStatus.Text = currentStatus;

            // Заполняем ComboBox возможными статусами
            comboNewStatus.Items.AddRange(new string[]
            {
                "В обработке",
                "Оплачен",
                "Доставлен"
            });

            // Выбираем текущий статус
            comboNewStatus.SelectedItem = currentStatus;

            // Настройки формы
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Text = "Изменение статуса заказа";
        }
    

        private void ChangeOrderStatusForm_Load(object sender, EventArgs e)
        {

        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            NewStatus = comboNewStatus.SelectedItem.ToString();

            // Проверяем, изменился ли статус
            if (NewStatus == currentStatus)
            {
                MessageBox.Show("Статус не изменен!", "Информация",
                              MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Обновляем статус в БД
            bool success = UpdateOrderStatus(orderId, NewStatus);

            if (success)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private bool UpdateOrderStatus(int orderId, string newStatus)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    string query = "UPDATE `order` SET status = @status WHERE order_id = @id";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@status", newStatus);
                        cmd.Parameters.AddWithValue("@id", orderId);

                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show($"Статус заказа №{orderId} успешно изменен на '{newStatus}'!", "Успех",
                                          MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return true;
                        }
                        else
                        {
                            MessageBox.Show("Не удалось изменить статус заказа!", "Ошибка",
                                          MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при изменении статуса: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
