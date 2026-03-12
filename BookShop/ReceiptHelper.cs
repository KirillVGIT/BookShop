using System;
using System.Collections.Generic;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace BookShop
{
    public class ReceiptHelper
    {
        private string connStr = "server=127.0.0.1;user=root;password=root;database=dbbook50;";

        public void CreateReceipt(int orderId)
        {
            try
            {
                // ============ ПОЛУЧАЕМ ДАННЫЕ ЗАКАЗА ============
                string orderNumber = "", orderDate = "", customer = "", phone = "", email = "", seller = "", total = "";

                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    string sql = @"SELECT
                            o.order_id,
                            DATE_FORMAT(o.order_date, '%d.%m.%Y %H:%i') as order_date,
                            CONCAT(o.customer_first_name, ' ', o.customer_last_name) as customer,
                            o.customer_phone,
                            o.customer_email,
                            CONCAT(u.first_name, ' ', u.last_name) as seller,
                            o.total_amount
                          FROM `order` o
                          LEFT JOIN user u ON o.user_id = u.user_id
                          WHERE o.order_id = @id";

                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", orderId);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                orderNumber = reader["order_id"].ToString();
                                orderDate = reader["order_date"].ToString();
                                customer = reader["customer"].ToString();
                                phone = reader["customer_phone"].ToString();
                                email = reader["customer_email"].ToString();
                                seller = reader["seller"].ToString();
                                total = Convert.ToDecimal(reader["total_amount"]).ToString("C");
                            }
                        }
                    }
                }

                // ============ ПОЛУЧАЕМ ТОВАРЫ ============
                List<string> items = new List<string>();
                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    string sql = @"SELECT
                            p.title,
                            op.quantity,
                            op.unit_price,
                            (op.quantity * op.unit_price) as total
                          FROM orderproduct op
                          JOIN product p ON op.product_id = p.product_id
                          WHERE op.order_id = @id";

                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@id", orderId);
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                items.Add($"{reader["title"]}|{reader["quantity"]}|{reader["unit_price"]}|{reader["total"]}");
                            }
                        }
                    }
                }

                // ============ СОЗДАЁМ WORD ============
                Microsoft.Office.Interop.Word.Application wordApp = new Microsoft.Office.Interop.Word.Application();
                wordApp.Visible = true;

                Microsoft.Office.Interop.Word.Document wordDoc = wordApp.Documents.Add();

                // === 1. ЗАГОЛОВОК ===
                Microsoft.Office.Interop.Word.Paragraph p1 = wordDoc.Content.Paragraphs.Add();
                p1.Range.Text = "КНИЖНЫЙ МАГАЗИН";
                p1.Range.Font.Name = "Arial";
                p1.Range.Font.Size = 18;
                p1.Range.Font.Bold = 1;
                p1.Range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphCenter;
                p1.Range.InsertParagraphAfter();

                Microsoft.Office.Interop.Word.Paragraph p2 = wordDoc.Content.Paragraphs.Add();
                p2.Range.Text = $"ЧЕК ЗАКАЗА №{orderNumber}";
                p2.Range.Font.Name = "Arial";
                p2.Range.Font.Size = 16;
                p2.Range.Font.Bold = 1;
                p2.Range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphCenter;
                p2.Range.InsertParagraphAfter();

                // === 2. ДАТА ===
                Microsoft.Office.Interop.Word.Paragraph p3 = wordDoc.Content.Paragraphs.Add();
                p3.Range.Text = $"Дата заказа: {orderDate}";
                p3.Range.Font.Name = "Arial";
                p3.Range.Font.Size = 11;
                p3.Range.Font.Bold = 0;
                p3.Range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphRight;
                p3.Range.InsertParagraphAfter();

                // === 3. КЛИЕНТ ===
                Microsoft.Office.Interop.Word.Paragraph p4 = wordDoc.Content.Paragraphs.Add();
                p4.Range.Text = "ИНФОРМАЦИЯ О КЛИЕНТЕ:";
                p4.Range.Font.Name = "Arial";
                p4.Range.Font.Size = 12;
                p4.Range.Font.Bold = 1;
                p4.Range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphLeft;
                p4.Range.InsertParagraphAfter();

                Microsoft.Office.Interop.Word.Paragraph p5 = wordDoc.Content.Paragraphs.Add();
                p5.Range.Text = $"Клиент: {customer}";
                p5.Range.Font.Name = "Arial";
                p5.Range.Font.Size = 11;
                p5.Range.Font.Bold = 0;
                p5.Range.InsertParagraphAfter();

                Microsoft.Office.Interop.Word.Paragraph p6 = wordDoc.Content.Paragraphs.Add();
                p6.Range.Text = $"Телефон: {phone}";
                p6.Range.Font.Name = "Arial";
                p6.Range.Font.Size = 11;
                p6.Range.Font.Bold = 0;
                p6.Range.InsertParagraphAfter();

                Microsoft.Office.Interop.Word.Paragraph p7 = wordDoc.Content.Paragraphs.Add();
                p7.Range.Text = $"Email: {email}";
                p7.Range.Font.Name = "Arial";
                p7.Range.Font.Size = 11;
                p7.Range.Font.Bold = 0;
                p7.Range.InsertParagraphAfter();

                Microsoft.Office.Interop.Word.Paragraph p8 = wordDoc.Content.Paragraphs.Add();
                p8.Range.Text = $"Продавец: {seller}";
                p8.Range.Font.Name = "Arial";
                p8.Range.Font.Size = 11;
                p8.Range.Font.Bold = 0;
                p8.Range.InsertParagraphAfter();

                wordDoc.Content.Paragraphs.Add().Range.InsertParagraphAfter();

                // === 4. ТОВАРЫ ===
                Microsoft.Office.Interop.Word.Paragraph p9 = wordDoc.Content.Paragraphs.Add();
                p9.Range.Text = "СОСТАВ ЗАКАЗА:";
                p9.Range.Font.Name = "Arial";
                p9.Range.Font.Size = 12;
                p9.Range.Font.Bold = 1;
                p9.Range.InsertParagraphAfter();

                if (items.Count > 0)
                {
                    foreach (string item in items)
                    {
                        string[] cols = item.Split('|');
                        string title = cols[0];
                        if (title.Length > 35) title = title.Substring(0, 32) + "...";

                        Microsoft.Office.Interop.Word.Paragraph pItem = wordDoc.Content.Paragraphs.Add();
                        pItem.Range.Text = $"{title} - {cols[1]} шт × {Convert.ToDecimal(cols[2]):C} = {Convert.ToDecimal(cols[3]):C}";
                        pItem.Range.Font.Name = "Arial";
                        pItem.Range.Font.Size = 11;
                        pItem.Range.Font.Bold = 0;
                        pItem.Range.InsertParagraphAfter();
                    }
                }
                else
                {
                    Microsoft.Office.Interop.Word.Paragraph pEmpty = wordDoc.Content.Paragraphs.Add();
                    pEmpty.Range.Text = "Нет товаров в заказе";
                    pEmpty.Range.Font.Name = "Arial";
                    pEmpty.Range.Font.Size = 11;
                    pEmpty.Range.Font.Bold = 0;
                    pEmpty.Range.InsertParagraphAfter();
                }

                wordDoc.Content.Paragraphs.Add().Range.InsertParagraphAfter();

                // === 5. ИТОГО ===
                Microsoft.Office.Interop.Word.Paragraph p10 = wordDoc.Content.Paragraphs.Add();
                p10.Range.Text = $"ИТОГО К ОПЛАТЕ: {total}";
                p10.Range.Font.Name = "Arial";
                p10.Range.Font.Size = 14;
                p10.Range.Font.Bold = 1;
                p10.Range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphRight;
                p10.Range.InsertParagraphAfter();

                // === 6. ПОДПИСЬ ===
                wordDoc.Content.Paragraphs.Add().Range.InsertParagraphAfter();

                Microsoft.Office.Interop.Word.Paragraph p11 = wordDoc.Content.Paragraphs.Add();
                p11.Range.Text = "Спасибо за покупку!";
                p11.Range.Font.Name = "Arial";
                p11.Range.Font.Size = 12;
                p11.Range.Font.Bold = 0;
                p11.Range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphCenter;
                p11.Range.InsertParagraphAfter();

                Microsoft.Office.Interop.Word.Paragraph p12 = wordDoc.Content.Paragraphs.Add();
                p12.Range.Text = $"Чек сформирован: {DateTime.Now:dd.MM.yyyy HH:mm}";
                p12.Range.Font.Name = "Arial";
                p12.Range.Font.Size = 11;
                p12.Range.Font.Bold = 0;
                p12.Range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphCenter;
                p12.Range.InsertParagraphAfter();

                Microsoft.Office.Interop.Word.Paragraph p13 = wordDoc.Content.Paragraphs.Add();
                p13.Range.Text = "_______________ (подпись продавца)";
                p13.Range.Font.Name = "Arial";
                p13.Range.Font.Size = 11;
                p13.Range.Font.Bold = 0;
                p13.Range.ParagraphFormat.Alignment = Microsoft.Office.Interop.Word.WdParagraphAlignment.wdAlignParagraphCenter;

                // === 7. СОХРАНЕНИЕ ===
                SaveFileDialog save = new SaveFileDialog();
                save.Filter = "Word документ|*.docx";
                save.FileName = $"Чек_заказа_{orderNumber}.docx";
                save.Title = "Сохранить чек";

                if (save.ShowDialog() == DialogResult.OK)
                {
                    wordDoc.SaveAs(save.FileName);
                    MessageBox.Show($"✅ Чек сохранён!\n{save.FileName}", "Успех",
                                  MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

                wordDoc.Close();
                wordApp.Quit();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Ошибка: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}