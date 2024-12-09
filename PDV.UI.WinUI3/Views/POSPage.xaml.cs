using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace PDV.UI.WinUI3.Views
{
    public sealed partial class POSPage : Page
    {
        private ObservableCollection<SaleItem> SaleItems { get; set; }
        private decimal TotalAmount { get; set; }
        private int ItemCounter { get; set; }

        public POSPage()
        {
            this.InitializeComponent();
            InitializeSale();
            StartClock();
        }

        private void InitializeSale()
        {
            SaleItems = new ObservableCollection<SaleItem>();
            ProductsGrid.ItemsSource = SaleItems;
            UpdateTotals();
            BarcodeInput.Focus(FocusState.Programmatic);
        }

        private void StartClock()
        {
            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += (s, e) =>
            {
                CurrentDateTime.Text = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            };
            timer.Start();
        }

        private void BarcodeInput_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                ProcessBarcode(BarcodeInput.Text);
                BarcodeInput.Text = string.Empty;
            }
        }

        private void ProcessBarcode(string barcode)
        {
            // Simulando busca de produto
            // Em um ambiente real, isso viria do banco de dados
            var product = new SaleItem
            {
                Code = barcode,
                Description = $"Produto {++ItemCounter}",
                Quantity = 1,
                UnitPrice = Random.Shared.Next(1, 1000) / 10.0m,
            };

            product.Total = product.UnitPrice * product.Quantity;

            SaleItems.Add(product);
            UpdateTotals();
        }

        private void RemoveItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is SaleItem item)
            {
                SaleItems.Remove(item);
                UpdateTotals();
            }
        }

        private void UpdateTotals()
        {
            TotalAmount = SaleItems.Sum(item => item.Total);

            ItemCount.Text = SaleItems.Count.ToString();
            Subtotal.Text = $"R$ {TotalAmount:N2}";
            Total.Text = $"R$ {TotalAmount:N2}";
        }
    }

    public class SaleItem
    {
        public string Code { get; set; }
        public string Description { get; set; }
        public decimal Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Total { get; set; }
    }
}
