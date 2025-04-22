using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using Microsoft.UI.Xaml.Navigation;
using PDV.Domain.Entities;
using PDV.Domain.Enums;
using PDV.Domain.Interfaces;
using PDV.UI.WinUI3.Services;
using PDV.Application.Services;
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
        private readonly IUnitOfWork _unitOfWork;
        private readonly EmployeeService _employeeService;

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

            // Obter serviços
            var app = Microsoft.UI.Xaml.Application.Current as App;
            _unitOfWork = app?.Services.GetService(typeof(IUnitOfWork)) as IUnitOfWork;
            _employeeService = app?.Services.GetService(typeof(EmployeeService)) as EmployeeService;

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
            else
            {
                // Recarregar dados ao retornar para a página
                LoadEmployeesAsync();
            }
        }

        private async void LoadEmployeesAsync()
        {
            try
            {
                IsLoading = true;

                // Limpar lista atual
                AllEmployees.Clear();

                // Carregar funcionários do banco de dados
                var employees = await _employeeService.GetAllEmployeesAsync();
                
                // Adicionar à coleção
                foreach (var employee in employees)
                {
                    AllEmployees.Add(employee);
                }

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

        private void AddEmployee_Click(object sender, RoutedEventArgs e)
        {
            // Navegar para a página de formulário em modo de adição
            Frame.Navigate(typeof(Forms.EmployeeFormPage), null, new DrillInNavigationTransitionInfo());
        }

        private void EditEmployee_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedEmployee != null)
            {
                // Navegar para a página de formulário em modo de edição
                Frame.Navigate(typeof(Forms.EmployeeFormPage), SelectedEmployee, new DrillInNavigationTransitionInfo());
            }
        }

        private void QuickEditEmployee_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Employee employee)
            {
                // Navegar para a página de formulário em modo de edição
                Frame.Navigate(typeof(Forms.EmployeeFormPage), employee, new DrillInNavigationTransitionInfo());
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
                try
                {
                    // Remover funcionário do banco de dados
                    await _unitOfWork.Employees.DeleteAsync(employee);
                    await _unitOfWork.CompleteAsync();
                    
                    // Remover da UI
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
                catch (Exception ex)
                {
                    await DialogService.ShowErrorAsync("Erro ao excluir", 
                        $"Não foi possível excluir o funcionário: {ex.Message}");
                }
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
            // Filtrar funcionários com base nas opções selecionadas
            var query = AllEmployees.AsQueryable();

            // Aplicar filtro de busca por texto
            if (!string.IsNullOrWhiteSpace(SearchBox.Text))
            {
                string searchTerm = SearchBox.Text.ToLower();
                query = query.Where(e => 
                    e.Name.ToLower().Contains(searchTerm) || 
                    e.Email.ToLower().Contains(searchTerm) || 
                    e.Phone.ToLower().Contains(searchTerm));
            }

            // Aplicar filtro de função
            if (FilterBox.SelectedIndex > 0)
            {
                // Mapear os índices do ComboBox para os valores do enum UserRole
                UserRole selectedRole = UserRole.Salesperson;  // Valor padrão

                switch (FilterBox.SelectedIndex)
                {
                    case 1: // Vendedor
                        selectedRole = UserRole.Salesperson;
                        break;
                    case 2: // Administrador
                        selectedRole = UserRole.Admin;
                        break;
                    case 3: // Caixa
                        selectedRole = UserRole.Cashier;
                        break;
                    case 4: // Gerente
                        selectedRole = UserRole.Manager;
                        break;
                    case 5: // Estoquista
                        selectedRole = UserRole.Stockist;
                        break;
                }

                query = query.Where(e => e.Role == selectedRole);
            }

            // Aplicar filtro de status
            if (StatusFilterBox.SelectedIndex > 0)
            {
                switch (StatusFilterBox.SelectedIndex)
                {
                    case 1: // Ativos
                        query = query.Where(e => e.IsActive);
                        break;
                    case 2: // Inativos
                        query = query.Where(e => !e.IsActive);
                        break;
                    case 3: // Bloqueados
                        query = query.Where(e => e.IsLocked);
                        break;
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