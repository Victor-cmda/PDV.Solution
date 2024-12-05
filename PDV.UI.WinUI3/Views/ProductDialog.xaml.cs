using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
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
    public sealed partial class ProductDialog : ContentDialog
    {
        public Product Product { get; private set; }
        private bool IsEditMode;

        public ProductDialog()
        {
            this.InitializeComponent();
        }

        public ProductDialog(Product product) : this()
        {
            IsEditMode = true;
            Product = product;
            Title = "Editar Produto";

            // Preencher campos com dados existentes
            NameBox.Text = product.Name;
            PriceBox.Text = product.Price.ToString("N2");
            BarcodeBox.Text = product.Barcode;
            StockBox.Text = product.Stock.ToString();
            SupplierBox.Text = product.Supplier;

            // Selecionar categoria
            foreach (ComboBoxItem item in CategoryBox.Items)
            {
                if (item.Content.ToString() == product.Category)
                {
                    CategoryBox.SelectedItem = item;
                    break;
                }
            }
        }

        private void ValidateInput(object sender, object e)
        {
            bool isValid = true;
            string errorMessage = "";

            // Validar Nome
            if (string.IsNullOrWhiteSpace(NameBox.Text))
            {
                isValid = false;
                errorMessage = "Nome é obrigatório";
            }
            else if (NameBox.Text.Length < 3)
            {
                isValid = false;
                errorMessage = "Nome deve ter pelo menos 3 caracteres";
            }

            // Validar Categoria
            if (CategoryBox.SelectedItem == null && isValid)
            {
                isValid = false;
                errorMessage = "Categoria é obrigatória";
            }

            // Validar Preço
            if (string.IsNullOrWhiteSpace(PriceBox.Text) && isValid)
            {
                isValid = false;
                errorMessage = "Preço é obrigatório";
            }
            else if (!decimal.TryParse(PriceBox.Text, out decimal price) && isValid)
            {
                isValid = false;
                errorMessage = "Preço inválido";
            }
            else if (price <= 0 && isValid)
            {
                isValid = false;
                errorMessage = "Preço deve ser maior que zero";
            }

            // Validar Código de Barras
            if (string.IsNullOrWhiteSpace(BarcodeBox.Text) && isValid)
            {
                isValid = false;
                errorMessage = "Código de barras é obrigatório";
            }
            else if (!Regex.IsMatch(BarcodeBox.Text, @"^\d{8,13}$") && isValid)
            {
                isValid = false;
                errorMessage = "Código de barras deve ter entre 8 e 13 dígitos";
            }

            // Validar Estoque
            if (string.IsNullOrWhiteSpace(StockBox.Text) && isValid)
            {
                isValid = false;
                errorMessage = "Estoque é obrigatório";
            }
            else if (!int.TryParse(StockBox.Text, out int stock) && isValid)
            {
                isValid = false;
                errorMessage = "Estoque deve ser um número inteiro";
            }
            else if (stock < 0 && isValid)
            {
                isValid = false;
                errorMessage = "Estoque não pode ser negativo";
            }

            // Validar Fornecedor
            if (string.IsNullOrWhiteSpace(SupplierBox.Text) && isValid)
            {
                isValid = false;
                errorMessage = "Fornecedor é obrigatório";
            }
            else if (SupplierBox.Text.Length < 3 && isValid)
            {
                isValid = false;
                errorMessage = "Fornecedor deve ter pelo menos 3 caracteres";
            }

            // Atualizar UI
            ValidationInfoBar.Message = errorMessage;
            ValidationInfoBar.IsOpen = !isValid;
            IsPrimaryButtonEnabled = isValid;
        }

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            Product = new Product
            {
                Name = NameBox.Text,
                Category = (CategoryBox.SelectedItem as ComboBoxItem)?.Content.ToString(),
                Price = decimal.Parse(PriceBox.Text),
                Barcode = BarcodeBox.Text,
                Stock = int.Parse(StockBox.Text),
                Supplier = SupplierBox.Text,
                LastUpdate = DateTime.Now
            };

            if (IsEditMode)
            {
                Product.Id = this.Product.Id;
            }

        }
    }
}
