using Microsoft.UI.Xaml;
using System;
using Microsoft.UI.Xaml.Controls;
using PDV.Domain.Entities;
using PDV.UI.WinUI3.ViewModels;
using PDV.Infrastructure.Data.Contexts;
using Microsoft.Extensions.DependencyInjection;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PDV.UI.WinUI3.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ProductPage : Page
    {
        public ProductViewModel ViewModel { get; }

        public ProductPage(ProductViewModel viewModel)
        {
            this.InitializeComponent();
            ViewModel = App.GetService<ProductViewModel>();
        }

        private void NewProduct_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.SelectedProduct = new Product();
            ViewModel.IsEditing = true;
        }

        private void EditProduct_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedProduct != null)
            {
                ViewModel.IsEditing = true;
            }
        }

        private async void DeleteProduct_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.SelectedProduct == null) return;

            ContentDialog dialog = new ContentDialog
            {
                Title = "Confirmar exclusão",
                Content = "Tem certeza que deseja excluir este produto?",
                PrimaryButtonText = "Sim",
                CloseButtonText = "Não",
                XamlRoot = this.XamlRoot
            };

            if (await dialog.ShowAsync() == ContentDialogResult.Primary)
            {
                await ViewModel.DeleteProduct();
            }
        }

        private void CancelEdit_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.IsEditing = false;
        }

        private async void SaveProduct_Click(object sender, RoutedEventArgs e)
        {
            await ViewModel.SaveProduct();
            ViewModel.IsEditing = false;
        }
    }
}
