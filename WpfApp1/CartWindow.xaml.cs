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
    /// Логика взаимодействия для CartWindow.xaml
    /// </summary>
    public partial class CartWindow : Window
    {
        private DBEntities dbContext;
        private List<CartItems> cartItems;

        public CartWindow()
        {
            InitializeComponent();
            dbContext = new DBEntities();
            LoadCart();
        }

        private void LoadCart()
        {
            try
            {
                cartItems = dbContext.CartItems
                    .Include("Products")
                    .Include("Products.ProductTypes")
                    .Where(c => c.UserId == App.CurrentUserId)
                    .ToList();

                CartItemsControl.ItemsSource = cartItems;
                UpdateTotal();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки корзины: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UpdateTotal()
        {
            decimal total = cartItems.Sum(item => item.Products.Price * item.Quantity);
            TotalAmountText.Text = total.ToString("C");
        }

        private void IncreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button;
            int cartItemId = (int)button.Tag;

            var cartItem = cartItems.FirstOrDefault(c => c.Id == cartItemId);
            if (cartItem != null)
            {
                cartItem.Quantity++;
                dbContext.SaveChanges();
                LoadCart(); 
            }
        }

        private void DecreaseQuantity_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button;
            int cartItemId = (int)button.Tag;

            var cartItem = cartItems.FirstOrDefault(c => c.Id == cartItemId);
            if (cartItem != null)
            {
                if (cartItem.Quantity > 1)
                {
                    cartItem.Quantity--;
                    dbContext.SaveChanges();
                    LoadCart();
                }
                else
                {
                    // Если количество 1, удаляем товар
                    RemoveFromCart(cartItem);
                }
            }
        }

        private void RemoveFromCart_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as System.Windows.Controls.Button;
            int cartItemId = (int)button.Tag;

            var cartItem = cartItems.FirstOrDefault(c => c.Id == cartItemId);
            if (cartItem != null)
            {
                RemoveFromCart(cartItem);
            }
        }

        private void RemoveFromCart(CartItems cartItem)
        {
            var result = MessageBox.Show("Удалить товар из корзины?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                dbContext.CartItems.Remove(cartItem);
                dbContext.SaveChanges();
                LoadCart();
            }
        }

        private void ClearCartButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show("Очистить всю корзину?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                dbContext.CartItems.RemoveRange(cartItems);
                dbContext.SaveChanges();
                LoadCart();
            }
        }

        private void OrderButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(AddressTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, введите адрес доставки", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                AddressTextBox.Focus();
                return;
            }

            if (cartItems.Count == 0)
            {
                MessageBox.Show("Корзина пуста", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // Генерируем номер заказа
                string orderNumber = "ORD-" + DateTime.Now.ToString("yyyyMMddHHmmss");
                decimal totalAmount = cartItems.Sum(item => item.Products.Price * item.Quantity);

                // Создаем заказ
                var order = new Orders
                {
                    OrderNumber = orderNumber,
                    UserId = App.CurrentUserId,
                    OrderStatusId = 1, // Новый
                    DeliveryAddress = AddressTextBox.Text.Trim(),
                    OrderDate = DateTime.Now,
                    DeliveryDate = DateTime.Now.AddDays(3),
                    TotalAmount = totalAmount
                };

                dbContext.Orders.Add(order);
                dbContext.SaveChanges();

                foreach (var cartItem in cartItems)
                {
                    var orderItem = new OrderItems
                    {
                        OrderId = order.Id,
                        ProductId = cartItem.ProductId,
                        Quantity = cartItem.Quantity,
                        PriceAtTime = cartItem.Products.Price,
                        DiscountAtTime = cartItem.Products.Discount ?? 0
                    };
                    dbContext.OrderItems.Add(orderItem);
                }

                // Очищаем корзину
                dbContext.CartItems.RemoveRange(cartItems);
                dbContext.SaveChanges();

                MessageBox.Show($"Заказ #{orderNumber} успешно оформлен!\nСумма: {totalAmount:C}",
                    "Успешно", MessageBoxButton.OK, MessageBoxImage.Information);

                LoadCart(); // Обновляем корзину
                AddressTextBox.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка оформления заказа: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Image_LoadFailed(object sender, ExceptionRoutedEventArgs e)
        {
            var image = sender as System.Windows.Controls.Image;
            if (image != null)
            {
                image.Source = new ImageSourceConverter().ConvertFromString("Resources/picture.png") as ImageSource;
            }
        }
    }
}
