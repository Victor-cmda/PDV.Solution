using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI;
using System.Collections.Generic;
using Microsoft.UI.Xaml.Shapes;
using WinRT.Interop;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView.WinUI;

namespace PDV.UI.WinUI3.Views
{
    public sealed partial class ReportsPage : Page
    {
        public ReportsPage()
        {
            this.InitializeComponent();
            LoadData();
        }

        private void LoadData()
        {
            // Dados de exemplo para Vendas Recentes
            RecentSalesGrid.ItemsSource = new ObservableCollection<RecentSale>
            {
                new RecentSale { Date = "05/12/2024", Product = "Produto A", Value = "R$ 150,00" },
                new RecentSale { Date = "05/12/2024", Product = "Produto B", Value = "R$ 75,00" },
                new RecentSale { Date = "04/12/2024", Product = "Produto C", Value = "R$ 200,00" },
                new RecentSale { Date = "04/12/2024", Product = "Produto D", Value = "R$ 120,00" },
                new RecentSale { Date = "03/12/2024", Product = "Produto E", Value = "R$ 180,00" }
            };

            // Dados de exemplo para Produtos Mais Vendidos
            TopProductsGrid.ItemsSource = new ObservableCollection<TopProduct>
            {
                new TopProduct { Product = "Produto A", Quantity = "150", Total = "R$ 7.500,00" },
                new TopProduct { Product = "Produto B", Quantity = "120", Total = "R$ 6.000,00" },
                new TopProduct { Product = "Produto C", Quantity = "100", Total = "R$ 5.000,00" },
                new TopProduct { Product = "Produto D", Quantity = "80", Total = "R$ 4.000,00" },
                new TopProduct { Product = "Produto E", Quantity = "60", Total = "R$ 3.000,00" }
            };

            // Dados de exemplo para Categorias
            CategoriesGrid.ItemsSource = new ObservableCollection<CategorySale>
            {
                new CategorySale { Category = "Eletrônicos", Total = "R$ 12.500,00", Percentage = "35%" },
                new CategorySale { Category = "Roupas", Total = "R$ 8.900,00", Percentage = "25%" },
                new CategorySale { Category = "Alimentos", Total = "R$ 7.100,00", Percentage = "20%" },
                new CategorySale { Category = "Acessórios", Total = "R$ 5.300,00", Percentage = "15%" },
                new CategorySale { Category = "Outros", Total = "R$ 1.780,00", Percentage = "5%" }
            };

            // Dados de exemplo para Vendedores
            SellersGrid.ItemsSource = new ObservableCollection<SellerPerformance>
            {
                new SellerPerformance { Seller = "João Silva", Sales = "145", Total = "R$ 8.500,00" },
                new SellerPerformance { Seller = "Maria Santos", Sales = "132", Total = "R$ 7.800,00" },
                new SellerPerformance { Seller = "Pedro Oliveira", Sales = "128", Total = "R$ 7.200,00" },
                new SellerPerformance { Seller = "Ana Costa", Sales = "120", Total = "R$ 6.900,00" },
                new SellerPerformance { Seller = "Carlos Souza", Sales = "115", Total = "R$ 6.500,00" }
            };
        }
    }

    // Classes de modelo para os dados
    public class RecentSale
    {
        public string Date { get; set; }
        public string Product { get; set; }
        public string Value { get; set; }
    }

    public class TopProduct
    {
        public string Product { get; set; }
        public string Quantity { get; set; }
        public string Total { get; set; }
    }

    public class CategorySale
    {
        public string Category { get; set; }
        public string Total { get; set; }
        public string Percentage { get; set; }
    }

    public class SellerPerformance
    {
        public string Seller { get; set; }
        public string Sales { get; set; }
        public string Total { get; set; }
    }
}