using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Drawing;

namespace BookShop
{
    public class DatabaseHelper
    {
        string connStr = "server=127.0.0.1;user=root;password=root;database=dbbook50;";

        // =================== ОБЩИЕ МЕТОДЫ ПРОВЕРКИ ===================

        // Проверка дублирования записи
        public bool CheckDuplicate(string tableName, string fieldName, string value, int? excludeId = null)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();

                    string query;
                    if (excludeId.HasValue)
                    {
                        query = $"SELECT COUNT(*) FROM {tableName} WHERE {fieldName} = @value AND {GetIdField(tableName)} != @excludeId";
                    }
                    else
                    {
                        query = $"SELECT COUNT(*) FROM {tableName} WHERE {fieldName} = @value";
                    }

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@value", value);
                        if (excludeId.HasValue)
                            cmd.Parameters.AddWithValue("@excludeId", excludeId.Value);

                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        return count > 0; // true если дубликат есть
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка проверки дублирования: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private string GetIdField(string tableName)
        {
            switch (tableName.ToLower())
            {
                case "user": return "user_id";
                case "role": return "role_id";
                case "category": return "category_id";
                case "product": return "product_id";
                case "order": return "order_id";
                default: return "id";
            }
        }

        // =================== ДОБАВЛЕНИЕ ДАННЫХ ===================

