using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using PDV.Domain.Entities;
using PDV.Domain.Enums;
using PDV.UI.WinUI3.Services;
using PDV.UI.WinUI3.Views.Forms;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace PDV.UI.WinUI3.Views
{
    public sealed partial class EmployeesPage : Page, INotifyPropertyChanged
    {
        private ObservableCollection<Employee> AllEmployees;
        private ObservableCollection<Employee> _filteredEmployees;
        private Employee _selectedEmployee;
        private bool _isLoading;
        private string _sortOption = "NameAsc";

        // Propriedades observáveis
        public ObservableCollection<Employee> FilteredEmployees
        {
            get => _filteredEmployees;
            set
            {
                _filteredEmployees = value;
                NotifyPropertyChanged(nameof(FilteredEmployees));
                NotifyPropertyChanged(nameof(HasItems));
                NotifyPropertyChanged(nameof(HasNoItems));
            }
        }

        public Employee SelectedEmployee
        {
            get => _selectedEmployee;
            set
            {
                _selectedEmployee = value;
                NotifyPropertyChanged(nameof(SelectedEmployee));
                NotifyPropertyChanged(nameof(HasSelectedEmployee));
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                NotifyPropertyChanged(nameof(IsLoading));
            }
        }

        // Propriedades de estado
        public bool HasItems => FilteredEmployees?.Count > 0;
        public bool HasNoItems => !HasItems;
        public bool HasSelectedEmployee => SelectedEmployee != null;

        // Implementação do INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public EmployeesPage()
        {
            this.InitializeComponent();

            // Inicializar coleções
            AllEmployees = new ObservableCollection<Employee>();
            FilteredEmployees = new ObservableCollection<Employee>();

            // Configurar os comboboxes
            SetupComboBoxes();

            // Carregar dados
            LoadEmployeesAsync();

            // Mostrar dica de recursos (opcional)
            this.Loaded += (s, e) =>
            {
                // Verificar se é a primeira vez que o usuário abre a página
                if (AppSettings.GetValue<bool>("FirstTimeEmployeesPage", true))
                {
                    FeatureTip.IsOpen = true;
                    AppSettings.SetValue("FirstTimeEmployeesPage", false);
                }
            };
        }

        private void SetupComboBoxes()
        {
            // Configurar combobox de ordenação
            SortingComboBox.SelectedIndex = 0; // Nome (A-Z) por padrão

            // Configurar combobox de filtro de status
            StatusFilterBox.SelectedIndex = 0; // Todos por padrão

            // Configurar combobox de filtro de cargo
            FilterBox.SelectedIndex = 0; // Todos por padrão
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is Employee updatedEmployee)
            {
                var existingEmployee = AllEmployees.FirstOrDefault(emp => emp.Id == updatedEmployee.Id);

                if (existingEmployee != null)
                {
                    // Atualizar funcionário existente
                    int index = AllEmployees.IndexOf(existingEmployee);
                    AllEmployees[index] = updatedEmployee;
                }
                else
                {
                    // Adicionar novo funcionário
                    AllEmployees.Add(updatedEmployee);
                }

                // Aplicar filtros para atualizar a lista
                ApplyFilters();

                // Selecionar o funcionário atualizado ou adicionado
                EmployeesList.SelectedItem = updatedEmployee;
                SelectedEmployee = updatedEmployee;
            }
        }

        private async void LoadEmployeesAsync()
        {
            try
            {
                IsLoading = true;

                // Simulação de carga de dados assíncrona
                // Em um ambiente real, isso viria de um serviço
                await Task.Delay(500);

                // Carregar dados de exemplo para teste
                LoadMockData();

                // Aplicar filtros e ordenação iniciais
                ApplyFilters();

                // Selecionar o primeiro funcionário automaticamente (opcional)
                if (FilteredEmployees.Count > 0)
                {
                    EmployeesList.SelectedIndex = 0;
                    SelectedEmployee = FilteredEmployees[0];
                }
            }
            catch (Exception ex)
            {
                // Gerenciar erro
                await DialogService.ShowErrorAsync("Erro ao carregar funcionários", ex.Message);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void LoadMockData()
        {
            // Dados de exemplo expandidos para teste
            AllEmployees.Clear();

            AllEmployees.Add(new Employee
            {
                Id = Guid.NewGuid(),
                Name = "João Silva",
                Email = "joao.silva@empresa.com",
                Phone = "(11) 98765-4321",
                Role = UserRole.Admin,
                Position = "Gerente Geral",
                HireDate = DateTime.Now.AddYears(-2),
                IsActive = true,
                Username = "joao.silva",
                EmployeeCode = "EMP001",
                Permissions = new System.Collections.Generic.List<string> { "gerenciar_funcionarios", "gerenciar_produtos", "relatórios_financeiros" },
                LastLoginDate = DateTime.Now.AddDays(-1)
            });

            AllEmployees.Add(new Employee
            {
                Id = Guid.NewGuid(),
                Name = "Maria Oliveira",
                Email = "maria.oliveira@empresa.com",
                Phone = "(11) 91234-5678",
                Role = UserRole.Cashier,
                Position = "Caixa",
                HireDate = DateTime.Now.AddMonths(-6),
                IsActive = true,
                Username = "maria.oliveira",
                EmployeeCode = "EMP002",
                Permissions = new System.Collections.Generic.List<string> { "vendas", "caixa" },
                LastLoginDate = DateTime.Now.AddHours(-3)
            });

            AllEmployees.Add(new Employee
            {
                Id = Guid.NewGuid(),
                Name = "Pedro Santos",
                Email = "pedro.santos@empresa.com",
                Phone = "(11) 99876-5432",
                Role = UserRole.Salesperson,
                Position = "Vendedor",
                HireDate = DateTime.Now.AddMonths(-3),
                IsActive = true,
                Username = "pedro.santos",
                EmployeeCode = "EMP003",
                Permissions = new System.Collections.Generic.List<string> { "vendas", "consultar_produtos" },
                LastLoginDate = DateTime.Now.AddHours(-12)
            });

            // Adicionar mais funcionários para testar a rolagem
            AllEmployees.Add(new Employee
            {
                Id = Guid.NewGuid(),
                Name = "Ana Ferreira",
                Email = "ana.ferreira@empresa.com",
                Phone = "(11) 93456-7890",
                Role = UserRole.Manager,
                Position = "Gerente de Vendas",
                HireDate = DateTime.Now.AddMonths(-12),
                IsActive = true,
                Username = "ana.ferreira",
                EmployeeCode = "EMP004",
                Permissions = new System.Collections.Generic.List<string> { "gerenciar_vendas", "gerenciar_produtos", "relatórios_vendas" },
                LastLoginDate = DateTime.Now.AddDays(-3)
            });

            AllEmployees.Add(new Employee
            {
                Id = Guid.NewGuid(),
                Name = "Lucas Costa",
                Email = "lucas.costa@empresa.com",
                Phone = "(11) 95678-9012",
                Role = UserRole.Stockist,
                Position = "Estoquista",
                HireDate = DateTime.Now.AddMonths(-9),
                IsActive = false,
                TerminationDate = DateTime.Now.AddDays(-15),
                Username = "lucas.costa",
                EmployeeCode = "EMP005",
                Permissions = new System.Collections.Generic.List<string> { "gerenciar_estoque" },
                LastLoginDate = DateTime.Now.AddDays(-20)
            });

            AllEmployees.Add(new Employee
            {
                Id = Guid.NewGuid(),
                Name = "Juliana Almeida",
                Email = "juliana.almeida@empresa.com",
                Phone = "(11) 96789-0123",
                Role = UserRole.Cashier,
                Position = "Caixa",
                HireDate = DateTime.Now.AddMonths(-2),
                IsActive = true,
                Username = "juliana.almeida",
                EmployeeCode = "EMP006",
                Permissions = new System.Collections.Generic.List<string> { "vendas", "caixa" },
                LastLoginDate = DateTime.Now.AddHours(-6),
                IsLocked = true,
                FailedLoginAttempts = 3
            });
        }

        private void AddEmployee_Click(object sender, RoutedEventArgs e)
        {
            // Navegar para a página de formulário em modo de adição
            Frame.Navigate(typeof(EmployeeFormPage), null, new DrillInNavigationTransitionInfo());
        }

        private void EditEmployee_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedEmployee != null)
            {
                // Navegar para a página de formulário em modo de edição
                Frame.Navigate(typeof(EmployeeFormPage), SelectedEmployee, new DrillInNavigationTransitionInfo());
            }
        }

        private void QuickEditEmployee_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Employee employee)
            {
                // Navegar para a página de formulário em modo de edição
                Frame.Navigate(typeof(EmployeeFormPage), employee, new DrillInNavigationTransitionInfo());
            }
        }

        private async void DeleteEmployee_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedEmployee != null)
            {
                await ShowDeleteConfirmationDialog(SelectedEmployee);
            }
        }

        private async void QuickDeleteEmployee_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Employee employee)
            {
                await ShowDeleteConfirmationDialog(employee);
            }
        }

        private async Task ShowDeleteConfirmationDialog(Employee employee)
        {
            ContentDialog deleteDialog = new ContentDialog
            {
                Title = "Confirmar exclusão",
                Content = $"Tem certeza que deseja excluir o funcionário '{employee.Name}'?",
                PrimaryButtonText = "Excluir",
                CloseButtonText = "Cancelar",
                DefaultButton = ContentDialogButton.Close,
                XamlRoot = this.XamlRoot
            };

            var result = await deleteDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                // Remover funcionário
                AllEmployees.Remove(employee);
                FilteredEmployees.Remove(employee);

                // Atualizar seleção
                if (SelectedEmployee == employee)
                {
                    SelectedEmployee = null;
                }

                // Mostrar confirmação
                await DialogService.ShowInfoAsync("Funcionário excluído",
                    $"O funcionário {employee.Name} foi removido com sucesso.");
            }
        }

        private void RefreshList_Click(object sender, RoutedEventArgs e)
        {
            // Recarregar dados e aplicar filtros
            LoadEmployeesAsync();
        }

        private void SearchBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                ApplyFilters();
            }
        }

        private void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            ApplyFilters();
        }

        private void FilterBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void StatusFilterBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ApplyFilters();
        }

        private void SortingComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SortingComboBox.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag is string sortOption)
            {
                _sortOption = sortOption;
                ApplyFilters();
            }
        }

        private void ApplyFilters()
        {
            var searchText = SearchBox.Text?.ToLower() ?? "";
            var selectedRole = (FilterBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            var selectedStatus = (StatusFilterBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            // Aplicar filtros
            var query = AllEmployees.AsEnumerable();

            // Filtrar por texto de busca
            if (!string.IsNullOrEmpty(searchText))
            {
                query = query.Where(e =>
                    e.Name?.ToLower().Contains(searchText) == true ||
                    e.Email?.ToLower().Contains(searchText) == true ||
                    e.Phone?.Contains(searchText) == true ||
                    e.Position?.ToLower().Contains(searchText) == true ||
                    e.EmployeeCode?.ToLower().Contains(searchText) == true);
            }

            // Filtrar por cargo
            if (!string.IsNullOrEmpty(selectedRole) && selectedRole != "Todos")
            {
                query = query.Where(e => e.Role.ToString() == selectedRole);
            }

            // Filtrar por status
            if (!string.IsNullOrEmpty(selectedStatus) && selectedStatus != "Todos")
            {
                if (selectedStatus == "Ativos")
                {
                    query = query.Where(e => e.IsActive);
                }
                else if (selectedStatus == "Inativos")
                {
                    query = query.Where(e => !e.IsActive);
                }
                else if (selectedStatus == "Bloqueados")
                {
                    query = query.Where(e => e.IsLocked);
                }
            }

            // Aplicar ordenação
            switch (_sortOption)
            {
                case "NameAsc":
                    query = query.OrderBy(e => e.Name);
                    break;
                case "NameDesc":
                    query = query.OrderByDescending(e => e.Name);
                    break;
                case "HireDateAsc":
                    query = query.OrderBy(e => e.HireDate);
                    break;
                case "HireDateDesc":
                    query = query.OrderByDescending(e => e.HireDate);
                    break;
                case "Role":
                    query = query.OrderBy(e => e.Role).ThenBy(e => e.Name);
                    break;
                default:
                    query = query.OrderBy(e => e.Name);
                    break;
            }

            // Atualizar a lista filtrada com animação
            var newFilteredList = new ObservableCollection<Employee>(query);
            FilteredEmployees.Clear();

            foreach (var employee in newFilteredList)
            {
                FilteredEmployees.Add(employee);
            }
        }

        private void EmployeesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (EmployeesList.SelectedItem is Employee employee)
            {
                SelectedEmployee = employee;
            }
            else
            {
                SelectedEmployee = null;
            }
        }
    }
}