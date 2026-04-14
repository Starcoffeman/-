using System;
using System.Collections.Generic;
using System.IO;
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
using Microsoft.Win32;

namespace WpfApp1
{
    public partial class ProductEditWindow : Window
    {
        private DBEntities dbContext;
        private Products editingProduct;
        private string selectedImagePath = null;
        private string oldImagePath = null;

        public ProductEditWindow(Products product = null)
        {
            InitializeComponent();
            dbContext = new DBEntities();

            if (product != null)
            {
                editingProduct = product;
                TitleText.Text = "Редактирование товара";
                LoadProductData();
            }
            else
            {
                editingProduct = null;
                TitleText.Text = "Добавление товара";
            }

            LoadProductTypes();
        }

        private void LoadProductTypes()
        {
            try
            {
                var types = dbContext.ProductTypes.ToList();
                ProductTypeComboBox.ItemsSource = types;
                ProductTypeComboBox.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки типов: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadProductData()
        {
            if (editingProduct == null) return;

            NameTextBox.Text = editingProduct.Name;
            ProductTypeComboBox.SelectedValue = editingProduct.ProductTypeId;
            PriceTextBox.Text = editingProduct.Price.ToString();
            DiscountTextBox.Text = editingProduct.Discount?.ToString() ?? "0";
            CompositionTextBox.Text = editingProduct.Composition;
            DescriptionTextBox.Text = editingProduct.Description;
            MinStockTextBox.Text = editingProduct.MinStock?.ToString() ?? "0";

            oldImagePath = editingProduct.ImagePath;

            if (!string.IsNullOrEmpty(editingProduct.ImagePath))
            {
                string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, editingProduct.ImagePath);
                if (File.Exists(fullPath))
                {
                    ProductImage.Source = ImageHelper.LoadImage(fullPath);
                }
                else
                {
                    ProductImage.Source = ImageHelper.LoadImage("Resources/picture.png");
                }
            }
            else
            {
                ProductImage.Source = ImageHelper.LoadImage("Resources/picture.png");
            }
        }

        private void SelectImageButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.png;*.jpg;*.jpeg)|*.png;*.jpg;*.jpeg|All files (*.*)|*.*";

            if (openFileDialog.ShowDialog() == true)
            {
                selectedImagePath = openFileDialog.FileName;

                // Изменяем размер изображения до 300x200
                var resizedImage = ImageHelper.ResizeImage(selectedImagePath, 300, 200);
                ProductImage.Source = resizedImage;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            // Валидация
            if (string.IsNullOrWhiteSpace(NameTextBox.Text))
            {
                MessageBox.Show("Пожалуйста, введите наименование товара", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                NameTextBox.Focus();
                return;
            }

            if (ProductTypeComboBox.SelectedItem == null)
            {
                MessageBox.Show("Пожалуйста, выберите тип продукции", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!decimal.TryParse(PriceTextBox.Text, out decimal price) || price < 0)
            {
                MessageBox.Show("Пожалуйста, введите корректную стоимость (неотрицательное число)",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                PriceTextBox.Focus();
                return;
            }

            if (!int.TryParse(DiscountTextBox.Text, out int discount) || discount < 0 || discount > 100)
            {
                MessageBox.Show("Пожалуйста, введите корректную скидку (0-100%)",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                DiscountTextBox.Focus();
                return;
            }

            if (!int.TryParse(MinStockTextBox.Text, out int minStock) || minStock < 0)
            {
                MessageBox.Show("Пожалуйста, введите корректное значение минимального остатка",
                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                MinStockTextBox.Focus();
                return;
            }

            try
            {
                string savedImagePath = null;

                // Сохраняем изображение
                if (!string.IsNullOrEmpty(selectedImagePath))
                {
                    // Удаляем старое изображение
                    if (!string.IsNullOrEmpty(oldImagePath))
                    {
                        string oldFullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, oldImagePath);
                        if (File.Exists(oldFullPath))
                        {
                            File.Delete(oldFullPath);
                        }
                    }

                    // Сохраняем новое изображение
                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(selectedImagePath);
                    string imagesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");

                    if (!Directory.Exists(imagesDir))
                    {
                        Directory.CreateDirectory(imagesDir);
                    }

                    string newPath = Path.Combine(imagesDir, fileName);
                    File.Copy(selectedImagePath, newPath);
                    savedImagePath = $"Images/{fileName}";
                }

                if (editingProduct == null)
                {
                    // Создаем новый товар
                    var newProduct = new Products
                    {
                        Name = NameTextBox.Text.Trim(),
                        ProductTypeId = (int)ProductTypeComboBox.SelectedValue,
                        Price = price,
                        Discount = discount,
                        Composition = CompositionTextBox.Text,
                        Description = DescriptionTextBox.Text,
                        MinStock = minStock,
                        ImagePath = savedImagePath ?? oldImagePath,
                        CreatedAt = DateTime.Now
                    };

                    dbContext.Products.Add(newProduct);
                }
                else
                {
                    // Обновляем существующий
                    editingProduct.Name = NameTextBox.Text.Trim();
                    editingProduct.ProductTypeId = (int)ProductTypeComboBox.SelectedValue;
                    editingProduct.Price = price;
                    editingProduct.Discount = discount;
                    editingProduct.Composition = CompositionTextBox.Text;
                    editingProduct.Description = DescriptionTextBox.Text;
                    editingProduct.MinStock = minStock;

                    if (savedImagePath != null)
                    {
                        editingProduct.ImagePath = savedImagePath;
                    }
                }

                dbContext.SaveChanges();

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
