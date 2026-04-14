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

    public partial class OrdersWindow : Window
    {
        private DBEntities dbContext;

        public OrdersWindow()
        {
            InitializeComponent();
            dbContext = new DBEntities();
            LoadOrders();
        }

        private void LoadOrders()
        {
            try
            {
                var orders = dbContext.Orders
                    .Include("Users")
                    .Include("OrderStatuses")
                    .OrderByDescending(o => o.OrderDate)
                    .ToList();

                OrdersDataGrid.ItemsSource = orders;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки заказов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
