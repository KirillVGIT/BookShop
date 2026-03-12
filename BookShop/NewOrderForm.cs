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
    public partial class NewOrderForm : Form
    {
        private string connStr = "server=127.0.0.1;user=root;password=root;database=dbbook50;";
        private int sellerId;
        private List<OrderItem> orderItems = new List<OrderItem>();
        public class OrderItem
        {
            public int ProductId { get; set; }
            public string Title { get; set; }
            public decimal Price { get; set; }
            public int Quantity { get; set; }
            public decimal Subtotal => Price * Quantity;
        }
        public NewOrderForm(int userId)
        {
            InitializeComponent();
            sellerId = userId;
        }

        private void NewOrderForm_Load(object sender, EventArgs e)
        {
            this.Text = "Новый заказ";
            LoadBooks();
            SetupDatePicker();
            UpdateTotal();
        }
        // Загрузка книг в ComboBox
        private void LoadBooks()
        {
            cmbBooks.Items.Clear();

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    string sql = @"SELECT product_id, title, price, stock_quantity 
                                 FROM product WHERE stock_quantity > 0 
                                 ORDER BY title";

                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                cmbBooks.Items.Add(new
                                {
                                    Id = reader.GetInt32(0),
                                    Title = reader.GetString(1),
                                    Price = reader.GetDecimal(2),
                                    Stock = reader.GetInt32(3)
                                });
                            }
                        }
                    }
                }

                if (cmbBooks.Items.Count > 0)
                {
                    cmbBooks.SelectedIndex = 0;
                    cmbBooks.DisplayMember = "Title";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки книг: " + ex.Message, "Ошибка");
            }
        }

        // Настройка DatePicker (календаря)
        private void SetupDatePicker()
        {
            // Минимальная дата - сегодня
            dateTimePicker1.MinDate = DateTime.Today;

            // Максимальная дата - через 30 дней
            dateTimePicker1.MaxDate = DateTime.Today.AddDays(30);

            // Значение по умолчанию - завтра
            dateTimePicker1.Value = DateTime.Today.AddDays(1);

            // Формат даты
            dateTimePicker1.Format = DateTimePickerFormat.Short;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (cmbBooks.SelectedItem == null)
            {
                MessageBox.Show("Выберите книгу", "Внимание");
                return;
            }

            dynamic book = cmbBooks.SelectedItem;
            int quantity = (int)numQuantity.Value;

            if (quantity > book.Stock)
            {
                MessageBox.Show($"Недостаточно на складе. Доступно: {book.Stock}", "Ошибка");
                return;
            }

            // Проверяем, не добавлена ли уже книга
            foreach (var item in orderItems)
            {
                if (item.ProductId == book.Id)
                {
                    item.Quantity += quantity;
                    UpdateItemsList();
                    UpdateTotal();
                    return;
                }
            }

            // Добавляем новую книгу
            orderItems.Add(new OrderItem
            {
                ProductId = book.Id,
                Title = book.Title,
                Price = book.Price,
                Quantity = quantity
            });

            UpdateItemsList();
            UpdateTotal();
        }
        // Обновление списка товаров
        private void UpdateItemsList()
        {
            listBox1.Items.Clear();
            foreach (var item in orderItems)
            {
                listBox1.Items.Add($"{item.Title} ({item.Price:C} × {item.Quantity}) = {item.Subtotal:C}");
            }
        }

        // Расчет итоговой суммы
        private void UpdateTotal()
        {
            decimal total = 0;
            foreach (var item in orderItems)
            {
                total += item.Subtotal;
            }

            decimal discount = (decimal)numDiscount.Value;
            decimal discountAmount = total * (discount / 100);
            decimal final = total - discountAmount;

            lblTotal.Text = $"{total:C}";
            lblFinal.Text = $"{final:C}";

            // Активируем кнопку, если есть товары
            btnCreateOrder.Enabled = orderItems.Count > 0;
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            if (listBox1.SelectedIndex >= 0)
            {
                orderItems.RemoveAt(listBox1.SelectedIndex);
                UpdateItemsList();
                UpdateTotal();
            }
        }

        private void numDiscount_ValueChanged(object sender, EventArgs e)
        {
            UpdateTotal();
        }

        private void numQuantity_ValueChanged(object sender, EventArgs e)
        {
            if (cmbBooks.SelectedItem != null)
            {
                dynamic book = cmbBooks.SelectedItem;
                if (numQuantity.Value > book.Stock)
                {
                    numQuantity.Value = book.Stock;
                }
            }
        }

        private void btnCreateOrder_Click(object sender, EventArgs e)
        {
            
            // Проверка данных
            if (orderItems.Count == 0)
            {
                MessageBox.Show("Добавьте товары в заказ", "Внимание");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtLastName.Text) ||
                string.IsNullOrWhiteSpace(txtFirstName.Text))
            {
                MessageBox.Show("Введите ФИО клиента", "Внимание");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtEmail.Text) || !txtEmail.Text.Contains("@"))
            {
                MessageBox.Show("Введите корректный email", "Внимание");
                return;
            }

            // Создание заказа
            try
            {
                decimal total = 0;
                foreach (var item in orderItems) total += item.Subtotal;
                decimal discount = (decimal)numDiscount.Value;
                decimal finalTotal = total - (total * discount / 100);

                // Дата доставки из календаря
                DateTime deliveryDate = dateTimePicker1.Value;

                // 1. Создаем заказ
                int orderId = CreateOrder(finalTotal, deliveryDate);

                // 2. Добавляем товары
                AddOrderItems(orderId);

                // 3. Обновляем остатки
                UpdateStock();

                MessageBox.Show($"Заказ #{orderId} создан!\nДата доставки: {deliveryDate:dd.MM.yyyy}\nСумма: {finalTotal:C}",
                              "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка: " + ex.Message, "Ошибка");
            }
        }

        private int CreateOrder(decimal total, DateTime deliveryDate)
        {
            string connStr = "server=127.0.0.1;user=root;password=root;database=dbbook50;";

            // Все заказы оформляем за продавцом seller1 (user_id = 3)
            // Если нужно другого продавца - поменяйте это число
            int sellerId = 3;

            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                conn.Open();

                string sql = @"INSERT INTO `order`
                      (user_id, customer_first_name, customer_last_name,
                       customer_email, customer_phone, order_date, total_amount, status)
                      VALUES (@userId, @firstName, @lastName,
                              @email, @phone, @orderDate, @total, 'В обработке');
                      SELECT LAST_INSERT_ID();";

                using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@userId", sellerId);
                    cmd.Parameters.AddWithValue("@firstName", Capitalize(txtFirstName.Text.Trim()));
                    cmd.Parameters.AddWithValue("@lastName", Capitalize(txtLastName.Text.Trim()));
                    cmd.Parameters.AddWithValue("@email", txtEmail.Text.Trim());
                    cmd.Parameters.AddWithValue("@phone", txtPhone.Text);
                    cmd.Parameters.AddWithValue("@orderDate", deliveryDate); // ← дата из календаря
                    cmd.Parameters.AddWithValue("@total", total);

                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        private void AddOrderItems(int orderId)
        {
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                conn.Open();

                foreach (var item in orderItems)
                {
                    string sql = @"INSERT INTO orderproduct 
                                  (order_id, product_id, quantity, unit_price)
                                  VALUES (@orderId, @productId, @quantity, @price)";

                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@orderId", orderId);
                        cmd.Parameters.AddWithValue("@productId", item.ProductId);
                        cmd.Parameters.AddWithValue("@quantity", item.Quantity);
                        cmd.Parameters.AddWithValue("@price", item.Price);

                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        private void UpdateStock()
        {
            using (MySqlConnection conn = new MySqlConnection(connStr))
            {
                conn.Open();

                foreach (var item in orderItems)
                {
                    string sql = @"UPDATE product 
                                  SET stock_quantity = stock_quantity - @quantity
                                  WHERE product_id = @productId";

                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@quantity", item.Quantity);
                        cmd.Parameters.AddWithValue("@productId", item.ProductId);

                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        private string Capitalize(string text)
        {
            if (string.IsNullOrEmpty(text)) return text;
            return char.ToUpper(text[0]) + text.Substring(1).ToLower();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void txtFirstName_Leave(object sender, EventArgs e)
        {
            txtFirstName.Text = Capitalize(txtFirstName.Text);
        }

        private void txtLastName_Leave(object sender, EventArgs e)
        {
            txtLastName.Text = Capitalize(txtLastName.Text);
        }

        private void txtFirstName_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Разрешаем: русские буквы, пробел, дефис, backspace
            char c = e.KeyChar;

            // Backspace разрешён
            if (c == (char)Keys.Back)
                return;

            // Проверка на русские буквы, пробел и дефис
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

