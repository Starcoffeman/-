using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
    /// Логика взаимодействия для ProductListWindow.xaml
    /// </summary>
    public partial class ProductListWindow : Window
    {

        private DBEntities dbContext;
        private bool isAdminMode;
        private ObservableCollection<Products> allProducts;
        private CollectionViewSource productViewSource;

        public bool IsAdminMode
        {
            get => isAdminMode;
            set
            {
                isAdminMode = value;
                OnPropertyChanged(nameof(IsAdminMode));

                // Показываем кнопку добавления только для администратора
                AddProductButton.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        public ProductListWindow(bool adminMode = false)
        {
            InitializeComponent();
            this.DataContext = this;

            dbContext = new DBEntities();
            IsAdminMode = adminMode && App.CurrentUserRoleId == 4; // Только для админа

            LoadProducts();
            LoadProductTypes();
        }

        private void LoadProducts()
        {
            try
            {
                allProducts = new ObservableCollection<Products>(
                    dbContext.Products.Include("ProductTypes").ToList()
                );

                productViewSource = new CollectionViewSource();
                productViewSource.Source = allProducts;
                productViewSource.Filter += ApplyFilters;

                ProductsItemsControl.ItemsSource = productViewSource.View;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки товаров: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadProductTypes()
        {
            try
            {
                var types = dbContext.ProductTypes.ToList();
                types.Insert(0, new ProductTypes { Id = 0, Name = "Все типы продукции" });

                TypeFilterComboBox.ItemsSource = types;
                TypeFilterComboBox.DisplayMemberPath = "Name";
                TypeFilterComboBox.SelectedValuePath = "Id";
                TypeFilterComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки типов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ApplyFilters(object sender, FilterEventArgs e)
        {
            var product = e.Item as Products;
            if (product == null)
            {
                e.Accepted = false;
                return;
            }

            // Фильтр по типу
            if (TypeFilterComboBox.SelectedValue != null &&
                (int)TypeFilterComboBox.SelectedValue != 0)
            {
                if (product.ProductTypeId != (int)TypeFilterComboBox.SelectedValue)
                {
                    e.Accepted = false;
                    return;
                }
            }

            // Поиск
            string searchText = SearchTextBox.Text?.Trim().ToLower();
            if (!string.IsNullOrEmpty(searchText))
            {
                bool matches = (product.Name?.ToLower().Contains(searchText) == true) ||
                              (product.Description?.ToLower().Contains(searchText) == true) ||
                              (product.Composition?.ToLower().Contains(searchText) == true);

                if (!matches)
                {
                    e.Accepted = false;
                    return;
                }
            }

            e.Accepted = true;
        }

        private void ApplySorting()
        {
            if (productViewSource?.View == null) return;

            productViewSource.View.SortDescriptions.Clear();

            if (SortComboBox.SelectedItem is ComboBoxItem selected)
            {
                switch (selected.Content.ToString())
                {
                    case "Цена ↑":
                        productViewSource.View.SortDescriptions.Add(
                            new SortDescription("Price", ListSortDirection.Ascending));
                        break;
                    case "Цена ↓":
                        productViewSource.View.SortDescriptions.Add(
                            new SortDescription("Price", ListSortDirection.Descending));
                        break;
                }
            }
        }

        private void SearchTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            productViewSource?.View.Refresh();
        }

        private void TypeFilterComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            productViewSource?.View.Refresh();
        }

        private void SortComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ApplySorting();
        }

        private void AddToCartButton_Click(object sender, RoutedEventArgs e)
        {
            if (App.CurrentUserId == 0)
            {
                MessageBox.Show("Для добавления товаров в корзину необходимо авторизоваться",
                    "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var button = sender as System.Windows.Controls.Button;
            int productId = (int)button.Tag;

            try
            {
                var existingCartItem = dbContext.CartItems
                    .FirstOrDefault(c => c.UserId == App.CurrentUserId && c.ProductId == productId);

                if (existingCartItem != null)
                {
                    existingCartItem.Quantity++;
                }
                else
                {
                    var cartItem = new CartItems
                    {
                        UserId = App.CurrentUserId,
                        ProductId = productId,
                        Quantity = 1
                    };
                    dbContext.CartItems.Add(cartItem);
                }

                dbContext.SaveChanges();

                MessageBox.Show("Товар добавлен в корзину", "Успешно",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка добавления в корзину: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (!IsAdminMode) return;

            var button = sender as System.Windows.Controls.Button;
            int productId = (int)button.Tag;

            var product = dbContext.Products.FirstOrDefault(p => p.Id == productId);
            if (product != null)
            {
                var editWindow = new ProductEditWindow(product);
                if (editWindow.ShowDialog() == true)
                {
                    RefreshProductList();
                }
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (!IsAdminMode) return;

            var button = sender as System.Windows.Controls.Button;
            int productId = (int)button.Tag;

            var result = MessageBox.Show("Вы уверены, что хотите удалить этот товар?",
                "Подтверждение удаления", MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var product = dbContext.Products.FirstOrDefault(p => p.Id == productId);
                    if (product != null)
                    {
                        // Удаляем изображение
                        if (!string.IsNullOrEmpty(product.ImagePath))
                        {
                            string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, product.ImagePath);
                            if (File.Exists(fullPath))
                            {
                                File.Delete(fullPath);
                            }
                        }

                        dbContext.Products.Remove(product);
                        dbContext.SaveChanges();

                        RefreshProductList();

                        MessageBox.Show("Товар успешно удален", "Успешно",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления: {ex.Message}", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void AddProductButton_Click(object sender, RoutedEventArgs e)
        {
            var editWindow = new ProductEditWindow();
            if (editWindow.ShowDialog() == true)
            {
                RefreshProductList();
            }
        }

        private void RefreshProductList()
        {
            allProducts.Clear();
            var products = dbContext.Products.Include("ProductTypes").ToList();
            foreach (var product in products)
            {
                allProducts.Add(product);
            }
            productViewSource?.View.Refresh();
        }

        private void Image_LoadFailed(object sender, ExceptionRoutedEventArgs e)
        {
            var image = sender as System.Windows.Controls.Image;
            if (image != null)
            {
                image.Source = ImageHelper.LoadImage("Resources/picture.png");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
