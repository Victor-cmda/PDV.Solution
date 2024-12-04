using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml;
using PDV.Domain.Entities;
using PDV.Infrastructure.Data.Contexts;
using PDV.UI.WinUI3.Models;
using PDV.UI.WinUI3.Helpers;

namespace PDV.UI.WinUI3.ViewModels
{
    public partial class ProductViewModel : ObservableObject
    {
        private readonly SqliteDbContext _dbContext;

        [ObservableProperty]
        private ObservableCollection<Product> _products;

        [ObservableProperty]
        private Product _selectedProduct;

        [ObservableProperty]
        private string _searchText;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private bool _isEditing;

        public ProductViewModel(SqliteDbContext dbContext)
        {
            _dbContext = dbContext;
            _products = new ObservableCollection<Product>();
            _selectedProduct = new Product();

            LoadProductsCommand.ExecuteAsync(null);
        }

        [RelayCommand]
        public async Task LoadProducts()
        {
            try
            {
                IsLoading = true;
                var products = await _dbContext.Products.ToListAsync();
                Products = new ObservableCollection<Product>(products);
            }
            catch (Exception ex)
            {
                await ShowError("Erro ao carregar produtos", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task FilterProducts()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                await LoadProducts();
                return;
            }

            try
            {
                IsLoading = true;
                var searchTerm = SearchText.ToLower();
                var filtered = await _dbContext.Products
                    .Where(p => p.Name.ToLower().Contains(searchTerm) ||
                               p.Code.ToLower().Contains(searchTerm) ||
                               p.BarCode.ToLower().Contains(searchTerm))
                    .ToListAsync();

                Products = new ObservableCollection<Product>(filtered);
            }
            catch (Exception ex)
            {
                await ShowError("Erro ao filtrar produtos", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task SaveProduct()
        {
            if (SelectedProduct == null) return;

            try
            {
                IsLoading = true;

                if (SelectedProduct.Id == 0)
                {
                    _dbContext.Products.Add(SelectedProduct);
                }
                else
                {
                    _dbContext.Products.Update(SelectedProduct);
                }

                await _dbContext.SaveChangesAsync();
                await LoadProducts();

                IsEditing = false;
                await ShowSuccess("Produto salvo com sucesso!");
            }
            catch (Exception ex)
            {
                await ShowError("Erro ao salvar produto", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public async Task DeleteProduct()
        {
            if (SelectedProduct?.Id <= 0) return;

            try
            {
                var dialog = new ContentDialog
                {
                    Title = "Confirmar exclusão",
                    Content = "Deseja realmente excluir este produto?",
                    PrimaryButtonText = "Sim",
                    CloseButtonText = "Não",
                    XamlRoot = WindowHelper.MainWindow.Content.XamlRoot,
                    DefaultButton = ContentDialogButton.Close
                };

                var result = await dialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    IsLoading = true;
                    _dbContext.Products.Remove(SelectedProduct);
                    await _dbContext.SaveChangesAsync();
                    await LoadProducts();
                    await ShowSuccess("Produto excluído com sucesso!");
                }
            }
            catch (Exception ex)
            {
                await ShowError("Erro ao excluir produto", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        [RelayCommand]
        public void NewProduct()
        {
            SelectedProduct = new Product();
            IsEditing = true;
        }

        [RelayCommand]
        public void EditProduct()
        {
            if (SelectedProduct != null)
            {
                IsEditing = true;
            }
        }

        [RelayCommand]
        public void CancelEdit()
        {
            IsEditing = false;
            if (SelectedProduct?.Id == 0)
            {
                SelectedProduct = null;
            }
        }

        private async Task ShowError(string title, string message)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = WindowHelper.MainWindow.Content.XamlRoot
            };
            await dialog.ShowAsync();
        }

        private async Task ShowSuccess(string message)
        {
            var dialog = new ContentDialog
            {
                Title = "Sucesso",
                Content = message,
                CloseButtonText = "OK",
                XamlRoot = WindowHelper.MainWindow.Content.XamlRoot
            };
            await dialog.ShowAsync();
        }

        partial void OnSearchTextChanged(string value)
        {
            FilterProductsCommand.ExecuteAsync(null);
        }
    }
}
