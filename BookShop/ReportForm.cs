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
    public partial class ReportForm : Form
    {
        private string connStr = "server=127.0.0.1;user=root;password=root;database=dbbook50;";
        public ReportForm()
        {
            InitializeComponent();
        }

        private void ReportForm_Load(object sender, EventArgs e)
        {
            // По умолчанию - начало месяца и сегодня
            dtpStart.Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            dtpEnd.Value = DateTime.Today;

            // Настройка таблицы
            dgvReport.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvReport.ReadOnly = true;
            dgvReport.RowHeadersVisible = false;
            
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            DateTime startDate = dtpStart.Value.Date;
            DateTime endDate = dtpEnd.Value.Date.AddDays(1).AddSeconds(-1);

            if (startDate > dtpEnd.Value.Date)
            {
                MessageBox.Show("Дата начала не может быть позже даты окончания!",
                              "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();

                    string sql = @"
                        SELECT 
                            DATE(o.order_date) AS 'Дата',
                            COUNT(*) AS 'Кол-во заказов',
                            SUM(o.total_amount) AS 'Выручка',
                            AVG(o.total_amount) AS 'Средний чек'
                        FROM `order` o
                        WHERE o.order_date BETWEEN @start AND @end
                        GROUP BY DATE(o.order_date)
                        ORDER BY DATE(o.order_date) DESC";

                    using (MySqlCommand cmd = new MySqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@start", startDate);
                        cmd.Parameters.AddWithValue("@end", endDate);

                        MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        // Форматирование чисел
                        if (dt.Columns.Contains("Средний чек"))
                        {
                            dt.Columns["Средний чек"].ColumnName = "Средний чек (₽)";
                        }

                        // Форматирование
                        if (dt.Columns.Contains("Выручка"))
                            dt.Columns["Выручка"].ColumnName = "Выручка (₽)";
                        if (dt.Columns.Contains("Средний чек"))
                            dt.Columns["Средний чек"].ColumnName = "Средний чек (₽)";

                        dgvReport.DataSource = dt;

                        // Подсчет итогов
                        decimal totalRevenue = 0;
                        foreach (DataRow row in dt.Rows)
                            totalRevenue += Convert.ToDecimal(row["Выручка (₽)"]);

                        lblTotal.Text = $"ИТОГО: {totalRevenue:C}";
                        lblTotal.ForeColor = totalRevenue > 0 ? Color.Green : Color.Gray;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка формирования отчета: {ex.Message}",
                              "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnExcel_Click(object sender, EventArgs e)
        {
            if (dgvReport.Rows.Count == 0)
            {
                MessageBox.Show("Сначала сформируйте отчет!", "Внимание",
                              MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SaveFileDialog save = new SaveFileDialog();
            save.Filter = "CSV файл|*.csv";
            save.FileName = $"Отчет_продаж_{dtpStart.Value:dd-MM-yyyy}_по_{dtpEnd.Value:dd-MM-yyyy}.csv";

            if (save.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (System.IO.StreamWriter writer = new System.IO.StreamWriter(save.FileName, false, System.Text.Encoding.UTF8))
                    {
                        // Заголовки
                        for (int i = 0; i < dgvReport.Columns.Count; i++)
                        {
                            writer.Write(dgvReport.Columns[i].HeaderText);
                            if (i < dgvReport.Columns.Count - 1)
                                writer.Write(";");
                        }
                        writer.WriteLine();

                        // Данные
                        foreach (DataGridViewRow row in dgvReport.Rows)
                        {
                            for (int i = 0; i < dgvReport.Columns.Count; i++)
                            {
                                writer.Write(row.Cells[i].Value?.ToString());
                                if (i < dgvReport.Columns.Count - 1)
                                    writer.Write(";");
                            }
                            writer.WriteLine();
                        }

                        // Итог
                        writer.WriteLine();
                        writer.WriteLine($"ИТОГО;{lblTotal.Text}");
                    }

                    MessageBox.Show($"Отчет сохранен как CSV!\n{save.FileName}\n\nОткрыть в Excel?",
                                  "Успех", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($" Ошибка: {ex.Message}", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }
}

