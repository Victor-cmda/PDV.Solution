using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.EntityFrameworkCore;
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
    public sealed partial class EmployeesPage : Page
    {
        private ObservableCollection<Employee> Employees { get; set; }
        private List<Employee> AllEmployees { get; set; }

        public EmployeesPage()
        {
            this.InitializeComponent();
            LoadEmployees();
            FilterBox.SelectedIndex = 0; // Selecionar "Todos" por padrão
        }

        private void LoadEmployees()
        {
            // Dados de exemplo
            AllEmployees = new List<Employee>
            {
                new Employee
                {
                    Name = "João Silva",
                    Role = "Vendedor",
                    Email = "joao@exemplo.com",
                    Phone = "(11) 98765-4321",
                    HireDate = new DateTime(2023, 1, 15)
                },
                new Employee
                {
                    Name = "Maria Santos",
                    Role = "Gerente",
                    Email = "maria@exemplo.com",
                    Phone = "(11) 98765-4322",
                    HireDate = new DateTime(2022, 6, 1)
                },
                new Employee
                {
                    Name = "Pedro Oliveira",
                    Role = "Caixa",
                    Email = "pedro@exemplo.com",
                    Phone = "(11) 98765-4323",
                    HireDate = new DateTime(2023, 3, 10)
                }
            };

            Employees = new ObservableCollection<Employee>(AllEmployees);
            EmployeesList.ItemsSource = Employees;
        }

        private void EmployeesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EmployeesList.SelectedItem is Employee employee)
            {
                NameText.Text = employee.Name;
                RoleText.Text = employee.Role;
                EmailText.Text = employee.Email;
                PhoneText.Text = employee.Phone;
                HireDateText.Text = employee.HireDate.ToShortDateString();
            }
            else
            {
                ClearDetails();
            }
        }

        private void ClearDetails()
        {
            NameText.Text = "-";
            RoleText.Text = "-";
            EmailText.Text = "-";
            PhoneText.Text = "-";
            HireDateText.Text = "-";
        }

        private async void AddEmployee_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new EmployeeDialog();
            dialog.XamlRoot = this.XamlRoot;

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                AllEmployees.Add(dialog.Employee);
                ApplyFilters();
            }
        }

        private async void EditEmployee_Click(object sender, RoutedEventArgs e)
        {
            if (EmployeesList.SelectedItem is Employee selectedEmployee)
            {
                var dialog = new EmployeeDialog(selectedEmployee);
                dialog.XamlRoot = this.XamlRoot;

                var result = await dialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    var index = AllEmployees.IndexOf(selectedEmployee);
                    AllEmployees[index] = dialog.Employee;
                    ApplyFilters();
                }
            }
            else
            {
                var dialog = new ContentDialog
                {
                    Title = "Aviso",
                    Content = "Selecione um funcionário para editar",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };

                await dialog.ShowAsync();
            }
        }

        private async void DeleteEmployee_Click(object sender, RoutedEventArgs e)
        {
            if (EmployeesList.SelectedItem is Employee selectedEmployee)
            {
                ContentDialog dialog = new ContentDialog
                {
                    Title = "Confirmar exclusão",
                    Content = $"Tem certeza que deseja excluir o funcionário {selectedEmployee.Name}?",
                    PrimaryButtonText = "Sim",
                    CloseButtonText = "Não",
                    DefaultButton = ContentDialogButton.Close,
                    XamlRoot = this.XamlRoot
                };

                var result = await dialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    AllEmployees.Remove(selectedEmployee);
                    ApplyFilters();
                    ClearDetails();
                }
            }
            else
            {
                var dialog = new ContentDialog
                {
                    Title = "Aviso",
                    Content = "Selecione um funcionário para excluir",
                    CloseButtonText = "OK",
                    XamlRoot = this.XamlRoot
                };

                await dialog.ShowAsync();
            }
        }

        private void RefreshList_Click(object sender, RoutedEventArgs e)
        {
            LoadEmployees();
            ClearDetails();
            SearchBox.Text = string.Empty;
            FilterBox.SelectedIndex = 0;
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
            var filteredEmployees = AllEmployees.AsEnumerable();

            // Aplicar filtro de busca
            var searchText = SearchBox.Text?.ToLower() ?? string.Empty;
            if (!string.IsNullOrEmpty(searchText))
            {
                filteredEmployees = filteredEmployees.Where(emp =>
                    emp.Name.ToLower().Contains(searchText) ||
                    emp.Email.ToLower().Contains(searchText) ||
                    emp.Phone.Contains(searchText));
            }

            // Aplicar filtro de cargo
            var selectedRole = (FilterBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            if (selectedRole != null && selectedRole != "Todos")
            {
                filteredEmployees = filteredEmployees.Where(emp => emp.Role == selectedRole);
            }

            // Atualizar lista
            Employees.Clear();
            foreach (var employee in filteredEmployees)
            {
                Employees.Add(employee);
            }

            // Se não houver itens selecionados após o filtro, limpar os detalhes
            if (EmployeesList.SelectedItem == null)
            {
                ClearDetails();
            }
        }
    }
}
