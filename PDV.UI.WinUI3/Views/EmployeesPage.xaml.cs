using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using PDV.Domain.Entities;
using System.Linq;
using Microsoft.UI.Xaml.Navigation;
using PDV.UI.WinUI3.Views.Forms;

namespace PDV.UI.WinUI3.Views
{
    public sealed partial class EmployeesPage : Page, INotifyPropertyChanged
    {
        private ObservableCollection<Employee> AllEmployees;
        private ObservableCollection<Employee> FilteredEmployees;

        // Propriedades para controle de visibilidade
        public bool HasItems => FilteredEmployees?.Count > 0;
        public bool HasNoItems => !HasItems;

        // Implementação do INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public EmployeesPage()
        {
            this.InitializeComponent();
            LoadMockData();
            FilteredEmployees = new ObservableCollection<Employee>(AllEmployees);
            EmployeesList.ItemsSource = FilteredEmployees;

            FilteredEmployees.CollectionChanged += (s, e) =>
            {
                NotifyPropertyChanged(nameof(HasItems));
                NotifyPropertyChanged(nameof(HasNoItems));
            };
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is Employee updatedEmployee)
            {
                var existingEmployee = AllEmployees.FirstOrDefault(e => e.Id == updatedEmployee.Id);

                if (existingEmployee != null)
                {
                    int index = AllEmployees.IndexOf(existingEmployee);
                    AllEmployees[index] = updatedEmployee;
                }
                else
                {
                    AllEmployees.Add(updatedEmployee);
                }

                ApplyFilters();

                EmployeesList.SelectedItem = updatedEmployee;
            }
        }

        private void LoadMockData()
        {
            // Dados de exemplo para teste
            AllEmployees = new ObservableCollection<Employee>
            {
                new Employee
                {
                    Id = Guid.NewGuid(),
                    Name = "João Silva",
                    Email = "joao.silva@empresa.com",
                    Phone = "(11) 98765-4321",
                    Role = Domain.Enums.UserRole.Admin,
                    Position = "Gerente Geral",
                    HireDate = DateTime.Now.AddYears(-2)
                },
                new Employee
                {
                    Id = Guid.NewGuid(),
                    Name = "Maria Oliveira",
                    Email = "maria.oliveira@empresa.com",
                    Phone = "(11) 91234-5678",
                    Role = Domain.Enums.UserRole.Cashier,
                    Position = "Caixa",
                    HireDate = DateTime.Now.AddMonths(-6)
                },
                new Employee
                {
                    Id = Guid.NewGuid(),
                    Name = "Pedro Santos",
                    Email = "pedro.santos@empresa.com",
                    Phone = "(11) 99876-5432",
                    Role = Domain.Enums.UserRole.Salesperson,
                    Position = "Vendedor",
                    HireDate = DateTime.Now.AddMonths(-3)
                }
            };
        }

        private void AddEmployee_Click(object sender, RoutedEventArgs e)
        {
            // Navegar para a página de formulário em modo de adição
            Frame.Navigate(typeof(EmployeeFormPage), null, new DrillInNavigationTransitionInfo());
        }

        private void EditEmployee_Click(object sender, RoutedEventArgs e)
        {
            if (EmployeesList.SelectedItem is Employee selectedEmployee)
            {
                // Navegar para a página de formulário em modo de edição
                Frame.Navigate(typeof(EmployeeFormPage), selectedEmployee, new DrillInNavigationTransitionInfo());
            }
        }

        private async void DeleteEmployee_Click(object sender, RoutedEventArgs e)
        {
            if (EmployeesList.SelectedItem is Employee employee)
            {
                ContentDialog deleteDialog = new ContentDialog
                {
                    Title = "Confirmar exclusão",
                    Content = $"Tem certeza que deseja excluir o funcionário '{employee.Name}'?",
                    PrimaryButtonText = "Sim",
                    CloseButtonText = "Não",
                    DefaultButton = ContentDialogButton.Close,
                    XamlRoot = this.XamlRoot
                };

                var result = await deleteDialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    AllEmployees.Remove(employee);
                    FilteredEmployees.Remove(employee);
                    ClearEmployeeDetails();
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
            var selectedRole = (FilterBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            var query = AllEmployees.AsEnumerable();

            if (!string.IsNullOrEmpty(searchText))
            {
                query = query.Where(e =>
                    e.Name.ToLower().Contains(searchText) ||
                    e.Email.ToLower().Contains(searchText) ||
                    e.Phone.Contains(searchText) ||
                    e.Position.ToLower().Contains(searchText));
            }

            if (!string.IsNullOrEmpty(selectedRole) && selectedRole != "Todos")
            {
                query = query.Where(e => e.Role.ToString() == selectedRole);
            }

            FilteredEmployees.Clear();
            foreach (var employee in query.OrderBy(e => e.Name))
            {
                FilteredEmployees.Add(employee);
            }
        }

        private void EmployeesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EmployeesList.SelectedItem is Employee employee)
            {
                UpdateEmployeeDetails(employee);
            }
            else
            {
                ClearEmployeeDetails();
            }
        }

        private void UpdateEmployeeDetails(Employee employee)
        {
            NameText.Text = employee.Name;
            RoleText.Text = employee.Role.ToString();
            EmailText.Text = employee.Email;
            PhoneText.Text = employee.Phone;
            HireDateText.Text = employee.HireDate.ToString("dd/MM/yyyy");
        }

        private void ClearEmployeeDetails()
        {
            NameText.Text = "-";
            RoleText.Text = "-";
            EmailText.Text = "-";
            PhoneText.Text = "-";
            HireDateText.Text = "-";
        }
    }
}