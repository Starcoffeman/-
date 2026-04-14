using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private DBEntities dbContext;


        public LoginWindow()
        {
            InitializeComponent();
            dbContext = new DBEntities();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string login = LoginTextBox.Text.Trim();
            string password = PasswordBox.Password;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                ShowError("Пожалуйста, введите логин и пароль");
                return;
            }

            try
            {
                // Поиск пользователя по логину
                var user = dbContext.Users.FirstOrDefault(u => u.Login == login);

                if (user == null)
                {
                    ShowError("Неверный логин или пароль");
                    return;
                }

                // Проверка пароля (в реальном приложении используйте хеширование)
                // Временно используем прямое сравнение
                if (VerifyPassword(password, user.PasswordHash))
                {
                    // Сохраняем данные пользователя
                    App.CurrentUserId = user.Id;
                    App.CurrentUserFullName = user.FullName;
                    App.CurrentUserRoleId = user.RoleId;
                    App.CurrentUserRole = user.Roles.Name;

                    this.DialogResult = true;
                    this.Close();
                }
                else
                {
                    ShowError("Неверный логин или пароль");
                }
            }
            catch (Exception ex)
            {
                ShowError($"Ошибка при входе: {ex.Message}");
            }
        }

        private bool VerifyPassword(string inputPassword, string storedHash)
        {
            // В реальном приложении используйте BCrypt или другой алгоритм
            // Временно просто сравниваем
            return inputPassword == "password123"; // Временное решение для тестирования

            // Правильная реализация с хешированием:
            // using (SHA256 sha256 = SHA256.Create())
            // {
            //     byte[] hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(inputPassword));
            //     string hash = Convert.ToBase64String(hashedBytes);
            //     return hash == storedHash;
            // }
        }

        private void ShowError(string message)
        {
            ErrorText.Text = message;
            ErrorText.Visibility = Visibility.Visible;
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}
