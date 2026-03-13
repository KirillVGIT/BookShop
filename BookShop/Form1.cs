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
using System.IO;

namespace BookShop
{
    public partial class LoginForm : Form
    // server=10.207.106.12; user=user83;password=qp96;database=db83;
    {
        // строка подкл
        string connStr = "server=127.0.0.1;user=root;password=;database=dbbook83;";
        private int failedAttempts = 0;
        private bool captchaRequired = false;
        private int currentCaptchaIndex = 0;
        private string[] captchaValues = { "kv7p", "vr5y", "dr3t" };
        private string[] captchaFiles = { "captcha1.png", "captcha2.png", "captcha3.png" };
        private Timer lockTimer = new Timer();
        private int lockSeconds = 0;
        public LoginForm()
        {
            InitializeComponent();
            SetupCaptchaControls();
            lockTimer.Interval = 1000;
            lockTimer.Tick += LockTimer_Tick;
        }

        private void SetupCaptchaControls()
        {
            // По умолчанию капча скрыта
            pictureBoxCaptcha.Visible = false;
            txtCaptcha.Visible = false;
            btnRefreshCaptcha.Visible = false;
            lblCaptcha.Visible = false;
        }

        private void ShowCaptcha()
        {
            pictureBoxCaptcha.Visible = true;
            txtCaptcha.Visible = true;
            btnRefreshCaptcha.Visible = true;
            lblCaptcha.Visible = true;
            LoadCaptchaImage(0);
        }

        private void LoadCaptchaImage(int index)
        {
            try
            {
                string path = Path.Combine(Application.StartupPath, "Images", captchaFiles[index]);
                if (File.Exists(path))
                {
                    pictureBoxCaptcha.Image = Image.FromFile(path);
                    currentCaptchaIndex = index;
                }
                else
                {
                    MessageBox.Show($"Файл {captchaFiles[index]} не найден в папке Images", "Ошибка");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки капчи: " + ex.Message);
            }
        }

        private void LockTimer_Tick(object sender, EventArgs e)
        {
            lockSeconds--;
            btnLogin.Enabled = false;
            btnRefreshCaptcha.Enabled = false;

            if (lockSeconds <= 0)
            {
                lockTimer.Stop();
                btnLogin.Enabled = true;
                btnRefreshCaptcha.Enabled = true;
                failedAttempts = 0;
                captchaRequired = false;
                SetupCaptchaControls();
                MessageBox.Show("Блокировка снята. Можете повторить попытку.", "Информация");
            }
        }

        private void LoginForm_Load(object sender, EventArgs e)
        {

        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            // ЕСЛИ КАПЧА АКТИВНА – ПРОВЕРЯЕМ ЕЁ В ПЕРВУЮ ОЧЕРЕДЬ
            if (captchaRequired)
            {
                // Проверяем капчу
                if (txtCaptcha.Text.Trim().ToLower() != captchaValues[currentCaptchaIndex])
                {
                    // НЕВЕРНАЯ КАПЧА → БЛОКИРОВКА 10 СЕКУНД
                    MessageBox.Show("Неверная капча! Вход заблокирован на 10 секунд.", "Блокировка");

                    // Скрываем капчу и блокируем кнопки
                    SetupCaptchaControls();
                    btnLogin.Enabled = false;
                    btnRefreshCaptcha.Enabled = false;

                    // Запускаем таймер на 10 секунд
                    lockSeconds = 10;
                    lockTimer.Start();

                    return; // Выходим, дальше не идём
                }
                // Если капча верна — продолжаем проверку логина/пароля
            }

            // Проверка логина и пароля
            if (username == "admin1" && password == "admin123")
            {
                // Успешный вход
                failedAttempts = 0;
                captchaRequired = false;
                SetupCaptchaControls();
                AdminForm adminForm = new AdminForm();
                adminForm.Show();
                this.Hide();
            }
            else if (username == "seller1" && password == "sell123")
            {
                failedAttempts = 0;
                captchaRequired = false;
                SetupCaptchaControls();
                ProdavecForm prodavecForm = new ProdavecForm();
                prodavecForm.Show();
                this.Hide();
            }
            else if (username == "store1" && password == "store123")
            {
                failedAttempts = 0;
                captchaRequired = false;
                SetupCaptchaControls();
                KladovshikForm kladovshikForm = new KladovshikForm();
                kladovshikForm.Show();
                this.Hide();
            }
            else
            {
                // Неверный логин/пароль
                MessageBox.Show("Неверный логин или пароль");

                if (!captchaRequired)
                {
                    // Если капча ещё не показывалась – включаем
                    captchaRequired = true;
                    ShowCaptcha();
                }
                else
                {
                    // Если капча уже была – обновляем на всякий случай
                    int nextIndex = (currentCaptchaIndex + 1) % captchaFiles.Length;
                    LoadCaptchaImage(nextIndex);
                    txtCaptcha.Text = "";
                }
            }
        }

        private void btnRefreshCaptcha_Click(object sender, EventArgs e)
        {
            // Переключаем на следующую картинку
            int nextIndex = (currentCaptchaIndex + 1) % captchaFiles.Length;
            LoadCaptchaImage(nextIndex);
            txtCaptcha.Text = "";
        }
    }
}




