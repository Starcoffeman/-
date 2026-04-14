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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            UpdateUIBasedOnRole();

        }

        private void UpdateUIBasedOnRole()
        {
            UserInfoText.Text = App.CurrentUserFullName;

            if (App.CurrentUserRoleId == 1) // Гость
            {
                AuthButton.Visibility = Visibility.Visible;
                LogoutButton.Visibility = Visibility.Collapsed;
                CartButton.Visibility = Visibility.Collapsed;
                OrdersButton.Visibility = Visibility.Collapsed;
                AdminProductsButton.Visibility = Visibility.Collapsed;
            }
            else // Авторизованный пользователь
            {
                AuthButton.Visibility = Visibility.Collapsed;
                LogoutButton.Visibility = Visibility.Visible;
                CartButton.Visibility = Visibility.Visible;

                if (App.CurrentUserRoleId == 2) // Обычный пользователь
                {
                    OrdersButton.Visibility = Visibility.Collapsed;
                    AdminProductsButton.Visibility = Visibility.Collapsed;
                }
                else if (App.CurrentUserRoleId == 3) // Менеджер
                {
                    OrdersButton.Visibility = Visibility.Visible;
                    AdminProductsButton.Visibility = Visibility.Collapsed;
                }
                else if (App.CurrentUserRoleId == 4) // Администратор
                {
                    OrdersButton.Visibility = Visibility.Visible;
                    AdminProductsButton.Visibility = Visibility.Visible;
                }
            }
        }

        private void AuthButton_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow();
            loginWindow.ShowDialog();
            UpdateUIBasedOnRole();
        }

        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            // Сброс данных пользователя
            App.CurrentUserId = 0;
            App.CurrentUserFullName = "Гость";
            App.CurrentUserRoleId = 1;
            App.CurrentUserRole = "Guest";

            UpdateUIBasedOnRole();

            MessageBox.Show("Вы успешно вышли из системы", "Информация",
                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ProductsButton_Click(object sender, RoutedEventArgs e)
        {
            var productListWindow = new ProductListWindow();
            productListWindow.ShowDialog();
        }

        private void CartButton_Click(object sender, RoutedEventArgs e)
        {
            var cartWindow = new CartWindow();
            cartWindow.ShowDialog();
        }

        private void OrdersButton_Click(object sender, RoutedEventArgs e)
        {
            var ordersWindow = new OrdersWindow();
            ordersWindow.ShowDialog();
        }

        private void AdminProductsButton_Click(object sender, RoutedEventArgs e)
        {
            var productListWindow = new ProductListWindow(true); // Режим администратора
            productListWindow.ShowDialog();
        }
    }
}