        // Добавить пользователя
        public bool AddUser(string username, string password, string email,
                           string firstName, string lastName, int roleId)
        {
            try
            {
                // Проверка обязательных полей
                if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
                {
                    MessageBox.Show("Логин и пароль обязательны!", "Внимание",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                // Проверка дублирования логина
                if (CheckDuplicate("user", "username", username))
                {
                    MessageBox.Show("Пользователь с таким логином уже существует!", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                // Проверка дублирования email
                if (!string.IsNullOrEmpty(email) && CheckDuplicate("user", "email", email))
                {
                    MessageBox.Show("Пользователь с таким email уже существует!", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    string query = @"INSERT INTO user (username, password_hash, email, first_name, last_name, role_id) 
                                   VALUES (@username, SHA2(@password, 256), @email, @firstName, @lastName, @roleId)";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        cmd.Parameters.AddWithValue("@password", password);
                        cmd.Parameters.AddWithValue("@email", email ?? "");
                        cmd.Parameters.AddWithValue("@firstName", CapitalizeFirstLetter(firstName ?? ""));
                        cmd.Parameters.AddWithValue("@lastName", CapitalizeFirstLetter(lastName ?? ""));
                        cmd.Parameters.AddWithValue("@roleId", roleId);

                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Пользователь успешно добавлен!", "Успех",
                                          MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении пользователя: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return false;
        }

        // Добавить категорию
        public bool AddCategory(string name, string description)
        {
            try
            {
                if (string.IsNullOrEmpty(name))
                {
                    MessageBox.Show("Название категории обязательно!", "Внимание",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                // Проверка дублирования названия
                if (CheckDuplicate("category", "name", name))
                {
                    MessageBox.Show("Категория с таким названием уже существует!", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    string query = "INSERT INTO category (name, description) VALUES (@name, @description)";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@name", name);
                        cmd.Parameters.AddWithValue("@description", description ?? "");

                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Категория успешно добавлена!", "Успех",
                                          MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении категории: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return false;
        }

        // Добавить книгу (товар)
        public int AddProduct(string title, string author, decimal price, int quantity, int categoryId)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    string query = @"INSERT INTO product (title, author, price, stock_quantity, category_id) 
                           VALUES (@title, @author, @price, @quantity, @categoryId);
                           SELECT LAST_INSERT_ID();";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@title", title);
                        cmd.Parameters.AddWithValue("@author", CapitalizeFirstLetter(author));
                        cmd.Parameters.AddWithValue("@price", price);
                        cmd.Parameters.AddWithValue("@quantity", quantity);
                        cmd.Parameters.AddWithValue("@categoryId", categoryId);

                        int newId = Convert.ToInt32(cmd.ExecuteScalar());
                        return newId; // ВОЗВРАЩАЕМ ID, А НЕ TRUE
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении книги: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 0;
            }
        }


        // Проверка дублирования книги (название + автор)
        private bool CheckBookDuplicate(string title, string author)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    string query = "SELECT COUNT(*) FROM product WHERE title = @title AND author = @author";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@title", title);
                        cmd.Parameters.AddWithValue("@author", author);

                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        return count > 0;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
        }
        public Image GetProductImage(string imageName)
        {
            try
            {
                if (string.IsNullOrEmpty(imageName))
                    imageName = "no_image.jpg";

                string path = Path.Combine(Application.StartupPath, "Images", imageName);

                if (File.Exists(path))
                    return Image.FromFile(path);
                else
                {
                    string noImagePath = Path.Combine(Application.StartupPath, "Images", "no_image.jpg");
                    return File.Exists(noImagePath) ? Image.FromFile(noImagePath) : null;
                }
            }
            catch
            {
                return null;
            }
        }

        public string SaveProductImage(string sourceFilePath, int productId)
        {
            try
            {
                string extension = Path.GetExtension(sourceFilePath);
                string fileName = $"book{productId}{extension}";
                string destPath = Path.Combine(Application.StartupPath, "Images", fileName);
                File.Copy(sourceFilePath, destPath, true);
                return fileName;
            }
            catch
            {
                return null;
            }
        }

        // Добавить заказ
        public bool AddOrder(int userId, string customerEmail, string customerFirstName,
                           string customerLastName, string customerPhone, decimal totalAmount)
        {
            try
            {
                // Проверка обязательных полей
                if (string.IsNullOrEmpty(customerEmail) ||
                    string.IsNullOrEmpty(customerFirstName) ||
                    string.IsNullOrEmpty(customerLastName))
                {
                    MessageBox.Show("Заполните данные клиента!", "Внимание",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                // Проверка email
                if (!IsValidEmail(customerEmail))
                {
                    MessageBox.Show("Введите корректный email!", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    string query = @"INSERT INTO `order` (user_id, customer_email, customer_first_name, 
                                   customer_last_name, customer_phone, total_amount, status) 
                                   VALUES (@userId, @email, @firstName, @lastName, @phone, @total, 'В обработке')";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);
                        cmd.Parameters.AddWithValue("@email", customerEmail);
                        cmd.Parameters.AddWithValue("@firstName", CapitalizeFirstLetter(customerFirstName));
                        cmd.Parameters.AddWithValue("@lastName", CapitalizeFirstLetter(customerLastName));
                        cmd.Parameters.AddWithValue("@phone", customerPhone ?? "");
                        cmd.Parameters.AddWithValue("@total", totalAmount);

                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Заказ успешно создан!", "Успех",
                                          MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании заказа: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return false;
        }
        // =================== МЕТОДЫ ОБНОВЛЕНИЯ ДАННЫХ ===================

        // Обновление пользователя
        public bool UpdateUser(int userId, string username, string email,
                              string firstName, string lastName, int roleId)
        {
            try
            {
                // Проверка обязательных полей
                if (string.IsNullOrEmpty(username) ||
                    string.IsNullOrEmpty(firstName) ||
                    string.IsNullOrEmpty(lastName))
                {
                    MessageBox.Show("Заполните все обязательные поля!", "Внимание",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                // Проверка дублирования логина (исключая текущего пользователя)
                if (CheckDuplicate("user", "username", username, userId))
                {
                    MessageBox.Show("Пользователь с таким логином уже существует!", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                // Проверка дублирования email (исключая текущего пользователя)
                if (!string.IsNullOrEmpty(email) && CheckDuplicate("user", "email", email, userId))
                {
                    MessageBox.Show("Пользователь с таким email уже существует!", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    string query = @"UPDATE user SET 
                           username = @username,
                           email = @email,
                           first_name = @firstName,
                           last_name = @lastName,
                           role_id = @roleId
                           WHERE user_id = @userId";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@username", username);
                        cmd.Parameters.AddWithValue("@email", email ?? "");
                        cmd.Parameters.AddWithValue("@firstName", CapitalizeFirstLetter(firstName));
                        cmd.Parameters.AddWithValue("@lastName", CapitalizeFirstLetter(lastName));
                        cmd.Parameters.AddWithValue("@roleId", roleId);
                        cmd.Parameters.AddWithValue("@userId", userId);

                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Данные пользователя успешно обновлены!", "Успех",
                                          MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении пользователя: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return false;
        }

        // Обновление категории
        public bool UpdateCategory(int categoryId, string name, string description)
        {
            try
            {
                if (string.IsNullOrEmpty(name))
                {
                    MessageBox.Show("Введите название категории!", "Внимание",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                // Проверка дублирования названия (исключая текущую категорию)
                if (CheckDuplicate("category", "name", name, categoryId))
                {
                    MessageBox.Show("Категория с таким названием уже существует!", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    string query = @"UPDATE category SET 
                           name = @name,
                           description = @description
                           WHERE category_id = @categoryId";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@name", name);
                        cmd.Parameters.AddWithValue("@description", description ?? "");
                        cmd.Parameters.AddWithValue("@categoryId", categoryId);

                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Категория успешно обновлена!", "Успех",
                                          MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении категории: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return false;
        }

        // Обновление книги (товара)
        public bool UpdateProduct(int productId, string title, string author, decimal price,
                                 int quantity, int categoryId)
        {
            try
            {
                // Проверка обязательных полей
                if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(author) || price <= 0)
                {
                    MessageBox.Show("Заполните все обязательные поля!", "Внимание",
                                  MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return false;
                }

                // Проверка дублирования книги (название + автор)
                if (CheckProductDuplicate(title, author, productId))
                {
                    MessageBox.Show("Такая книга уже существует в базе!", "Ошибка",
                                  MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return false;
                }

                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    string query = @"UPDATE product SET 
                           title = @title,
                           author = @author,
                           price = @price,
                           stock_quantity = @quantity,
                           category_id = @categoryId
                           WHERE product_id = @productId";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@title", title);
                        cmd.Parameters.AddWithValue("@author", CapitalizeFirstLetter(author));
                        cmd.Parameters.AddWithValue("@price", price);
                        cmd.Parameters.AddWithValue("@quantity", quantity);
                        cmd.Parameters.AddWithValue("@categoryId", categoryId);
                        cmd.Parameters.AddWithValue("@productId", productId);

                        int result = cmd.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Книга успешно обновлена!", "Успех",
                                          MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении книги: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return false;
        }

        // Проверка дублирования книги при редактировании
        private bool CheckProductDuplicate(string title, string author, int excludeId)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    string query = "SELECT COUNT(*) FROM product WHERE title = @title AND author = @author AND product_id != @excludeId";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@title", title);
                        cmd.Parameters.AddWithValue("@author", author);
                        cmd.Parameters.AddWithValue("@excludeId", excludeId);

                        int count = Convert.ToInt32(cmd.ExecuteScalar());
                        return count > 0;
                    }
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Получить данные пользователя по ID
        public User GetUserById(int userId)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    string query = @"SELECT user_id, username, email, first_name, last_name, role_id 
                           FROM user WHERE user_id = @userId";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@userId", userId);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new User
                                {
                                    UserId = Convert.ToInt32(reader["user_id"]),
                                    Username = reader["username"].ToString(),
                                    Email = reader["email"].ToString(),
                                    FirstName = reader["first_name"].ToString(),
                                    LastName = reader["last_name"].ToString(),
                                    RoleId = Convert.ToInt32(reader["role_id"])
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке пользователя: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return null;
        }

        // Получить данные категории по ID
        public Category GetCategoryById(int categoryId)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    string query = "SELECT category_id, name, description FROM category WHERE category_id = @categoryId";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@categoryId", categoryId);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new Category
                                {
                                    CategoryId = Convert.ToInt32(reader["category_id"]),
                                    Name = reader["name"].ToString(),
                                    Description = reader["description"].ToString()
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке категории: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return null;
        }

        // Получить данные книги по ID
        public Product GetProductById(int productId)
        {
            try
            {
                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    string query = @"SELECT product_id, title, author, price, stock_quantity, category_id, image_path 
                 FROM product WHERE product_id = @productId";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@productId", productId);

                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new Product
                                {
                                    ProductId = Convert.ToInt32(reader["product_id"]),
                                    Title = reader["title"].ToString(),
                                    Author = reader["author"].ToString(),
                                    Price = Convert.ToDecimal(reader["price"]),
                                    StockQuantity = Convert.ToInt32(reader["stock_quantity"]),
                                    CategoryId = Convert.ToInt32(reader["category_id"]),
                                    ImagePath = reader["image_path"]?.ToString()
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке книги: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return null;
        }

        // =================== ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ===================

        // Заглавная буква в начале (требование ТЗ)
        private string CapitalizeFirstLetter(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            text = text.Trim();
            if (text.Length == 0)
                return text;

            // Первая буква - заглавная, остальные - маленькие
            return char.ToUpper(text[0]) + (text.Length > 1 ? text.Substring(1).ToLower() : "");
        }

        // Проверка email
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        // Получить все роли
        public List<Role> GetRoles()
        {
            List<Role> roles = new List<Role>();

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    string query = "SELECT role_id, role_name FROM role";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                roles.Add(new Role
                                {
                                    RoleId = Convert.ToInt32(reader["role_id"]),
                                    RoleName = reader["role_name"].ToString()
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке ролей: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return roles;
        }

        // Получить все категории
        public List<Category> GetCategories()
        {
            List<Category> categories = new List<Category>();

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    string query = "SELECT category_id, name FROM category";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                categories.Add(new Category
                                {
                                    CategoryId = Convert.ToInt32(reader["category_id"]),
                                    Name = reader["name"].ToString()
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке категорий: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return categories;
        }

        // Получить всех пользователей
        public List<User> GetUsers()
        {
            List<User> users = new List<User>();

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    string query = "SELECT user_id, username, first_name, last_name FROM user";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                users.Add(new User
                                {
                                    UserId = Convert.ToInt32(reader["user_id"]),
                                    Username = reader["username"].ToString(),
                                    FirstName = reader["first_name"].ToString(),
                                    LastName = reader["last_name"].ToString()
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке пользователей: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return users;
        }
        public List<Product> GetProductsForSeller()
        {
            List<Product> products = new List<Product>();

            try
            {
                using (MySqlConnection conn = new MySqlConnection(connStr))
                {
                    conn.Open();
                    string query = @"SELECT p.product_id, p.title, p.author, p.price, 
                           p.stock_quantity, p.category_id, c.name as category_name
                           FROM product p
                           LEFT JOIN category c ON p.category_id = c.category_id
                           ORDER BY p.title";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        using (MySqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                products.Add(new Product
                                {
                                    ProductId = Convert.ToInt32(reader["product_id"]),
                                    Title = reader["title"].ToString(),
                                    Author = reader["author"].ToString(),
                                    Price = Convert.ToDecimal(reader["price"]),
                                    StockQuantity = Convert.ToInt32(reader["stock_quantity"]),
                                    CategoryId = Convert.ToInt32(reader["category_id"]),
                                    CategoryName = reader["category_name"].ToString()
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки книг: {ex.Message}", "Ошибка",
                              MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return products;
        }

        // Получить статусы заказов
        public List<string> GetOrderStatuses()
        {
            return new List<string>
    {
        "В обработке",
        "Оплачен",
        "Доставлен",
        "Отменен"
    };
        }

    }


    // Классы для данных
    public class Role
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; }
    }

    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public int RoleId { get; set; }

    }

    public class Product
    {
        public int ProductId { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public int CategoryId { get; set; }

        public string CategoryName { get; set; }

        public string ImagePath { get; set; }
    }
    public class Category
    {
        public int CategoryId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
