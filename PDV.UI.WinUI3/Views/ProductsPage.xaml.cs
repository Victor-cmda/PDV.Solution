using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using PDV.Domain.Entities;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PDV.UI.WinUI3.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ProductsPage : Page, INotifyPropertyChanged
    {
        private ObservableCollection<Product> AllProducts;
        private ObservableCollection<Product> FilteredProducts;

        // Adicione estas propriedades
        public bool HasItems => FilteredProducts?.Count > 0;
        public bool HasNoItems => !HasItems;

        // Implemente o evento PropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public ProductsPage()
        {
            this.InitializeComponent();
            LoadMockData();
            FilteredProducts = new ObservableCollection<Product>(AllProducts);
            ProductsList.ItemsSource = FilteredProducts;

            // Adicione um handler para a coleção
            FilteredProducts.CollectionChanged += (s, e) =>
            {
                NotifyPropertyChanged(nameof(HasItems));
                NotifyPropertyChanged(nameof(HasNoItems));
            };
        }

        private void LoadMockData()
        {
            AllProducts = new ObservableCollection<Product>
            {
                new Product
                {
                    Id = 1,
                    Name = "Arroz Integral 1kg",
                    Category = "Alimentos",
                    Price = 8.99m,
                    Barcode = "7891234567890",
                    Stock = 50,
                    Supplier = "Distribuidora ABC",
                    LastUpdate = DateTime.Now.AddDays(-5)
                },
                new Product
                {
                    Id = 2,
                    Name = "Água Mineral 500ml",
                    Category = "Bebidas",
                    Price = 2.50m,
                    Barcode = "7891234567891",
                    Stock = 100,
                    Supplier = "Bebidas XYZ",
                    LastUpdate = DateTime.Now.AddDays(-2)
                },
                new Product
                {
                    Id = 3,
                    Name = "Detergente Líquido 500ml",
                    Category = "Limpeza",
                    Price = 3.99m,
                    Barcode = "7891234567892",
                    Stock = 30,
                    Supplier = "Limpeza & Cia",
                    LastUpdate = DateTime.Now.AddDays(-1)
                }
            };
        }

        private async void AddProduct_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ProductDialog();
            dialog.XamlRoot = this.XamlRoot;

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                var newProduct = dialog.Product;
                newProduct.Id = AllProducts.Max(p => p.Id) + 1;
                AllProducts.Add(newProduct);
                ApplyFilters();
            }
        }

        private async void EditProduct_Click(object sender, RoutedEventArgs e)
        {
            if (ProductsList.SelectedItem is Product selectedProduct)
            {
                var dialog = new ProductDialog(selectedProduct);
                dialog.XamlRoot = this.XamlRoot;

                var result = await dialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    var index = AllProducts.IndexOf(selectedProduct);
                    AllProducts[index] = dialog.Product;
                    ApplyFilters();
                    UpdateProductDetails(dialog.Product);
                }
            }
        }

        private async void DeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            if (ProductsList.SelectedItem is Product product)
            {
                ContentDialog deleteDialog = new ContentDialog
                {
                    Title = "Confirmar exclusão",
                    Content = $"Tem certeza que deseja excluir o produto '{product.Name}'?",
                    PrimaryButtonText = "Sim",
                    CloseButtonText = "Não",
                    DefaultButton = ContentDialogButton.Close,
                    XamlRoot = this.XamlRoot
                };

                var result = await deleteDialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    AllProducts.Remove(product);
                    FilteredProducts.Remove(product);
                    ClearProductDetails();
                }
            }
        }

        private void RefreshList_Click(object sender, RoutedEventArgs e)
        {
            ApplyFilters();
        }

        private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                ApplyFilters();
            }
        }

        private void FilterBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void ApplyFilters()
        {
            var searchText = SearchBox.Text.ToLower();
            var selectedCategory = (FilterBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            var query = AllProducts.AsEnumerable();

            if (!string.IsNullOrEmpty(searchText))
            {
                query = query.Where(p =>
                    p.Name.ToLower().Contains(searchText) ||
                    p.Barcode.Contains(searchText) ||
                    p.Supplier.ToLower().Contains(searchText));
            }

            if (!string.IsNullOrEmpty(selectedCategory) && selectedCategory != "Todas")
            {
                query = query.Where(p => p.Category == selectedCategory);
            }

            FilteredProducts.Clear();
            foreach (var product in query.OrderBy(p => p.Name))
            {
                FilteredProducts.Add(product);
            }
        }

        private void ProductsList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ProductsList.SelectedItem is Product product)
            {
                UpdateProductDetails(product);
            }
            else
            {
                ClearProductDetails();
            }
        }

        private void UpdateProductDetails(Product product)
        {
            NameText.Text = product.Name;
            CategoryText.Text = product.Category;
            PriceText.Text = $"R$ {product.Price:N2}";
            BarcodeText.Text = product.Barcode;
            StockText.Text = product.Stock.ToString();
            SupplierText.Text = product.Supplier;
            LastUpdateText.Text = product.LastUpdate.ToString("dd/MM/yyyy HH:mm");
        }

        private void ClearProductDetails()
        {
            NameText.Text = "-";
            CategoryText.Text = "-";
            PriceText.Text = "-";
            BarcodeText.Text = "-";
            StockText.Text = "-";
            SupplierText.Text = "-";
            LastUpdateText.Text = "-";
        }
    }
}
